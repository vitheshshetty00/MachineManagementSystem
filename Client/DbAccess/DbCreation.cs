using Client.DbAccess;
using Microsoft.Data.SqlClient;

namespace Clinet.DbAccess
{
    public class DbCreation
    {
        public static void InitializeDB()
        {
            string database = @"
            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Machine_Management_System_Clinet')
            BEGIN
                CREATE DATABASE Machine_Management_System_Client;
            END;
            ";

            string machineTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MachineTableMaster' AND xtype='U')
            CREATE TABLE MachineTableMaster (
                MachineId INTEGER PRIMARY KEY IDENTITY(1,1),
                MachineName NVARCHAR(50) NOT NULL,
                IP NVARCHAR(50) NOT NULL,
                Port NVarchar(4) NOT NULL,
                Image VARBINARY(Max) NOT NULL, 
                Timestamp DATETIME NOT NULL DEFAULT GETDATE())
               ;";

            string transactionTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TransactionTableMaster' AND xtype='U')
            CREATE TABLE TransactionTableMaster (
                TransactionId INTEGER PRIMARY KEY IDENTITY(1,1),
                M_Id INTEGER FOREIGN KEY REFERENCES MachineTableMaster(MachineId),
                Event NVARCHAR(10) NOT NULL DEFAULT 'Ping',
                Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
                Status NVARCHAR(10) NOT NULL);";

            string userTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserTableMaster' AND xtype='U')
            CREATE TABLE UserTableMaster (
                UserId INTEGER PRIMARY KEY IDENTITY(1,1),
                UserName NVARCHAR(50) NOT NULL,
                Password varchar(20),
                Email NVARCHAR(50) NOT NULL, 
                IsAdmin BIT DEFAULT 0);";

            SqlParameter[] parameters = { };

            try
            {
                DbConnectionManager.ExecuteNonQuery(database, parameters);
                Console.WriteLine("Database created successfully");

                DbConnectionManager.ExecuteNonQuery(machineTable, parameters);
                Console.WriteLine("Machine Table created successfully");

                DbConnectionManager.ExecuteNonQuery(transactionTable, parameters);
                Console.WriteLine("Transaction Table created successfully");

                DbConnectionManager.ExecuteNonQuery(userTable, parameters);
                Console.WriteLine("User Table created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}


