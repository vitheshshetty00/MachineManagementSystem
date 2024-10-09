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
        private TcpClient _client;
        private NetworkStream _networkStream;
        private StreamReader _reader;
        private StreamWriter _writer;

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

            Thread receiveThread = new Thread(MonitorConnection);
            receiveThread.Start();


            //ReceiveInitialData();
            ReceiveData();
        }

        private void ReceiveData()
        {
            try
            {
                while(_client.Connected)
                {
                    string base64Json = _reader.ReadLine();
                    ProcessData(base64Json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex.Message}");
            }
        }

        private void ProcessData(string? base64Json)
        {
            try
            {
                byte[] jsonBytes = Convert.FromBase64String(base64Json);
                string jsonString = Encoding.UTF8.GetString(jsonBytes);
                var jsonData = JsonSerializer.Deserialize<ReceivedData>(jsonString);
                switch(jsonData?.OperationType)
                {
                    case "InitialDataTransfer":
                        DataSet dataSet = new DataSet();
                        dataSet = Base64Helper.DecodeDataSet(jsonData.EncodedData);
                        dataSet.Tables[0].TableName = jsonData.TableName;
                        SyncToLocalDatabase(jsonData.TableName, dataSet);
                        break;
                    case "DataChange":
                        handleDataChange(jsonData);
                        break;
                    default:
                        Console.WriteLine("Unknown operation type.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing data: {ex.Message}");
                throw;
            }
        }

        private void handleDataChange(ReceivedData jsonData)
        {
            string tableName = jsonData.TableName;
            string base64Data = jsonData.EncodedData;
            DataTable ChnageTable = Base64Helper.DecodeDatatable(base64Data);
           
            SqlConnection connection = DbConnectionManager.GetConnection();

            try
            {
                if (tableName == "MachineTableMaster" && ChnageTable.Columns.Contains("Image"))
                {
                    DataColumn newColumn = new DataColumn("Image_temp", typeof(byte[]));
                    ChnageTable.Columns.Add(newColumn);

                    foreach (DataRow row in ChnageTable.Rows)
                    {
                        if (row["Image"] is string imageString)
                        {
                            row["Image_temp"] = Convert.FromBase64String(imageString);
                        }
                    }

                    ChnageTable.Columns.Remove("Image");

                    newColumn.ColumnName = "Image";
                }

                foreach(DataRow row in ChnageTable.Rows)
                {
                    switch (row["SYS_CHANGE_OPERATION"])
                    {
                        case "I":
                            InsertRow(tableName, row, connection);
                            break;
                        case "U":
                            UpdateRow(tableName, row, connection);
                            break;
                        case "D":
                            DeleteRow(tableName, row, connection);
                            break;
                        default:
                            Console.WriteLine("Unknown operation type.");
                            break;

                    }

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

        private void DeleteRow(string tableName, DataRow row, SqlConnection connection)
        {
            try
            {
                string primaryKeyColumn = GetPrimaryKeyColumn(connection, tableName);
                string query = $"DELETE FROM {tableName} WHERE {primaryKeyColumn} = @primaryKeyValue";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@primaryKeyValue", row[primaryKeyColumn]);
                    _ = command.ExecuteNonQuery();
                }
                Console.WriteLine($"Row Deleted from {tableName}");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error While Deleting Row: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While Deleting Row: {ex.Message}");
                throw;
            }

        }

        private void UpdateRow(string tableName, DataRow row, SqlConnection connection)
        {
            throw new NotImplementedException();
        }

        private void InsertRow(string tableName, DataRow row, SqlConnection connection)
        {
            Console.WriteLine("Hii");
            var columnValues = new Dictionary<string, object>();
            foreach (DataColumn column in row.Table.Columns)
            {
                if (column.ColumnName != "SYS_CHANGE_OPERATION" && column.ColumnName != "SYS_CHANGE_VERSION")
                    columnValues.Add(column.ColumnName, row[column]);
            }
            StringBuilder query = new StringBuilder();
            query.Append($"INSERT INTO {tableName} (");
            query.Append(string.Join(", ", columnValues.Keys));
            //query.Length -= 2;
            query.Append(") VALUES (");
            foreach (var column in columnValues.Keys)
            {
                query.Append($"@{column}, ");
            }
            query.Length -= 2;
            query.Append(");");
            string queryString = query.ToString();

            List<SqlParameter> parameters = new List<SqlParameter>();
            foreach (var column in columnValues)
            {
                parameters.Add(new SqlParameter($"@{column.Key}", column.Value));
            }
            try
            {
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    _=command.ExecuteNonQuery();
                }
                Console.WriteLine($"New Row Inserted to {tableName}");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error While Inserting Row: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While Inserting Row: {ex.Message}");
                throw;
            }
        }
        private string GetPrimaryKeyColumn(SqlConnection connection, string tableName)
        {
            string query = $@"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
                AND TABLE_NAME = @tableName";

            try
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@tableName", tableName);
                    return command.ExecuteScalar()?.ToString() ?? throw new Exception($"Primary key not found for table {tableName}");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error While Getting Primary Key: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While Getting Primary Key: {ex.Message}");
                throw;
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
                    //adapter.InsertCommand = builder.GetInsertCommand();
                    adapter.UpdateCommand = builder.GetUpdateCommand();
                    adapter.DeleteCommand = builder.GetDeleteCommand();

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

                    _client = new TcpClient("172.36.0.24", 3400);
                    Console.WriteLine("Connected to server.");
                    _networkStream =  _client.GetStream();
                    _reader = new StreamReader(_networkStream);
                    _writer = new StreamWriter(_networkStream);

                    transactionSyncHandler = new TransactionSyncHandler(_writer);
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
                if (!_client.Connected)
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



