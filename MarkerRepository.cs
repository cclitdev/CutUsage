using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CutUsage.Models;

namespace CutUsage
{
    public class MarkerRepository
    {
        private readonly string _conn;
        public MarkerRepository(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Marker>> GetAllAsync()
        {
            var list = new List<Marker>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetAllMarkers", conn) { CommandType = CommandType.StoredProcedure };
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new Marker
                {
                    MarkerId = (int)rdr["MarkerId"],
                    MarkerName = rdr["MarkerName"].ToString(),
                    MarkerWidth = (decimal)rdr["MarkerWidth"],
                    MarkerLength = (decimal)rdr["MarkerLength"],
                    MarkerUsage = (decimal)rdr["MarkerUsage"]
                });
            }
            return list;
        }

        public async Task<Marker> GetByIdAsync(int id)
        {
            Marker m = null;
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetMarkerById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MarkerId", id);
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
                m = new Marker
                {
                    MarkerId = id,
                    MarkerName = rdr["MarkerName"].ToString(),
                    MarkerWidth = (decimal)rdr["MarkerWidth"],
                    MarkerLength = (decimal)rdr["MarkerLength"],
                    MarkerUsage = (decimal)rdr["MarkerUsage"]
                };
            return m;
        }

        public async Task<int> CreateAsync(Marker m)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_InsertMarker", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MarkerName", m.MarkerName);
            cmd.Parameters.AddWithValue("@MarkerWidth", m.MarkerWidth);
            cmd.Parameters.AddWithValue("@MarkerLength", m.MarkerLength);
            cmd.Parameters.AddWithValue("@MarkerUsage", m.MarkerUsage);
            await conn.OpenAsync();
            var newId = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }

        public async Task UpdateAsync(Marker m)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_UpdateMarker", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MarkerId", m.MarkerId);
            cmd.Parameters.AddWithValue("@MarkerName", m.MarkerName);
            cmd.Parameters.AddWithValue("@MarkerWidth", m.MarkerWidth);
            cmd.Parameters.AddWithValue("@MarkerLength", m.MarkerLength);
            cmd.Parameters.AddWithValue("@MarkerUsage", m.MarkerUsage);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_DeleteMarker", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MarkerId", id);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
