﻿using Server.DbAccess;
using System;
using System.Data.SqlClient;

namespace Server.DbAccess
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
                Image VARBINARY(1000) NOT NULL, 
                Creation_Time DATETIME NOT NULL DEFAULT GETDATE(),
                LastUpdated DATETIME NOT NULL DEFAULT GETDATE()
            );";

            string transactionTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TransactionTableMaster' AND xtype='U')
            CREATE TABLE TransactionTableMaster (
                TransactionId INTEGER PRIMARY KEY IDENTITY(1,1),
                M_Id NVARCHAR(50) FOREIGN KEY REFERENCES MachineTableMaster(MachineId),
                Event NVARCHAR(10) NOT NULL DEFAULT 'Ping',
                Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
                Status NVARCHAR(10) NOT NULL
            );";

            string userTable = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserTableMaster' AND xtype='U')
            CREATE TABLE UserTableMaster (
                UserId NVARCHAR(50) PRIMARY KEY,
                UserName NVARCHAR(50) NOT NULL,
                Email NVARCHAR(50) NOT NULL, 
                IsAdmin BIT DEFAULT 0
            );";

            try
            {
                // Execute each table creation query
                DataBaseAccess.ExecuteNonQuery(machineTable, null);
                Console.WriteLine("Machine Table created successfully");

                DataBaseAccess.ExecuteNonQuery(transactionTable, null);
                Console.WriteLine("Transaction Table created successfully");

                DataBaseAccess.ExecuteNonQuery(userTable, null);
                Console.WriteLine("User Table created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating tables: {ex.Message}");
            }
        }

        public static string GenerateMachineId()
        {
         
            int count = GetMachineCount();
            return $"MC{(count + 1):D2}"; 
        }

        public static string GenerateUserId()
        {
            
            int count = GetUserCount();
            return $"U{(count + 1):D3}"; 
        }

        private static int GetMachineCount()
        {
            string query = "SELECT TOP 1 UserId FROM MachineTableMaster ORDER BY UserId DESC ";
            object result = DataBaseAccess.ExecuteScalar(query, null);
            return (int)(result ?? 0);

            string number=result.ToString().Substring(2);

            return Convert.ToInt32(number);

           
        }

        private static int GetUserCount()
        {
            string query = "SELECT TOP 1 UserId FROM UserTableMaster ORDER BY UserId DESC";
            
            object result = DataBaseAccess.ExecuteScalar(query, null); 

     
            if (result == null)
            {
                return 0; 
            }

            string userId = result.ToString();

     
            if (userId.Length > 1 && userId.StartsWith("U"))
            {
                string numberPart = userId.Substring(1); 
                return Convert.ToInt32(numberPart); 
            }

            return 0; 
        }

    }
}
