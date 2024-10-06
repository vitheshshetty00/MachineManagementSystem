using Microsoft.Data.SqlClient;
using Server.DbAccess;
using Shared.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Shared.Utils.Utils;

namespace Server.Network
{
    public class ServerNetworkManager
    {
        //private static List<TcpClient> clients = new List<TcpClient>();
        private static TcpListener? server;
        private bool isRunning;

        public void Start()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 3400);
                server.Start();
                isRunning = true;
                Console.WriteLine("Server started. Waiting for clients...");

                while (isRunning)
                {
                    try
                    {
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Client connected.");

                        Thread clientThread = new Thread(() => HandleClient(client));
                        clientThread.IsBackground = true;
                        clientThread.Start();
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine($"Socket error: {se.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error accepting client: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error Starting the Server:{e.Message}");
            }
           
        }
        public void StopServer()
        {
            isRunning = false;
            server?.Stop();
            Console.WriteLine("Server stopped.");
        }


        private static void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
                StreamReader reader = new StreamReader(stream);
                //When a client connects, send the database to the client(i,e the Machine table and Users table)
                SendDatabaseTables(writer);
                Console.WriteLine("Hii");
                //Thread transactionThread = new Thread(() => HandleTransactions(client, reader));
                //transactionThread.IsBackground = true;
                //transactionThread.Start();


                //Task.Run(() => MonitorDatabaseChanges(client, writer));
                //while (client.Connected)
                //{
                //    try
                //    {

