using Client.DbAccess;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Sockets;
using System.Text;
using static Shared.Utils.Base64Helper;


namespace Client.Sync
{
    public class TransactionSyncHandler
    {
        private NetworkStream networkStream;
        private Timer? _syncTimer;

        public TransactionSyncHandler(NetworkStream networkStream)
        {
            this.networkStream = networkStream;
        }

        public void Start()
        {
            _syncTimer = new Timer(SyncTransaction, null, TimeSpan.Zero,TimeSpan.FromMinutes(2));
        }

        private void SyncTransaction(object? state)
        {
            Console.WriteLine("Transaction sync started.");

            DataTable transactionTable = GetRecentTransactions();

            if (transactionTable.Rows.Count > 0)
            {
                string transactionBase64 =EncodeDatatable(transactionTable);
                SendDataToServer(transactionBase64);
            }
            else
            {
                Console.WriteLine("No New Transactions to Sync.");
            }
            Console.WriteLine("Transaction sync completed.");

        }

        private void SendDataToServer(string base64Data)
        {
            try
            {
                byte[] dataToSend = Encoding.UTF8.GetBytes(base64Data);

                networkStream.Write(dataToSend, 0, dataToSend.Length);
                networkStream.Flush();
                Console.WriteLine("Data sent to server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to server: {ex.Message}");
            }
        }

        private DataTable GetRecentTransactions()
        {
            DataTable transactionTable = new DataTable();

            SqlConnection connection = DbConnectionManager.GetConnection();
            try
            {
                string query = @"SELECT * FROM TransactionTableMaster
                        WHERE Timestamp >= DATEADD(MINUTE, -2, GETDATE())";
                using (SqlCommand command = new SqlCommand(query,connection))
                {
                    using(SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(transactionTable);
                    }
                }
            }
            catch(SqlException ex)
            {
                Console.WriteLine(String.Format("An SQL Exception During Getting the Recent Transaction: {0}",ex.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("An Exception During Getting the Recent Transaction: {0}", ex.Message));
            }
            finally
            {
                connection.Close();
            }

            return transactionTable;
        }
    }
}

