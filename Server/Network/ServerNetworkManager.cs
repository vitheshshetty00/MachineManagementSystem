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
        private static List<TcpClient> clients = new List<TcpClient>();
        private static TcpListener listener;

        public static void Start()
        {
            listener = new TcpListener(IPAddress.Any, 3400);
            listener.Start();
            Console.WriteLine("Server started. Waiting for clients...");

            Thread consoleThread = new Thread(HandleConsoleInput);
            consoleThread.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                clients.Add(client);
                Console.WriteLine("Client connected.");

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        private static void HandleConsoleInput()
        {
            while (true)
            {
                string message = Console.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    BroadcastMessage(message);
                }
            }
        }

        private static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                string message = SendData();
                BroadcastMessage(message);
                //Thread.Sleep(10000);
                //BroadcastMessage(message);
            }
            catch (IOException)
            {
                Console.WriteLine("Client disconnected.");
            }
        }

        private static void BroadcastMessage(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            foreach (var client in clients)
            {
                NetworkStream stream = client.GetStream();
                try
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch (IOException)
                {
                    Console.WriteLine("Failed to send message to a client.");
                }
            }
        }

        public static string SendData()
        {
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter("Select * from Student", DbConnectionManager.GetConnection());
            da.Fill(ds, "Student");
            DataSet ds2 = new DataSet();
            string base64ds;
            using (MemoryStream ms = new MemoryStream())
            {
                ds.WriteXml(ms);
                byte[] buffer = ms.ToArray();
                base64ds = Convert.ToBase64String(buffer);

                byte[] decoded = Convert.FromBase64String(base64ds);
                using (MemoryStream ms2 = new MemoryStream(decoded))
                {
                    ds2.ReadXml(ms2);
                    printDataSet(ds2);
                }
            }
            return base64ds + "\n";
        }
    }
}