                //        string? messageFromClient = reader.ReadLine();
                //        if (!string.IsNullOrEmpty(messageFromClient))
                //        {
                //            Console.WriteLine($"Received from client: {messageFromClient}");
                //            // Handle the transactions recieved from the client
                //        }
                //    }
                //    catch (IOException ioEx)
                //    {
                //        Console.WriteLine($"Client connection error: {ioEx.Message}");
                //        break;
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"Client connection error: {ex.Message}");
                //        break;
                //    }
                //}
                
                    while (client.Connected)
                    {
                        try
                        {
                            string? messageFromClient = reader.ReadLine();

                            if (!string.IsNullOrEmpty(messageFromClient))
                            {
                                //Console.WriteLine($"Received from client: {messageFromClient}");
                                HandleClientMessage(messageFromClient);
                            }
                        }
                        catch (IOException ioEx)
                        {
                            Console.WriteLine($"Client connection error: {ioEx.Message}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Client connection error: {ex.Message}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling transactions: {ex.Message}");
                }
            

        }

        private static void HandleTransactions(TcpClient client, StreamReader reader)
        {
            try
            {
                while (client.Connected)
                {
                    try
                    {
                        string? messageFromClient = reader.ReadLine();
                        if (!string.IsNullOrEmpty(messageFromClient))
                        {
                            //Console.WriteLine($"Received from client: {messageFromClient}");
                            HandleClientMessage(messageFromClient);
                        }
                    }
                    catch (IOException ioEx)
                    {
                        Console.WriteLine($"Client connection error: {ioEx.Message}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Client connection error: {ex.Message}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling transactions: {ex.Message}");
            }
        }

        private static void HandleClientMessage(string messageFromClient)
        {
            
                try
                {
                    byte[] jsonBytes = Convert.FromBase64String(messageFromClient);
                    string jsonString = Encoding.UTF8.GetString(jsonBytes);
                    var jsonData = JsonSerializer.Deserialize<ReceivedData>(jsonString);

                    if (jsonData != null && jsonData.OperationType.Equals("TransactionSync"))
                    {

                        DataSet dataSet = new DataSet();


                        dataSet = Base64Helper.DecodeDataSet(jsonData.EncodedData);
                        dataSet.Tables[0].TableName = jsonData.TableName;
                        //printDataSet(dataSet);
                        HandleTransactionSync(dataSet);
    
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while processing  the recieved data{ex.Message}");

                }

            
        }

        private static void HandleTransactionSync(DataSet dataSet)
        {
            try
            {
                // Get the timestamp of the last updated transaction
                DateTime lastUpdatedTimestamp = GetLastUpdatedTransactionTimestamp();

                // Filter the dataset to include only new transactions
                DataTable transactionTable = dataSet.Tables[0];
                var newTransactions = transactionTable.AsEnumerable()
                .Where(row =>
                {
                    try
                    {
                        string timestampString = row.Field<string>("Timestamp");
                        DateTime timestamp = DateTime.Parse(timestampString);
                        return timestamp > lastUpdatedTimestamp;
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Invalid format for row with Timestamp: {row["Timestamp"]}");
                        return false;
                    }
                    catch (InvalidCastException)
                    {
                        Console.WriteLine($"Invalid cast for row with Timestamp: {row["Timestamp"]}");
                        return false;
                    }
                })
                .CopyToDataTable();

                printDataTable(newTransactions);
                // Insert new transactions into the database
                InsertNewTransactions(newTransactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling transaction sync: {ex.Message}");
            }
        }
        private static DateTime GetLastUpdatedTransactionTimestamp()
        {
            DateTime lastUpdatedTimestamp = DateTime.MinValue;
            SqlConnection connection = DbConnectionManager.GetConnection();
            try
            {
                string query = "SELECT MAX(Timestamp) FROM TransactionTableMaster";
                
                
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            lastUpdatedTimestamp = (DateTime)result;
                        }
                    }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting last updated transaction timestamp: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }

            return lastUpdatedTimestamp;
        }

        private static void InsertNewTransactions(DataTable newTransactions)
        {
            string insertQuery = @"
                INSERT INTO TransactionTableMaster (M_Id, Event, Timestamp, Status)
                VALUES (@M_Id, @Event, @Timestamp, @Status)";

            SqlConnection connection = DbConnectionManager.GetConnection();
            try
            {
                
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (DataRow row in newTransactions.Rows)
                            {
                                using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@M_Id", row["M_Id"]);
                                    command.Parameters.AddWithValue("@Event", row["Event"]);
                                    command.Parameters.AddWithValue("@Timestamp", DateTime.Parse((string)row["Timestamp"]));
                                    command.Parameters.AddWithValue("@Status", row["Status"]);
                                    command.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error inserting new transactions: {ex.Message}");
                        }
                    }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error establishing database connection: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }


        private static void SendDatabaseTables(StreamWriter writer)
        {
            try
            {
                string machineData = GetEncodedData("MachineTableMaster", "SELECT * FROM MachineTableMaster");
                string usersData = GetEncodedData("UserTableMaster", "SELECT * FROM UserTableMaster");


                writer.WriteLine(machineData);
                writer.WriteLine(usersData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending database tables: {ex.Message}");
            }
        }

        private static string GetEncodedData(string tableName, string query)
        {
            try
            {
               string base64Data = GetData(query);
                var jsonData = new
                {
                    OperationType = "InitialDataTransfer",
                    TableName = tableName,
                    EncodedData = base64Data
                };
                string jsonString = JsonSerializer.Serialize(jsonData);
                string base64Json = Base64Helper.Encode(jsonString);
                return base64Json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Encoding data: {ex.Message}");
                return string.Empty;
            }

        }

        private static void MonitorDatabaseChanges(TcpClient client, StreamWriter writer)
        {
            throw new NotImplementedException();
        }

     

        public static string GetData(string query)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(query, DbConnectionManager.GetConnection());
                da.Fill(ds);
                string base64ds= Base64Helper.EncodeDataSet(ds);
              
                return base64ds ;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Geeting data: {ex.Message}");
                return string.Empty;
            }
        }
        private class ReceivedData
        {
            public string OperationType { get; set; }
            public string TableName { get; set; }
            public string EncodedData { get; set; }
        }
    }

}
