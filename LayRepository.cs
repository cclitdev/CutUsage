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

        public async Task<List<DocketLookup>> GetDocketsAsync(int layType)
        {
            var list = new List<DocketLookup>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetDocketsByLayType", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@LayType", layType);
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
                list.Add(new DocketLookup
                {
                    DocketNo = rdr["DocketNo"].ToString(),
                    SO = rdr["SO"].ToString()
                });
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
                    MarkerId = (int)rdr["MarkerId"],
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
                    MarkerId = (int)rdr["MarkerId"],
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

        public async Task<List<LayDetail>> GetLayDetailsAsync(int layId)
        {
            var list = new List<LayDetail>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetLayDetails", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@LayID", layId);
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
                list.Add(new LayDetail
                {
                    LayID = (int)rdr["LayID"],
                    SO = rdr["SO"].ToString(),
                    DocketNo = rdr["DocketNo"].ToString()
                });
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

        public async Task<LayRollDetailsViewModel> GetLayRollDetailsAsync(int layID)
        {
            var vm = new LayRollDetailsViewModel();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetLayRollDetails", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@LayID", layID);

            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                // Populate header once
                if (vm.Header.LayID == 0)
                {
                    vm.Header = new LayRollHeader
                    {
                        LayID = (int)rdr["LayID"],
                        MarkerId = (int)rdr["MarkerId"],
                        LayTypeName = rdr["LayTYpeName"].ToString()!,
                        LayTableName = rdr["LayTableName"].ToString()!,
                        SO = rdr["SO"].ToString()!,
                        MarkerName = rdr["MarkerName"].ToString()!,

                        // These fields may be INT or DECIMAL in SQL:
                        MarkerWidth = Convert.ToDecimal(rdr["MarkerWidth"]),
                        MarkerLength = Convert.ToDecimal(rdr["MarkerLength"]),
                        MarkerUsage = Convert.ToDecimal(rdr["MarkerUsage"]),

                        FGStyle = rdr["FGStyle"].ToString()!,
                        FGColor = rdr["FGColor"].ToString()!
                    };
                }

                // Populate each detail row
                var detail = new LayRollDetail
                {
                    MaterialCode = rdr["MaterialCode"].ToString()!,
                    VendorCode = rdr["VendorCode"].ToString()!,
                    VendorBatch = rdr["VendorBatch"].ToString()!,
                    SAPBatchNo = rdr["SAPBatchNo"].ToString()!,
                    RollNo = rdr["RollNo"].ToString()!,
                    Shade = rdr["Shade"].ToString()!,
                    MaterialDescription = rdr["MaterialDescription"].ToString()!,

                    // Convert length (INT or DECIMAL) to decimal
                    Length = Convert.ToDecimal(rdr["Length"]),

                    // The value columns might be NULL, so check first
                    NoOfPlys = rdr["NoOfPlys"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["NoOfPlys"]),
                    PartPly = rdr["PartPly"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["PartPly"]),
                    BindingQty = rdr["BindingQty"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["BindingQty"]),

                    Cat1Value = rdr["Cat1Value"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["Cat1Value"]),
                    Cat2Value = rdr["Cat2Value"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["Cat2Value"]),
                    Cat3Value = rdr["Cat3Value"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["Cat3Value"]),
                    Cat4Value = rdr["Cat4Value"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(rdr["Cat4Value"])
                };

                vm.Details.Add(detail);
            }

            return vm;
        }


    }
}
