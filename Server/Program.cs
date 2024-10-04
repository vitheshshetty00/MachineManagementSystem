using Server.DbAccess;
using Server.View;
using Server.Network;
using Shared.Models;

Console.WriteLine("Server Initialization");

DbCreation.InitializeDB();
Console.WriteLine("DB Initialized");

Task.Run(() => new ServerNetworkManager().Start());

while (true)
{
    int count = UserAccess.CountAdminUserMasterTable();
    if (count == 0)
    {
        Console.WriteLine("Enter the Admin detailss\nEnter your Username");
        UserServices.addUser();
    }
    Console.Write("Enter Login ID: ");
    int.TryParse(Console.ReadLine(), out int id);

    string? password = Password.GetPassword("Enter Password: ");

    while (string.IsNullOrEmpty(password))
    {
        Console.WriteLine("Please Enter a password..");
        password = Password.GetPassword("Enter Password: ");
    }
    if (UserAccess.ValidateLogin(id,password)) 
    {
        Menu.display(id);
    }
    else
    {
        Console.WriteLine("User not found.\nInCorrect Credentials");
    }
}