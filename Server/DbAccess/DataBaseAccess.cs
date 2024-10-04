using Microsoft.Data.SqlClient;
using static Server.DbAccess.DbConnectionManager;

namespace Server.DbAccess
{
    public class DataBaseAccess
    {
        
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
        public static object ExecuteScalar(string query, SqlParameter[] parameters)
        {
            SqlConnection conn = GetConnection();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteScalar();
                }
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
        public static SqlDataReader ExecuteReader(string query, SqlParameter[] parameters)
        {
            SqlConnection conn = GetConnection();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                }
            }
            catch (SqlException ex) {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
            finally
            {
                conn.Close();
            }
        }            
    }
}
