using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CutUsage.Models;

namespace CutUsage
{
    public class LayRepository
    {
        private readonly string _conn;
        public LayRepository(IConfiguration cfg)
            => _conn = cfg.GetConnectionString("DefaultConnection");

        // ---- Lookups ----

        public async Task<List<LayType>> GetLayTypesAsync()
        {
            var list = new List<LayType>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetLayTypes", conn) { CommandType = CommandType.StoredProcedure };
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
                list.Add(new LayType
                {
                    LayTypeId = (int)rdr["LayTypeId"],
                    LayTYpeName = rdr["LayTYpeName"].ToString()
                });
            return list;
        }

        public async Task<List<LayTable>> GetLayTablesAsync()
        {
            var list = new List<LayTable>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetLayTables", conn) { CommandType = CommandType.StoredProcedure };
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
                list.Add(new LayTable
                {
                    LayTableId = (int)rdr["LayTableId"],
                    LayTableName = rdr["LayTableName"].ToString()
                });
            return list;
        }
        public async Task<List<StyleM>> GetAllStyles()
        {
            var list = new List<StyleM>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetAllStyles", conn) { CommandType = CommandType.StoredProcedure };
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new StyleM
                {
                    Style = rdr["Style"].ToString(),
                    FGStyle = rdr["FGStyle"].ToString()
                });
            }
            return list;
        }

        /// <summary>
        /// Retrieves the DocketLookup list for Assign view, now including MaterialCode.
        /// </summary>
        public async Task<List<DocketLookup>> GetDocketsAsync(int layType, string style,int layId)
        {
            var list = new List<DocketLookup>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetDocketsByLayTypeAndStyle", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@LayType", layType);
            cmd.Parameters.AddWithValue("@Style", style);
            cmd.Parameters.AddWithValue("@LayId", layId);

            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new DocketLookup
                {
                    SO = rdr["SO"].ToString(),
                    DocketNo = rdr["DocketNo"].ToString(),
                    MaterialCode = rdr["MaterialCode"].ToString()   // ensure your SP returns this column
                });
            }
            return list;
        }

        // ---- Lay Master ----

        public async Task<List<LayMaster>> GetAllLaysAsync()
        {
            var list = new List<LayMaster>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetAllLays", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new LayMaster
                {
                    LayID = (int)rdr["LayID"],
                    Style = rdr["Style"].ToString(),
                    MarkerId = rdr["MarkerId"].ToString(),
                    LayType = (int)rdr["LayType"],
                    LayTable = (int)rdr["LayTable"],
                    LayDate = rdr["LayDate"] as DateTime?,
                    LayStartTime = rdr["LayStartTime"] as DateTime?,
                    LayCompleteTime = rdr["LayCompleteTime"] as DateTime?,

                    // ← NEW LINES:
                    MarkerName = rdr["MarkerName"].ToString(),
                    LayTypeName = rdr["LayTYpeName"].ToString(),
                    LayTableName = rdr["LayTableName"].ToString()
                });
            }
            return list;
        }

        public async Task<LayMaster> GetLayByIdAsync(int id)
        {
            LayMaster m = null;
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetLayById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@LayID", id);
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
                m = new LayMaster
                {
                    LayID = id,
                    MarkerId = rdr["MarkerId"].ToString(),
                    MarkerName = rdr["MarkerName"].ToString(),
                    Style = rdr["Style"].ToString(),
                    LayType = (int)rdr["LayType"],
                    LayTable = (int)rdr["LayTable"],
                    LayDate = rdr["LayDate"] as DateTime?,
                    LayStartTime = rdr["LayStartTime"] as DateTime?,
                    LayCompleteTime = rdr["LayCompleteTime"] as DateTime?
                };
            return m;
        }

        public async Task<int> InsertLayMasterAsync(LayMaster m)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_InsertLay", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MarkerId", m.MarkerId);
            cmd.Parameters.AddWithValue("@LayType", m.LayType);
            cmd.Parameters.AddWithValue("@LayTable", m.LayTable);
            cmd.Parameters.AddWithValue("@Style", m.Style);
            await conn.OpenAsync();
            var newId = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }

        public async Task UpdateLayMasterAsync(LayMaster m)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_UpdateLay", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@LayID", m.LayID);
            cmd.Parameters.AddWithValue("@LayDate", (object)m.LayDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LayStartTime", (object)m.LayStartTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LayCompleteTime", (object)m.LayCompleteTime ?? DBNull.Value);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ---- Lay Detail ----

        /// <summary>
        /// Retrieves the LayDetail rows for a given LayID, including MaterialCode.
        /// </summary>
        public async Task<List<LayDetail>> GetLayDetailsAsync(int layId)
        {
            var list = new List<LayDetail>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetLayDetails", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@LayID", layId);

            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new LayDetail
                {
                    LayID = (int)rdr["LayID"],
                    SO = rdr["SO"].ToString(),
                    DocketNo = rdr["DocketNo"].ToString(),
                    MaterialCode = rdr["MaterialCode"].ToString()   // ensure your SP returns this column
                });
            }
            return list;
        }

        public async Task InsertLayDetailAsync(LayDetail d)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_InsertLayDetail", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@LayID", d.LayID);
            cmd.Parameters.AddWithValue("@SO", d.SO ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DocketNo", d.DocketNo ?? (object)DBNull.Value);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteLayDetailAsync(LayDetail d)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_DeleteLayDetail", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@LayID", d.LayID);
            cmd.Parameters.AddWithValue("@SO", d.SO);
            cmd.Parameters.AddWithValue("@DocketNo", d.DocketNo);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // LayRepository.cs
        public async Task<string> UpsertCutUsageValuesAsync(LayRollDetailsViewModel vm)
        {
            string lastMessage = null!;
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var detail in vm.Details)
                {
                    using var cmd = new SqlCommand("spCutUsage_UpdateInsertValue", conn, tx)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@LayId", vm.Header.LayID);
                    cmd.Parameters.AddWithValue("@MarkerId", vm.Header.MarkerId);
                    cmd.Parameters.AddWithValue("@RollID", detail.SAPBatchNo);

                    cmd.Parameters.AddWithValue("@NoOfPlys", (object?)detail.NoOfPlys ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PartPly", (object?)detail.PartPly ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BindingQty", (object?)detail.BindingQty ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LayedQty", (object?)detail.LayedQty ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cat1Value", (object?)detail.Cat1Value ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cat2Value", (object?)detail.Cat2Value ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cat3Value", (object?)detail.Cat3Value ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cat4Value", (object?)detail.Cat4Value ?? DBNull.Value);

                    // OUTPUT params
                    var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                    var pMsg = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pSuccess);
                    cmd.Parameters.Add(pMsg);

                    await cmd.ExecuteNonQueryAsync();

                    var ok = (bool)pSuccess.Value!;
                    var msg = (string)pMsg.Value!;
                    if (!ok)
                        throw new InvalidOperationException(msg);

                    lastMessage = msg;
                }

                tx.Commit();
                return lastMessage!;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }



        public async Task<LayRollDetailsViewModel> GetLayRollDetailsAsync(int layID)
        {
            var vm = new LayRollDetailsViewModel();

            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // — first proc: header + roll details —
            using (var cmd = new SqlCommand("spCutUsage_GetLayRollDetails", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                cmd.Parameters.AddWithValue("@LayID", layID);
                using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    if (vm.Header.LayID == 0)
                    {
                        vm.Header = new LayRollHeader
                        {
                            LayID = int.Parse(rdr["LayID"].ToString()!),
                            MarkerId = int.Parse(rdr["MarkerId"].ToString()!),
                            LayTypeName = rdr["LayTYpeName"].ToString()!,
                            LayTableName = rdr["LayTableName"].ToString()!,
                            SO = rdr["SO"].ToString()!,       // still kept if you like
                            MarkerName = rdr["MarkerName"].ToString()!,

                            MarkerWidth = Convert.ToDecimal(rdr["MarkerWidth"]),
                            MarkerLength = Convert.ToDecimal(rdr["MarkerLength"]),
                            MarkerUsage = Convert.ToDecimal(rdr["MarkerUsage"]),

                            FGStyle = rdr["FGStyle"].ToString()!,
                            FGColor = rdr["FGColor"].ToString()!
                        };
                    }

                    vm.Details.Add(new LayRollDetail
                    {
                        MaterialCode = rdr["MaterialCode"].ToString()!,
                        VendorCode = rdr["VendorCode"].ToString()!,
                        VendorBatch = rdr["VendorBatch"].ToString()!,
                        SAPBatchNo = rdr["SAPBatchNo"].ToString()!,
                        RollNo = rdr["RollNo"].ToString()!,
                        Shade = rdr["Shade"].ToString()!,
                        MaterialDescription = rdr["MaterialDescription"].ToString()!,

                        Length = Convert.ToDecimal(rdr["Length"]),
                        NoOfPlys = rdr["NoOfPlys"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["NoOfPlys"]),
                        PartPly = rdr["PartPly"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["PartPly"]),
                        BindingQty = rdr["BindingQty"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["BindingQty"]),
                        LayedQty = rdr["LayedQty"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["LayedQty"]),

                        Cat1Value = rdr["Cat1Value"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["Cat1Value"]),
                        Cat2Value = rdr["Cat2Value"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["Cat2Value"]),
                        Cat3Value = rdr["Cat3Value"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["Cat3Value"]),
                        Cat4Value = rdr["Cat4Value"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["Cat4Value"])
                    });
                }
            }

            // — second proc: docket & SO list —
            using (var cmd2 = new SqlCommand("spCutUsage_GetLayDocketAndSODetails", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                cmd2.Parameters.AddWithValue("@LayID", layID);
                using var rdr2 = await cmd2.ExecuteReaderAsync();
                while (await rdr2.ReadAsync())
                {
                    vm.DocketDetails.Add(new LayDocketSo
                    {
                        LayID = Convert.ToInt32(rdr2["LayID"]),
                        DocketNo = rdr2["DocketNo"].ToString()!,
                        SO = rdr2["SO"].ToString()!
                    });
                }
            }

            return vm;
        }

        /// <summary>
        /// Calls spCutUsage_GetLaySODetails for a comma-list of SOs
        /// </summary>
        public async Task<List<SOSizeDetail>> GetLaySODetailsAsync(string commaSeparatedSO)
        {
            var list = new List<SOSizeDetail>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetLaySODetails", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@SO", commaSeparatedSO);
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new SOSizeDetail
                {
                    SO = rdr["SO"].ToString(),
                    SOSize = rdr["SOSize"].ToString(),
                    Qty = rdr.GetDecimal(rdr.GetOrdinal("Qty"))
                });
            }
            return list;
        }

    }
}
