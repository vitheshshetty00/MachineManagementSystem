using Microsoft.Data.SqlClient;
using static Server.DbAccess.DbConnectionManager;
using Serilog;
namespace Server.DbAccess
{
    public class DataBaseAccess
    {
        static DataBaseAccess()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
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
                Log.Error($"SQL Error: {ex.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
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
                Log.Error($"SQL Error: {ex.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
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
                Log.Error($"SQL Error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex.Message}");
                return null;
            }
            finally
            {
                conn.Close();
            }
        }            
    }
}
