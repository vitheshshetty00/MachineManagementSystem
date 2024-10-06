using Client.DbAccess;
using Client.Sync;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Sockets;
using System.Text;
using Shared.Utils;
using System.Text.Json;

namespace Client.Network
{
    public class ClientNetworkManager
    {
        private TcpClient client;
        private NetworkStream networkStream;
        private StreamReader reader;
        private StreamWriter writer;

        private TransactionSyncHandler transactionSyncHandler;


        public void Start()
        {

            try
            {
                ConnectToServer();
                networkStream = client.GetStream();
                reader = new StreamReader(networkStream);
                writer = new StreamWriter(networkStream);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting the connecting server: {ex.Message}");
            }
            ReceiveInitialData();


            Thread receiveThread = new Thread(MonitorConnection);
            receiveThread.Start();
        }
        private void ReceiveInitialData()
        {
            try
            {
                Console.WriteLine("Receiving initial data...");

                // Read machine data
                string machineBase64Json = reader.ReadLine();
                ProcessReceivedData(machineBase64Json);

                // Read user data
                string userBase64Json = reader.ReadLine();
                ProcessReceivedData(userBase64Json);


                Console.WriteLine("Initial data received and synchronized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving initial data: {ex.Message}");
            }
        }

        private void ProcessReceivedData(string base64Json)
        {
            try
            {
                byte[] jsonBytes = Convert.FromBase64String(base64Json);
                string jsonString = Encoding.UTF8.GetString(jsonBytes);
                var jsonData = JsonSerializer.Deserialize<ReceivedData>(jsonString);

                if (jsonData != null && jsonData.OperationType.Equals("InitialDataTransfer"))
                {
                    // Decode the Base64-encoded data table

                    DataSet dataSet = new DataSet();
                    

                    dataSet = Base64Helper.DecodeDataSet(jsonData.EncodedData);
                    dataSet.Tables[0].TableName = jsonData.TableName;
                    // Sync to local database
                    SyncToLocalDatabase(jsonData.TableName, dataSet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing  the recieved data{ex.Message}");

            }

        }

        private void SyncToLocalDatabase(string tableName, DataSet dataSet)
        {

            Console.WriteLine($"Syncing table {tableName} to local database...");
            //Utils.printDataTable(dataSet.Tables[0]);
        }

        private void ConnectToServer()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Attempting to connect to server...");

                    client = new TcpClient("172.18.224.1", 3400);
                    Console.WriteLine("Connected to server.");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection failed: {ex.Message}");
                    Console.WriteLine("Retrying in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }
            
        }

        private void MonitorConnection()
        {
            while (true)
            {
                if (!client.Connected)
                {
                    Console.WriteLine("Server disconnected. Attempting to reconnect...");
                    ConnectToServer();
                }
                Thread.Sleep(2000);

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



