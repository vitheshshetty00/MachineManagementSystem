using Microsoft.Data.SqlClient;
using Serilog;

namespace Server.DbAccess
{
    public class MachineAccess
    {
        public static void InsertMachineData(string machineName, string ip, string port, byte[] imageData)
        {
            string machineId = DbCreation.GenerateMachineId();
            string query = @"INSERT INTO MachineTableMaster (MachineId,MachineName, IP, Port, Image) OUTPUT INSERTED.MachineId
                     VALUES (@MachineId,@MachineName, @IP, @Port, @Image);";

                    SqlParameter[] parameters =
                    {

                        new SqlParameter("@MachineId", machineId),
                        new SqlParameter("@MachineName", machineName),
                        new SqlParameter("@IP", ip),
                        new SqlParameter("@Port", port),
                        new SqlParameter("@Image", imageData),
                    };
            try
            {
                string? M_id = DataBaseAccess.ExecuteScalar(query, parameters)?.ToString();
                Log.Information("Machine inserted successfully with ID: {MachineId}", M_id);
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "SQL Error occurred while inserting machine: {MachineName}, IP: {IP}, Port: {Port}", machineName, ip, port);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while inserting machine: {MachineName}, IP: {IP}, Port: {Port}", machineName, ip, port);
            }
        }

        public static int deleteMachineData(int machineId)
        {
            string query = "DELETE FROM MachineTableMaster WHERE MachineId = @machineid";
            SqlParameter[] parameters = { new SqlParameter("@machineid", machineId) };

            try
            {
                int deletedCount = DataBaseAccess.ExecuteNonQuery(query, parameters);

                if (deletedCount > 0)
                {
                    // Log success message
                    Log.Information("Machine deleted successfully. Machine ID: {MachineId}", machineId);
                }
                else
                {
                    // Log warning if no rows were affected
                    Log.Warning("No machine found with ID: {MachineId}. No rows deleted.", machineId);
                }

                return deletedCount;
            }
            catch (Exception ex)
            {
                // Log error message
                Log.Error(ex, "Error deleting machine with ID: {MachineId}", machineId);
                return -1; // Return -1 or an error-specific value
            }
        }

        public static void displayMachineData()
        {
            string query = "SELECT * from MachineTableMaster";
            SqlParameter[] parameters = { };
            SqlDataReader reader = DataBaseAccess.ExecuteReader(query, parameters);
            Console.WriteLine($"{"ID",-10} {"Machine Name",-20} {"IP",-15} {"Port",-10} {"Image (bytes)",-15} {"Timestamp",-20}");
            Console.WriteLine(new string('-', 90));

            while (reader.Read())
            {
                string? m_id = reader["MachineId"].ToString();
                string? m_name = reader["MachineName"].ToString();
                string? ip = reader["IP"].ToString();
                string? port = reader["Port"].ToString();
                byte[] img = (byte[])reader["Image"];
                string? time = reader["Creation_Time"].ToString();

                Console.WriteLine($"{m_id,-10} {m_name,-20} {ip,-15} {port,-10} {img.Length,-15} {time,-20}");
            }

        }
        public static bool IsMachineIdValid(int machineId)
        {
            string query = "SELECT COUNT(1) FROM MachineTableMaster WHERE MachineId = @MachineId";
            SqlParameter[] parameters = { new SqlParameter("@MachineId", machineId) };
            int count = Convert.ToInt32(DataBaseAccess.ExecuteScalar(query, parameters));
            return count > 0;
        }

        public static byte[] GetImageFromDatabase(int machineId)
        {
            string query = "SELECT Image FROM MachineTableMaster WHERE MachineId = @MachineId";
            byte[]? imageBytes = null;
            SqlParameter[] parameters =
                {  new SqlParameter("@MachineId", machineId)};

            object result = DataBaseAccess.ExecuteScalar(query, parameters);
            if (result != null && result != DBNull.Value)
            {
                imageBytes = (byte[])result;
            }
            return imageBytes;
        }
    }
}
