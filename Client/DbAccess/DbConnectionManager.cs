using Microsoft.Data.SqlClient;
using System.Configuration;


namespace Client.DbAccess
{
        public static class DbConnectionManager
        {
            private static readonly string? _connectionString = ConfigurationManager.ConnectionStrings["ClientMachineDBContext"]?.ConnectionString
                              ?? throw new InvalidOperationException("Connection string 'ClientMachineDBContext' not found.");


            public static SqlConnection GetConnection()
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new InvalidOperationException("Connection string is not configured.");
                }

                SqlConnection connection = new(_connectionString);

                try
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }

                return connection;
            }

            public static async Task<SqlConnection> GetConnAsync()
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new InvalidOperationException("Connection string is not configured.");
                }

                SqlConnection connection = new(_connectionString);

                try
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        await connection.OpenAsync();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }

                return connection;
            }
        }
}
