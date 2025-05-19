using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using CutUsage.ViewModels;  // Ensure you have the correct using directive for your view models
using System.Collections.Generic;
using System.Threading.Tasks;
using CutUsage.Models;

namespace CutUsage
{
    public class DocketRepository
    {
        private readonly string _connectionString;

        public DocketRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<DocketDetail> GetDocketDetailsAsync(string docketNo)
        {
            DocketDetail detail = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spCutUsage_GetDocketDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DocketNo", docketNo);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            detail = new DocketDetail
                            {
                                DocketNo = reader["DocketNo"].ToString(),
                                SO = reader["SO"].ToString(),
                                PrdOrder = reader["PrdOrder"].ToString(),
                                CustomerStyle = reader["CustomerStyle"].ToString(),
                                FGStyle = reader["FGStyle"].ToString(),
                                FGColor = reader["FGColor"].ToString(),
                                Qty = Convert.ToDecimal(reader["Qty"]),
                                SpecWidth = reader["SpecWidth"] != DBNull.Value ? Convert.ToDecimal(reader["SpecWidth"]) : 0,
                                BOMUsage = reader["BOMUsage"] != DBNull.Value ? Convert.ToDecimal(reader["BOMUsage"]) : 0,
                                MarkerName = reader["MarkerName"].ToString(),
                                MarkerWidth = reader["MarkerWidth"] != DBNull.Value ? Convert.ToDecimal(reader["MarkerWidth"]) : 0,
                                MarkerUsage = reader["GerberMarkerUsage"] != DBNull.Value ? Convert.ToDecimal(reader["GerberMarkerUsage"]) : 0,
                                NoOfPlys = reader["NoOfPlys"] != DBNull.Value ? Convert.ToInt32(reader["NoOfPlys"]) : 0,
                                
                            };
                        }
                    }
                }
            }
            return detail;
        }

        // New method: Retrieve usage roles for a given docket number.
        // 2) Updated C# method to call the SP
        public async Task<List<UsageRoleViewModel>> GetUsageRoleDetailsAsync(string docketNo)
        {
            var usageRoles = new List<UsageRoleViewModel>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("spCutUsage_GetUsageRoleDetails", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DocketNo", docketNo);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        usageRoles.Add(new UsageRoleViewModel
                        {
                            DocketNo = reader["DocketNo"].ToString(),
                            SystemBatch = reader["SystemBatch"].ToString(),
                            Supplier = reader["Supplier"].ToString(),
                            Material = reader["Material"].ToString(),
                            Composition = reader["Composition"].ToString(),
                            Qty = reader["Qty"] != DBNull.Value
                                          ? Convert.ToDecimal(reader["Qty"])
                                          : 0,
                            Shade = reader["Shade"].ToString(),
                            Cat1Value = reader["Cat1Value"] != DBNull.Value
                                          ? (decimal?)Convert.ToDecimal(reader["Cat1Value"])
                                          : null,
                            Cat2Value = reader["Cat2Value"] != DBNull.Value
                                          ? (decimal?)Convert.ToDecimal(reader["Cat2Value"])
                                          : null,
                            Cat3Value = reader["Cat3Value"] != DBNull.Value
                                          ? (decimal?)Convert.ToDecimal(reader["Cat3Value"])
                                          : null,
                            Cat4Value = reader["Cat4Value"] != DBNull.Value
                                          ? (decimal?)Convert.ToDecimal(reader["Cat4Value"])
                                          : null
                        });
                    }
                }
            }

            return usageRoles;
        }


        public async Task InsertCatValuesAsync(CatValueModel model)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spCutUsage_InsertCatValues", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DocketNo", model.DocketNo);
                    cmd.Parameters.AddWithValue("@SystemBatch", string.IsNullOrEmpty(model.SystemBatch) ? (object)DBNull.Value : model.SystemBatch);
                    cmd.Parameters.AddWithValue("@Cat1Value", model.Cat1Value.HasValue ? (object)model.Cat1Value.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cat2Value", model.Cat2Value.HasValue ? (object)model.Cat2Value.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cat3Value", model.Cat3Value.HasValue ? (object)model.Cat3Value.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cat4Value", model.Cat4Value.HasValue ? (object)model.Cat4Value.Value : DBNull.Value);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            bool isValid = false;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spValidateUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);  // In production, hash the password first.

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    isValid = (result != null && Convert.ToInt32(result) > 0);
                }
            }
            return isValid;
        }
    }
}
