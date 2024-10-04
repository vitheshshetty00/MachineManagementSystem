using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DbAccess
{
    public class MachineAccess
    {
        public static void InsertMachineData(string machineName, string ip, string port, string base64Image)
        {
            byte[] imageData = Convert.FromBase64String(base64Image);
            string query = @"INSERT INTO MachineTableMaster (MachineName, IP, Port, Image) OUTPUT INSERTED.MachineId
                         VALUES (@MachineName, @IP, @Port, @Image);";

            SqlParameter[] parameters =
            {
            new SqlParameter("@MachineName", machineName),
            new SqlParameter("@IP", ip),
            new SqlParameter("@Port", port),
            new SqlParameter("@Image", imageData),
            };
            int M_id = Convert.ToInt32(DataBaseAccess.ExecuteScalar(query, parameters));
            Console.WriteLine($"Machine inserted with ID:{M_id}");
        }
        public static int deleteMachineData(int machineId)
        {
            string query = "DELETE FROM MachineTableMaster WHERE MachineId = @machineid";
            SqlParameter[] parameters = { new SqlParameter("@machineid", machineId) };
            return DataBaseAccess.ExecuteNonQuery(query, parameters);
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
                string? time = reader["Timestamp"].ToString();

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
