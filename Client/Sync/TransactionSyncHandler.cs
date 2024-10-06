using Client.DbAccess;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using static Shared.Utils.Base64Helper;

namespace Client.Sync
{
    public class TransactionSyncHandler
    {
        private StreamWriter writer;
        private Timer? _syncTimer;

        public TransactionSyncHandler(StreamWriter writer)
        {
            this.writer = writer;
        }

        public void Start()
        {
            
            _syncTimer = new Timer(SyncTransaction, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }
        public void Stop()
        {
            if (_syncTimer != null)
            {
                _syncTimer.Dispose();
                _syncTimer = null;
                Console.WriteLine("Transaction sync stopped.");
            }
        }

        private void SyncTransaction(object? state)
        {
            Console.WriteLine("Transaction sync started.");

            DataSet transactionDataSet = GetRecentTransactions();

            if (transactionDataSet.Tables[0].Rows.Count > 0)
            {
                // Remove the TransactionId column
                try
                {
                    transactionDataSet.Tables[0].Columns.Remove("TransactionId");

                    string transactionBase64 = EncodeDataSet(transactionDataSet);

                    var payload = new
                    {
                        OperationType = "TransactionSync",
                        TableName = "TransactionTableMaster",
                        EncodedData = transactionBase64
                    };

                    string jsonPayload = JsonSerializer.Serialize(payload);
                    SendDataToServer(jsonPayload);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error encoding transaction data: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No New Transactions to Sync.");
            }
            Console.WriteLine("Transaction sync completed.");
        }

        private void SendDataToServer(string jsonPayload)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonPayload))
                {
                    Console.WriteLine("No data to send to server.");
                    return;
                }
                string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonPayload));

                writer.WriteLine(base64Payload);
                writer.Flush();
                Console.WriteLine("Data sent to server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to server: {ex.Message}");
            }
        }

        private DataSet GetRecentTransactions()
        {
            DataSet transactionDataSet = new DataSet();


            SqlConnection connection = DbConnectionManager.GetConnection();
            try
            {
                string query = @"SELECT * FROM TransactionTableMaster
                                 WHERE Timestamp >= DATEADD(MINUTE, -2, GETDATE())";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(transactionDataSet);
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"An SQL Exception During Getting the Recent Transaction: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An Exception During Getting the Recent Transaction: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }

            return transactionDataSet;
        }
    }
}
