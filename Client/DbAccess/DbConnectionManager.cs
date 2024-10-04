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

        public static int ExecuteNonQuery(string query, SqlParameter[]? parameters)
        {
            SqlConnection conn = GetConnection();
            try
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return -1;
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
