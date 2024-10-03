using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace Server.DbAccess
{
    public class DataBaseAccess
    {
        private static string cs = ConfigurationManager.ConnectionStrings["dbcs"].ConnectionString;
        private static SqlConnection conn = new SqlConnection(cs);

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(cs);
        }
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
        public static object ExecuteScalar(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }
        public static SqlDataReader ExecuteReader(string query, SqlParameter[] parameters)
        {
            SqlConnection conn = GetConnection();
            SqlCommand cmd = new SqlCommand(query, conn);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            conn.Open();
            return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
        public static void InsertMachineData(string machineName, string ip, string port, string base64Image)
        {
            byte[] imageData = Convert.FromBase64String(base64Image);
            //Console.WriteLine("Maybe...");
            string query = @"INSERT INTO MachineTableMaster (MachineName, IP, Port, Image) OUTPUT INSERTED.MachineId
                         VALUES (@MachineName, @IP, @Port, @Image);";

            SqlParameter[] parameters =
            {
            new SqlParameter("@MachineName", machineName),
            new SqlParameter("@IP", ip),
            new SqlParameter("@Port", port),
            new SqlParameter("@Image", imageData),
            };
            int M_id = Convert.ToInt32(ExecuteScalar(query, parameters));
            Console.WriteLine($"Machine inserted with ID:{M_id}");
        }
        public static int deleteMachineData(int machineId)
        {
            string query = "DELETE FROM MachineTableMaster WHERE MachineId = @machineid";
            SqlParameter[] parameters = { new SqlParameter("@machineid", machineId) };
            return ExecuteNonQuery(query, parameters);
        }
        public static void displayMachineData()
        {
            string query = "SELECT * from MachineTableMaster";
            SqlParameter[] parameters = { };
            SqlDataReader reader = ExecuteReader(query, parameters);

            // Print header row with formatted columns
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
            int count = Convert.ToInt32(ExecuteScalar(query, parameters));
            return count > 0;
        }
        public static int InsertTransactionData(int machineID, string eventInput, string statusInput)
        {
            string query = "INSERT INTO TransactionTableMaster(M_Id,Event,Status) OUtput Inserted.TransactionId VALUES(@m_id,@event,@status)";
            SqlParameter[] parameters = {
                new SqlParameter("@m_id",machineID),
                new SqlParameter("@event",eventInput),
                new SqlParameter("status",statusInput)};
            return Convert.ToInt32(ExecuteScalar(query, parameters));
        }
        public static int deleteTransactionData(int t_id)
        {

            string query = "DELETE FROM TransactionTableMaster WHERE TransactionId = @t_id";
            SqlParameter[] parameters = { new SqlParameter("@t_id", t_id) };
            return ExecuteNonQuery(query, parameters);
        }
        public static void displayTransactionData()
        {
            string query = "SELECT * from TransactionTableMaster";
            SqlParameter[] parameters = { };
            SqlDataReader reader = ExecuteReader(query, parameters);

            // Print header row with formatted columns
            Console.WriteLine($"{"Transaction ID",-15} {"Machine ID",-12} {"Event",-20} {"Timestamp",-15} {"Status",-10}");
            Console.WriteLine(new string('-', 75)); // Separator line for clarity

            while (reader.Read())
            {
                string t_id = reader.GetInt32(0).ToString();
                string m_id = reader.GetInt32(1).ToString();
                string eventName = reader.GetString(2);
                string timestamp = reader.GetDateTime(3).ToShortDateString(); // Date formatting
                string status = reader.GetString(4);

                // Print each row with the same formatting as the header
                Console.WriteLine($"{t_id,-15} {m_id,-12} {eventName,-20} {timestamp,-15} {status,-10}");
            }

        }

        public static bool IsUserAdmin(int userId)
        {
            string query = "SELECT IsAdmin FROM UserTableMaster WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };

            object result = ExecuteScalar(query, parameters);

            // Assuming IsAdmin is of type BIT
            return result != null && Convert.ToInt32(result) == 1;
        }

        public static bool IsUserTableEmpty()
        {
            string query = "SELECT COUNT(*) FROM UserTableMaster";
            object result = ExecuteScalar(query, null);
            return (int)(result ?? 0) == 0; // If count is 0, table is empty
        }


        public static int InsertIntoUserMasterTable(string username, string password, string email, int isAdmin)
        {
            string query = "INSERT INTO UserTableMaster(UserName,Password,Email,IsAdmin) OUTPUT INSERTED.UserId VALUES (@username,@password,@email,@isAdmin)";
            SqlParameter[] parameters = {
                new SqlParameter("@username",username),
                new SqlParameter("@email",email),
                new SqlParameter("@password",password),
                new SqlParameter("@isAdmin", isAdmin )};
            return Convert.ToInt32(ExecuteScalar(query, parameters));
        }

        public static int DeleteUserMasterTable(int u_id)
        {
            string query = "DELETE FROM UserTableMaster WHERE UserId = @u_id";
            SqlParameter[] parameters = { new SqlParameter("@u_id", u_id) };
            return ExecuteNonQuery(query, parameters);

        }
        public static byte[] GetImageFromDatabase(int machineId)
        {
            string query = "SELECT Image FROM MachineTableMaster WHERE MachineId = @MachineId";
            byte[]? imageBytes = null;
            SqlParameter[] parameters =
                {  new SqlParameter("@MachineId", machineId)};

            object result = ExecuteScalar(query, parameters);
            if (result != null && result != DBNull.Value)
            {
                imageBytes = (byte[])result;  // Cast the result to a byte array
            }
            return imageBytes;
        }

        public static void displayUserMasterTable()
        {

            string query = "SELECT * from UserTableMaster";
            SqlParameter[] parameters = { };
            SqlDataReader reader = ExecuteReader(query, parameters);

            // Print header row with formatted columns
            Console.WriteLine($"{"User Id",-10} {"User Name",-20} {"Email",-30} {"User Type",-10}");
            Console.WriteLine(new string('-', 70)); // Separator line for clarity

            while (reader.Read())
            {
                string userId = reader.GetInt32(0).ToString();
                string userName = reader.GetString(1);
                string email = reader.GetString(2);
                string isAdmin = reader["IsAdmin"].ToString() == "True" ? "Admin" : "User";

                // Print each row with the same formatting as the header
                Console.WriteLine($"{userId,-10} {userName,-20} {email,-30} {isAdmin,-10}");
            }

            // return count;

        }
        public static int CountAdminUserMasterTable()
        {
            string query = "SELECT * from UserTableMaster where Isadmin = 1";
            SqlParameter[] parameters = { };
            SqlDataReader reader = ExecuteReader(query, parameters);
            int count = 0;
            while (reader.Read())
            {
                count++;
            }
            return count;

        }

        public static bool UserExists(int userId)
        {
            string query = "SELECT COUNT(*) FROM UserTableMaster WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };

            object result = ExecuteScalar(query, parameters);
            return Convert.ToInt32(result ?? 0) > 0;
        }

    }
}
