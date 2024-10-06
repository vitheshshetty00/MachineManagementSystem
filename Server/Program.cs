using Server.DbAccess;
using Server.View;
using Server.Network;
using Shared.Models;
using System.Threading.Channels;
using System.Text.RegularExpressions;

//Console.WriteLine("Server Initialization");

DbCreation.InitializeDB();
//Console.WriteLine("DB Initialized");

Console.WriteLine("Server Started...");

Task.Run(() => new ServerNetworkManager().Start());

while (true)
{
    try
    {
        int count = UserAccess.CountAdminUserMasterTable();
        if (count == 0)
        {
            Console.WriteLine("Enter the Admin details...\n ");
            bool adminAdded = false;
            while (!adminAdded)
            {
                try
                {
                    UserServices.addUser();
                    adminAdded = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add admin. Error: {ex.Message}");
                    Console.WriteLine("Please try again.");
                }
            }
        }

        bool loginSuccessful = false;
        while (!loginSuccessful)
        {
            Console.Write("Enter User ID (format U000): ");
            string? userId = Console.ReadLine();

            while (string.IsNullOrEmpty(userId) || !Regex.IsMatch(userId, @"^U\d{3}$"))
            {
                Console.WriteLine("Please enter a valid User ID in the format U000.");
                userId = Console.ReadLine();
            }

            string? password = Password.GetPassword("Enter Password: ");

            while (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Please Enter a password..");
                password = Password.GetPassword("Enter Password: ");
            }

            if (UserAccess.ValidateLogin(userId, password))
            {
                loginSuccessful = true;
                Menu.display(userId);
            }
            else
            {
                Console.WriteLine("User not found.\nIncorrect Credentials. Please try again.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}