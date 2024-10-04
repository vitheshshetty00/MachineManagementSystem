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



                Task.Run(() => MonitorDatabaseChanges(client, writer));
                while (client.Connected)
                {
                    try
                    {

                        string? messageFromClient = reader.ReadLine();
                        if (!string.IsNullOrEmpty(messageFromClient))
                        {
                            Console.WriteLine($"Received from client: {messageFromClient}");
                            // Handle the transactions recieved from the client
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
                Console.WriteLine($"Error handling client: {ex.Message}");
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
    }
}
