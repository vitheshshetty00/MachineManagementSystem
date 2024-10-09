using Server.DbAccess;
using Shared.Utils;
using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;

namespace Server.Sync
{
    public class ServerUpdateHandler
    {
        private readonly string? _connectionString;
        private readonly StreamWriter _writer;
        private TcpClient _client;
        private long _lastSyncVersion;

        public ServerUpdateHandler(TcpClient client, StreamWriter writer)
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ServerMachineDBContext"]?.ConnectionString;
            _writer = writer;
            _client = client;
            _lastSyncVersion = 0; // Initialize with 0 or load from persistent storage

        }

        public void Start()
        {
            try
            {
                Console.WriteLine("Change Tracking started.");
                SqlConnection connection = DbConnectionManager.GetConnection();
                try
                {
                    _lastSyncVersion = GetCurrentVersion(connection);
                }
                finally
                {
                    connection.Close();
                }

                StartMonitoring("MachineTableMaster");
                StartMonitoring("UserTableMaster");

                Console.WriteLine("Monitoring for changes.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void Stop()
        {
            Console.WriteLine("Change Tracking stopped.");
        }

        private void StartMonitoring(string tableName)
        {
            try
            {
                Console.WriteLine($"Monitoring started for {tableName}.");

                // Check for changes periodically
                Task.Run(async () =>
                {
                    while (_client.Connected)
                    {
                        await Task.Delay(5000); // Check every 5 seconds
                        CheckForChanges(tableName);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartMonitoring for {tableName}: {ex.Message}");
            }
        }

        private void CheckForChanges(string tableName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string primaryKeyColumn = GetPrimaryKeyColumn(connection, tableName);

                    string query = $@"
                         SELECT CT.SYS_CHANGE_VERSION, CT.SYS_CHANGE_OPERATION, T.*
                        FROM CHANGETABLE(CHANGES {tableName}, @lastSyncVersion) AS CT
                        LEFT JOIN {tableName} AS T
                        ON CT.[{primaryKeyColumn}] = T.[{primaryKeyColumn}]";


                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@lastSyncVersion", _lastSyncVersion);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                DataTable changes = new DataTable();
                                changes.Load(reader);

                                
                                _lastSyncVersion = GetCurrentVersion(connection);

                                
                                foreach (DataRow row in changes.Rows)
                                {
                                    Console.WriteLine($"{row["SYS_CHANGE_OPERATION"]} \t{row["UserId"]}");
                                }

                                NotifyClient(tableName, changes);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for changes in {tableName}: {ex.Message}");
            }
        }

        private string GetPrimaryKeyColumn(SqlConnection connection, string tableName)
        {
            string query = $@"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
                AND TABLE_NAME = @tableName";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@tableName", tableName);
                return command.ExecuteScalar()?.ToString() ?? throw new Exception($"Primary key not found for table {tableName}");
            }
        }

        private long GetCurrentVersion(SqlConnection connection)
        {
            string query = "SELECT CHANGE_TRACKING_CURRENT_VERSION()";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                return (long)command.ExecuteScalar();
            }
        }

        private void NotifyClient(string tableName, DataTable changes)
        {
            try
            {
                changes.TableName = tableName;
                string data = GetEncodedData(tableName, changes);
                Console.WriteLine(data);
                _writer.WriteLine(data);
                Console.WriteLine($"Data sent to client for {tableName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying client for {tableName}: {ex.Message}");
            }
        }

        private string GetEncodedData(string tableName, DataTable changes)
        {
            try
            {
                string base64Data = Base64Helper.EncodeDatatable(changes);
                var jsonData = new
                {
                    OperationType = "DataChange",
                    TableName = tableName,
                    EncodedData = base64Data
                };
                string jsonString = JsonSerializer.Serialize(jsonData);
                return Base64Helper.Encode(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encoding data for {tableName}: {ex.Message}");
                return string.Empty;
            }
        }
        private class SendingData
        {
            public string OperationType { get; set; }
            public string TableName { get; set; }
            public string EncodedData { get; set; }
        }
    }
}


