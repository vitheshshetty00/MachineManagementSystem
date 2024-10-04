using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
namespace Server.DbAccess
{
    public class UserAccess
    {
        public static bool IsUserAdmin(int userId)
        {
            string query = "SELECT IsAdmin FROM UserTableMaster WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };

            object result = DataBaseAccess.ExecuteScalar(query, parameters);

            return result != null && Convert.ToInt32(result) == 1;
        }

        public static bool IsUserTableEmpty()
        {
            string query = "SELECT COUNT(*) FROM UserTableMaster";
            object result = DataBaseAccess.ExecuteScalar(query, null);
            return (int)(result ?? 0) == 0;
        }

        public static int InsertIntoUserMasterTable(string username, string password, string email, int isAdmin)
        {
            string query = "INSERT INTO UserTableMaster(UserName,Password,Email,IsAdmin) OUTPUT INSERTED.UserId VALUES (@username,@password,@email,@isAdmin)";
            SqlParameter[] parameters = {
                new SqlParameter("@username",username),
                new SqlParameter("@email",email),
                new SqlParameter("@password",password),
                new SqlParameter("@isAdmin", isAdmin )};
            return Convert.ToInt32(DataBaseAccess.ExecuteScalar(query, parameters));
        }

        public static int DeleteUserMasterTable(int u_id)
        {
            string query = "DELETE FROM UserTableMaster WHERE UserId = @u_id";
            SqlParameter[] parameters = { new SqlParameter("@u_id", u_id) };
            return DataBaseAccess.ExecuteNonQuery(query, parameters);

        }

        public static void displayUserMasterTable()
        {

            string query = "SELECT * from UserTableMaster";
            SqlParameter[] parameters = { };
            SqlDataReader reader = DataBaseAccess.ExecuteReader(query, parameters);

            Console.WriteLine($"{"User Id",-10} {"User Name",-20} {"Email",-30} {"User Type",-10}");
            Console.WriteLine(new string('-', 70));

            while (reader.Read())
            {
                string userId = reader["UserId"].ToString();
                string userName = reader["UserName"].ToString();
                string email = reader["Email"].ToString();
                string isAdmin = reader["IsAdmin"].ToString() == "True" ? "Admin" : "User";

                Console.WriteLine($"{userId,-10} {userName,-20} {email,-30} {isAdmin,-10}");
            }
        }
        public static int CountAdminUserMasterTable()
        {
            string query = "SELECT Count(*) from UserTableMaster where Isadmin = 1";
            SqlParameter[] parameters = { };
            int count = Convert.ToInt32(DataBaseAccess.ExecuteScalar(query, parameters));
            return count;
        }

        public static bool ValidateLogin(int userId, string enteredPassword)
        {

            string query = "SELECT Password FROM UserTableMaster WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
            var dbPassword = DataBaseAccess.ExecuteScalar(query, parameters)?.ToString();

            if (dbPassword != null)
            {
                return dbPassword == enteredPassword;
            }
            return false;
        }
    }
}
