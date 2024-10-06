using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Serilog;
namespace Server.DbAccess
{
    public class UserAccess
    {
        public static bool IsUserAdmin(string userId)
        {
            string query = "SELECT IsAdmin FROM UserTableMaster WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };

            try
            {
                object result = DataBaseAccess.ExecuteScalar(query, parameters);

                if (result != null && int.TryParse(result.ToString(), out int isAdmin))
                {
                    return isAdmin == 1;
                }
                else
                {
                    Log.Warning("User ID {UserId} not found or IsAdmin value is invalid.", userId);
                    return false;
                }
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "SQL Error occurred while checking if user with ID {UserId} is admin.", userId);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while checking if user with ID {UserId} is admin.", userId);
                return false;
            }
        }


        public static bool IsUserTableEmpty()
        {
            string query = "SELECT COUNT(*) FROM UserTableMaster";
            object result = DataBaseAccess.ExecuteScalar(query, null);
            return (int)(result ?? 0) == 0;
        }

        public static string InsertIntoUserMasterTable(string username, string password, string email, int isAdmin)
        {
            string query = "INSERT INTO UserTableMaster(UserId,UserName,Password,Email,IsAdmin) OUTPUT INSERTED.UserId VALUES (@userId,@username,@password,@email,@isAdmin)";
            string userId = DbCreation.GenerateUserId();
            string hashedPassword = HashPassword(password);
            SqlParameter[] parameters = {
                new SqlParameter("@userId",userId),
                new SqlParameter("@username", username),
                new SqlParameter("@password", hashedPassword),
                new SqlParameter("@email", email),
                new SqlParameter("@isAdmin", isAdmin)
            };

            try
            {
                userId = (DataBaseAccess.ExecuteScalar(query, parameters)).ToString();

                Log.Information("User inserted successfully. Username: {Username}, Email: {Email}, IsAdmin: {IsAdmin}, UserID: {UserID}",
                    username, email, isAdmin, userId);

                return userId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inserting user data. Username: {Username}, Email: {Email}, IsAdmin: {IsAdmin}",
                    username, email, isAdmin);
                throw;

                return "-1";
            }
        }


        public static int DeleteUserMasterTable(int u_id)
        {
            string query = "DELETE FROM UserTableMaster WHERE UserId = @u_id";
            SqlParameter[] parameters = { new SqlParameter("@u_id", u_id) };

            try
            {
                int rowsAffected = DataBaseAccess.ExecuteNonQuery(query, parameters);
                Log.Information("User with ID: {UserId} deleted successfully. Rows affected: {RowsAffected}", u_id, rowsAffected);
                return rowsAffected;
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "SQL Error occurred while deleting user with ID: {UserId}", u_id);
                return -1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting user with ID: {UserId}", u_id);
                return -1;
            }
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

        public static bool ValidateLogin(string userId, string enteredPassword)
        {

            string query = "SELECT Password FROM UserTableMaster WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
            var dbPassword = DataBaseAccess.ExecuteScalar(query, parameters)?.ToString();

            if (dbPassword != null)
            {
                string hashedEnteredPassword = HashPassword(enteredPassword);
                return dbPassword == hashedEnteredPassword;
            }
            return false;
        }
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
