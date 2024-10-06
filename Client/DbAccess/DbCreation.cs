using Client.DbAccess;
using Microsoft.Data.SqlClient;

namespace Clinet.DbAccess
{
    public class DbCreation
    {
        public static void InitializeDB()
        {
            
            string machineTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MachineTableMaster' AND xtype='U')
            CREATE TABLE MachineTableMaster (
                MachineId NVARCHAR(50) PRIMARY KEY,
                MachineName NVARCHAR(50) NOT NULL,
                IP NVARCHAR(50) NOT NULL,
                Port INTEGER NOT NULL,
                Image VARBINARY(MAX) NOT NULL, 
                Creation_Time DATETIME NOT NULL DEFAULT GETDATE(),
                LastUpdated DATETIME NOT NULL DEFAULT GETDATE()
            );";

            string transactionTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TransactionTableMaster' AND xtype='U')
            CREATE TABLE TransactionTableMaster (
                TransactionId INTEGER PRIMARY KEY IDENTITY(1,1),
                M_Id NVARCHAR(50) FOREIGN KEY REFERENCES MachineTableMaster(MachineId) ON DELETE SET NULL,
                Event NVARCHAR(10) NOT NULL DEFAULT 'Ping',
                Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
                Status NVARCHAR(10) NOT NULL
            );";

            string userTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserTableMaster' AND xtype='U')
            CREATE TABLE UserTableMaster (
                UserId NVARCHAR(50) PRIMARY KEY,
                UserName NVARCHAR(50) NOT NULL,
                Password NVARCHAR(256) NOT NULL,
                Email NVARCHAR(50) NOT NULL, 
                IsAdmin BIT DEFAULT 0
            );";

            SqlParameter[] parameters = { };

            try
            {

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


