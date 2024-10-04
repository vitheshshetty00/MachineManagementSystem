using Server.DbAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Shared.Utils.InputValidators;
namespace Server.View
{
    public class UserServices
    {
        public static void addUser()
        {
            string? userName = Console.ReadLine();
            while (string.IsNullOrEmpty(userName))
            {
                Console.WriteLine("User Name cannot be empty.");
                userName = Console.ReadLine();
            }
            if (userName == "0") return;
            string? email = PromtAndValidateEmail("Enter Your Email");
            string password = Password.InputPassword();
            string userId = "0";
            if (UserAccess.IsUserTableEmpty() || UserAccess.CountAdminUserMasterTable() == 0)
            {
                userId = UserAccess.InsertIntoUserMasterTable(userName, password, email, 1);
                Console.WriteLine($"User '{userName}' added as admin with ID:{userId}");
            }
            else
            {
                Console.WriteLine("Is Admin? (1 for Yes, 0 for No): ");

                int.TryParse(Console.ReadLine(), out int isAdmin);

                while (isAdmin != 0 && isAdmin != 1)
                {
                    Console.WriteLine("Enter valid option.");
                    int.TryParse(Console.ReadLine(), out isAdmin);
                }
                userId = UserAccess.InsertIntoUserMasterTable(userName, password, email, isAdmin);
                Console.WriteLine($"User '{userName}' with ID:{userId} registered successfully.");
            }
        }

        public static void deleteuser()
        {
            try
            {
                Console.WriteLine("Enter User Id :");
                int.TryParse(Console.ReadLine(), out int id);

                if (UserAccess.DeleteUserMasterTable(id) > 0)
                {
                    Console.WriteLine("Deletion was successful");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void displayusertable()
        {
            UserAccess.displayUserMasterTable();
        }
    }

    public class Password
    {
        public static string GetPassword(string prompt)
        {
            Console.Write(prompt);
            StringBuilder password = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b"); 
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            return password.ToString();
        }

        public static bool IsValidPassword(string password)
        {
            return password.Length >= 8 &&
                   Regex.IsMatch(password, "[A-Z]") &&   
                   Regex.IsMatch(password, "[a-z]");     
        }

        public static string InputPassword()
        {
            while (true)
            {

                string password;
                while (true)
                {
                    password = GetPassword("Enter the password (at least 8 characters, 1 uppercase, 1 lowercase): ");
                    if (IsValidPassword(password))
                        break;
                    else
                    {
                        Console.WriteLine("re-enter the password atleast contain a Captail and a smaller letter and minimum size of 8");
                    }
                }
                string repassword = GetPassword("Re-enter the password: ");

                if (password == repassword)
                {
                    Console.WriteLine("\nCorrect password. Proceed.");
                    return password;
                }

                else
                {
                    Console.WriteLine("\nPasswords do not match. Please enter them again.");
                }
            }
        }
    }
}