using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using WebApi.Interfaces;

namespace WebApi.Models
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task InsertMultipleProductsAsync(List<ProductInputModel> products)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("InsertMultipleProducts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    var productDataJson = JsonConvert.SerializeObject(products);
                    cmd.Parameters.Add(new SqlParameter("@ProductData", SqlDbType.NVarChar) { Value = productDataJson });

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("GetProductById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productId });

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Product
                            {
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                Price_Value = reader.GetDecimal(reader.GetOrdinal("Price_Value")),
                                Currency_Code = reader.GetString(reader.GetOrdinal("Currency_Code"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("GetProductByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProductID", DBNull.Value));

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        var products = new List<Product>();
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                Price_Value = reader.GetDecimal(reader.GetOrdinal("Price_Value")),
                                Currency_Code = reader.GetString(reader.GetOrdinal("Currency_Code")),
                            });
                        }
                        return products;
                    }
                }
            }
        }

    }
}
