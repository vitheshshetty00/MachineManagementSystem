using Microsoft.Data.SqlClient;
using Server.DbAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Shared.Utils.Utils;

namespace Server.Network
{
    internal class ServerNetworkManager
    {
        //private static List<TcpClient> clients = new List<TcpClient>();
        private static TcpListener? Server;
        private bool isRunning;

        public void Start()
        {
            try
            {
                Server = new TcpListener(IPAddress.Any, 3400);
                Server.Start();
                isRunning = true;
                Console.WriteLine("Server started. Waiting for clients...");

                while (isRunning)
                {
                    try
                    {
                        TcpClient client = Server.AcceptTcpClient();
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
            Server?.Stop();
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
                string machineData = SendData("SELECT * FROM MachineTableMaster");
                string usersData = SendData("SELECT * FROM UserTableMaster");

                writer.WriteLine(machineData);
                writer.WriteLine(usersData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending database tables: {ex.Message}");
            }
        }

        private static void MonitorDatabaseChanges(TcpClient client, StreamWriter writer)
        {
            throw new NotImplementedException();
        }

        //private static void BroadcastMessage(string message)
        //{
        //    byte[] buffer = Encoding.ASCII.GetBytes(message);
        //    foreach (var client in clients)
        //    {
        //        NetworkStream stream = client.GetStream();
        //        try
        //        {
        //            stream.Write(buffer, 0, buffer.Length);
        //        }
        //        catch (IOException)
        //        {
        //            Console.WriteLine("Failed to send message to a client.");
        //        }
        //    }
        //}

        public static string SendData(string query)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(query, DbConnectionManager.GetConnection());
                da.Fill(ds);
                string base64ds;
                using (MemoryStream ms = new MemoryStream())
                {
                    ds.WriteXml(ms);
                    byte[] buffer = ms.ToArray();
                    base64ds = Convert.ToBase64String(buffer);
                }
                return base64ds + "\n";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
