using Client.DbAccess;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Shared.Utils.Utils;

namespace Client.Network
{
    internal class ClientNetworkManager
    {
        private static TcpClient client;
        private static NetworkStream stream;


        public static void Start()
        {
            DBServices.CreateDataBases();
            ConnectToServer();
            Thread receiveThread = new Thread(MonitorConnection);
            receiveThread.Start();
        }

        private static void ConnectToServer()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Attempting to connect to server...");
                    client = new TcpClient("172.36.0.30", 3400);
                    stream = client.GetStream();
                    Console.WriteLine("Connected to server.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection failed: {ex.Message}");
                    Console.WriteLine("Retrying in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }
        }

        private static void MonitorConnection()
        {
            while (true)
            {
                try
                {
                    ReceiveMessages();
                }
                catch (IOException)
                {
                    Console.WriteLine("Server disconnected. Attempting to reconnect...");
                    ConnectToServer();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    break;
                }
            }
        }

        private static void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            MemoryStream accumulatedStream = new();
            DataSet ds2 = new DataSet();

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                accumulatedStream.Write(buffer, 0, bytesRead);
                string accumulatedMessage = Encoding.ASCII.GetString(accumulatedStream.ToArray());
                Console.WriteLine(accumulatedMessage);
                if (accumulatedMessage.Contains("\n"))
                {
                    string base64Message = accumulatedMessage.Trim();
                    byte[] decoded = Convert.FromBase64String(base64Message);
                    accumulatedStream.SetLength(0);
                    using (MemoryStream ms2 = new MemoryStream(decoded))
                    {
                        ds2.ReadXml(ms2);
                        printDataSet(ds2);
                    }
                    SqlDataAdapter da = new SqlDataAdapter("Select * from Student", DbConnectionManager.GetConnection());//
                    //We make this for all the master tables
                    SqlCommandBuilder cmdbuilder = new SqlCommandBuilder(da);
                    //We make this for all the master tables

                    if (ds2.HasChanges())
                    {
                        da.Update(ds2, "Student");
                    }
                    ds2.Clear();
                }
            }
        }
    }
}
