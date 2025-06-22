using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public async Task<int> CreateAsync(MarkerPlanCreateViewModel vm)
        {
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
                    cmdHeader.Parameters.AddWithValue("@Style", vm.Style);
                    var pId = new SqlParameter("@PlanId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmdHeader.Parameters.Add(pId);
                    await cmdHeader.ExecuteNonQueryAsync();
                    planId = (int)pId.Value;
                }

                // 2) Insert each detail row
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

                // 3) Insert Lay and LayDetail per distinct marker
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
                        cmdLay.Parameters.AddWithValue("@Style", vm.Style);
                        // Proc returns NewLayID via SELECT SCOPE_IDENTITY()
                        layId = Convert.ToInt32(await cmdLay.ExecuteScalarAsync());
                    }

                    // 3b) Insert into LayDetail (LayD)
                    var distinctDockets = grp.Select(x => x.DocketNo).Distinct();
                    foreach (var docket in distinctDockets)
                    {
                        // fetch SO via stored procedure spCutUsage_GetSOByDocket
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

                // 4) Insert marker summary into MarkerM via spCutUsage_InsertMarker
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

            // Call stored procedure instead of inline SQL
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("spCutUsage_GetExistingCutBySO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            // pass the SOs as a comma-separated list
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
