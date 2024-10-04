using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Serilog;
namespace Server.DbAccess
{
    public class TransactionAccess
    {
        public static int InsertTransactionData(int machineID, string eventInput, string statusInput)
        {
                string query = "INSERT INTO TransactionTableMaster(M_Id,Event,Status) OUTPUT Inserted.TransactionId VALUES(@m_id,@event,@status)";
                SqlParameter[] parameters = {
                new SqlParameter("@m_id", machineID),
                new SqlParameter("@event", eventInput),
                new SqlParameter("@status", statusInput)
        };

            try
            {
                int transactionId = Convert.ToInt32(DataBaseAccess.ExecuteScalar(query, parameters));

                // Log success message
                Log.Information($"Transaction data inserted successfully. MachineID: {machineID}, Event: {eventInput}, Status: {statusInput}");

                return transactionId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error inserting transaction data for MachineID: {machineID}, Event: {eventInput}, Status: {statusInput}");

                return -1; // Return -1 or an error-specific value
            }
        }

        public static int deleteTransactionData(int t_id)
        {
            string query = "DELETE FROM TransactionTableMaster WHERE TransactionId = @t_id";
            SqlParameter[] parameters = { new SqlParameter("@t_id", t_id) };

            try
            {
                int deletedCount = DataBaseAccess.ExecuteNonQuery(query, parameters);

                if (deletedCount > 0)
                {
                   
                    Log.Information("Transaction deleted successfully. Transaction ID: {TransactionId}", t_id);
                }
                else
                {
                    
                    Log.Warning("No transaction found with ID: {TransactionId}. No rows deleted.", t_id);
                }

                return deletedCount;
            }
            catch (Exception ex)
            {
                
                Log.Error(ex, "Error deleting transaction with ID: {TransactionId}", t_id);
                return -1; 
            }
        }

        public static void displayTransactionData()
        {
            string query = "SELECT * from TransactionTableMaster";
            SqlParameter[] parameters = { };
            SqlDataReader reader = DataBaseAccess.ExecuteReader(query, parameters);

            Console.WriteLine($"{"Transaction ID",-15} {"Machine ID",-12} {"Event",-20} {"Timestamp",-15} {"Status",-10}");
            Console.WriteLine(new string('-', 75));

            while (reader.Read())
            {
                string t_id = reader["TransactionId"].ToString();
                string m_id = reader["M_id"].ToString();
                string eventName = reader["Event"].ToString();
                string timestamp = reader["timestamp"].ToString();
                string status = reader["Status"].ToString();

                Console.WriteLine($"{t_id,-15} {m_id,-12} {eventName,-20} {timestamp,-15} {status,-10}");
            }
        }
    }
}
