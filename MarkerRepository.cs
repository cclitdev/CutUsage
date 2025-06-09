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
                    MarkerId = rdr["MarkerId"].ToString(),
                    MarkerName = rdr["MarkerName"].ToString(),
                    MarkerWidth = decimal.Parse(rdr["MarkerWidth"].ToString()),
                    MarkerLength = decimal.Parse(rdr["MarkerLength"].ToString()),
                    MarkerUsage = (decimal)rdr["MarkerUsage"]
                });
            }
            return list;
        }

     

        public async Task<Marker> GetByIdAsync(string id)
        {
            Marker m = null;
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_GetMarkerById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
                m = new Marker
                {
                    MarkerId = id,
                    MarkerName = rdr["MarkerName"].ToString(),
                    MarkerWidth = decimal.Parse(rdr["MarkerWidth"].ToString()),
                    MarkerLength = decimal.Parse(rdr["MarkerLength"].ToString()),
                    MarkerUsage = (decimal)rdr["MarkerUsage"]
                };
            return m;
        }

        public async Task<string> CreateAsync(Marker marker)
        {
            using (var conn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand("spCutUsage_InsertMarker", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MarkerName", marker.MarkerName);
                cmd.Parameters.AddWithValue("@MarkerWidth", marker.MarkerWidth);
                cmd.Parameters.AddWithValue("@MarkerLength", marker.MarkerLength);
                cmd.Parameters.AddWithValue("@MarkerUsage", marker.MarkerUsage);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();

                marker.MarkerId = result?.ToString(); // ✅ Assign the generated MarkerId (string GUID)
                return marker.MarkerId;
            }
        }

        public async Task UpdateAsync(Marker marker)
        {
            using (var conn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand("spCutUsage_UpdateMarker", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@MarkerID", marker.MarkerId); // string
                cmd.Parameters.AddWithValue("@MarkerName", marker.MarkerName);
                cmd.Parameters.AddWithValue("@MarkerWidth", marker.MarkerWidth);
                cmd.Parameters.AddWithValue("@MarkerLength", marker.MarkerLength);
                cmd.Parameters.AddWithValue("@MarkerUsage", marker.MarkerUsage);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync(); // error was here
            }
        }


        public async Task DeleteAsync(string id)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("spCutUsage_DeleteMarker", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MarkerId", id);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
