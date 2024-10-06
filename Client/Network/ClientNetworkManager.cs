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

                
                string machineBase64Json = reader.ReadLine();
                ProcessReceivedData(machineBase64Json);

           
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

                    DataSet dataSet = new DataSet();
                    

                    dataSet = Base64Helper.DecodeDataSet(jsonData.EncodedData);
                    dataSet.Tables[0].TableName = jsonData.TableName;
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
            SqlConnection connection = DbConnectionManager.GetConnection();
            try
            {
                string deleteQuery = $"DELETE FROM {tableName}";
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                if (tableName == "MachineTableMaster" && dataSet.Tables[0].Columns.Contains("Image"))
                {
                    DataColumn newColumn = new DataColumn("Image_temp", typeof(byte[]));
                    dataSet.Tables[0].Columns.Add(newColumn);

                    
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        if (row["Image"] is string imageString)
                        {
                            row["Image_temp"] = Convert.FromBase64String(imageString);
                        }
                    }

                    dataSet.Tables[0].Columns.Remove("Image");

                    newColumn.ColumnName = "Image";
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter())
                {
                    adapter.SelectCommand = new SqlCommand($"SELECT * FROM {tableName} WHERE 1 = 0", connection);
                    SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                    adapter.InsertCommand = builder.GetInsertCommand();

                    adapter.Update(dataSet.Tables[0]);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error While Sync to local Database: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While Syncto local Database: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
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
                    networkStream = client.GetStream();
                    reader = new StreamReader(networkStream);
                    writer = new StreamWriter(networkStream);

                    transactionSyncHandler = new TransactionSyncHandler(writer);
                    transactionSyncHandler.Start();
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
                    transactionSyncHandler.Stop();
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



