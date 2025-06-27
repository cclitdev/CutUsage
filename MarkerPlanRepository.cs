using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CutUsage.Models;

namespace CutUsage
{
    public class MarkerPlanRepository
    {
        private readonly string _conn;
        public MarkerPlanRepository(IConfiguration cfg)
            => _conn = cfg.GetConnectionString("DefaultConnection");

        /// <summary>
        /// Creates a new marker plan and related lays and markers in the database.
        /// Supports multiple styles by joining them into a comma-delimited string.
        /// </summary>
        public async Task<int> CreateAsync(MarkerPlanCreateViewModel vm)
        {
            // Join selected styles (if any) into a CSV for the stored proc
            var styleCsv = vm.SelectedStyles != null && vm.SelectedStyles.Any()
                ? string.Join(",", vm.SelectedStyles)
                : string.Empty;

            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            try
            {
                // 1) Insert marker-plan header
                int planId;
                using (var cmdHeader = new SqlCommand("spCutUsage_InsertMarkerPlan", conn, tx))
                {
                    cmdHeader.CommandType = CommandType.StoredProcedure;
                    cmdHeader.Parameters.AddWithValue("@Style", styleCsv);
                    var pId = new SqlParameter("@PlanId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmdHeader.Parameters.Add(pId);
                    await cmdHeader.ExecuteNonQueryAsync();
                    planId = (int)pId.Value;
                }

                // 2) Insert each detail row for the marker plan
                foreach (var d in vm.Details)
                {
                    using var cmdDetail = new SqlCommand("spCutUsage_InsertMarkerPlanD", conn, tx)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmdDetail.Parameters.AddWithValue("@PlanId", planId);
                    cmdDetail.Parameters.AddWithValue("@DocketNo", d.DocketNo);
                    cmdDetail.Parameters.AddWithValue("@Size", d.Size);
                    cmdDetail.Parameters.AddWithValue("@Qty", d.Qty);
                    cmdDetail.Parameters.AddWithValue("@ExistingCutQty", d.ExistingCutQty);
                    cmdDetail.Parameters.AddWithValue("@MaterialCode", d.MaterialCode);
                    cmdDetail.Parameters.AddWithValue("@BOMUsage", d.BOMUsage);
                    cmdDetail.Parameters.AddWithValue("@NoOfPlies", d.NoOfPlies);
                    cmdDetail.Parameters.AddWithValue("@FabricRequirement", d.FabricRequirement);
                    cmdDetail.Parameters.AddWithValue("@MarkerUsage", d.MarkerUsage);
                    cmdDetail.Parameters.AddWithValue("@MarkerSaving", d.MarkerSaving);
                    cmdDetail.Parameters.AddWithValue("@TargetLength", d.TargetLength);
                    cmdDetail.Parameters.AddWithValue("@MarkerName", d.MarkerName);
                    cmdDetail.Parameters.AddWithValue("@MarkerLength", d.MarkerLength);
                    cmdDetail.Parameters.AddWithValue("@MarkerWidth", d.MarkerWidth);
                    cmdDetail.Parameters.AddWithValue("@Allowance", d.Allowance);
                    await cmdDetail.ExecuteNonQueryAsync();
                }

                // 3) For each distinct marker name, create a Lay (LayM) and LayDetail (LayD)
                var groupedByMarker = vm.Details.GroupBy(x => x.MarkerName);
                foreach (var grp in groupedByMarker)
                {
                    var markerName = grp.Key;
                    int layId;

                    // 3a) Insert into Lay (LayM)
                    using (var cmdLay = new SqlCommand("spCutUsage_InsertLay", conn, tx))
                    {
                        cmdLay.CommandType = CommandType.StoredProcedure;
                        cmdLay.Parameters.AddWithValue("@MarkerId", markerName);
                        cmdLay.Parameters.AddWithValue("@LayType", "1");
                        cmdLay.Parameters.AddWithValue("@LayTable", "1");
                        cmdLay.Parameters.AddWithValue("@Style", styleCsv);
                        // stored proc returns new LayID via SELECT SCOPE_IDENTITY()
                        layId = Convert.ToInt32(await cmdLay.ExecuteScalarAsync());
                    }

                    // 3b) Insert into LayDetail (LayD) for each distinct docket in this marker
                    var distinctDockets = grp.Select(x => x.DocketNo).Distinct();
                    foreach (var docket in distinctDockets)
                    {
                        // retrieve SO by docket via stored proc
                        string so;
                        using (var cmdSo = new SqlCommand("spCutUsage_GetSOByDocket", conn, tx))
                        {
                            cmdSo.CommandType = CommandType.StoredProcedure;
                            cmdSo.Parameters.AddWithValue("@DocketNo", docket);
                            var pSo = new SqlParameter("@SO", SqlDbType.VarChar, 50)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmdSo.Parameters.Add(pSo);
                            await cmdSo.ExecuteNonQueryAsync();
                            so = (string)(pSo.Value ?? string.Empty);
                        }

                        using var cmdLayD = new SqlCommand("spCutUsage_InsertLayDetail", conn, tx)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmdLayD.Parameters.AddWithValue("@LayID", layId);
                        cmdLayD.Parameters.AddWithValue("@SO", so);
                        cmdLayD.Parameters.AddWithValue("@DocketNo", docket);
                        await cmdLayD.ExecuteNonQueryAsync();
                    }
                }

                // 4) Insert marker summary into MarkerM
                foreach (var grp in groupedByMarker)
                {
                    var first = grp.First();
                    using var cmdMarker = new SqlCommand("spCutUsage_InsertMarker", conn, tx)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmdMarker.Parameters.AddWithValue("@MarkerName", first.MarkerName);
                    cmdMarker.Parameters.AddWithValue("@MarkerWidth", first.MarkerWidth);
                    cmdMarker.Parameters.AddWithValue("@MarkerLength", first.MarkerLength);
                    cmdMarker.Parameters.AddWithValue("@MarkerUsage", first.MarkerUsage);
                    cmdMarker.Parameters.AddWithValue("@Allowance", first.Allowance);
                    await cmdMarker.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return planId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Aggregates existing MarkerPlanDetail.Qty by size for the given SO list
        /// by joining LayD -> LayM -> MarkerPlanDetail.
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetExistingCutBySOAsync(IEnumerable<string> soList)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            if (soList == null || !soList.Any())
                return result;

            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("spCutUsage_GetExistingCutBySO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@SOList", string.Join(",", soList));

            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var size = rdr.GetString(0);
                var qty = rdr.GetDecimal(1);
                result[size] = qty;
            }

            return result;
        }
    }
}
