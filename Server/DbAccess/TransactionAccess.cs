using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
namespace Server.DbAccess
{
    public class TransactionAccess
    {
        public static int InsertTransactionData(int machineID, string eventInput, string statusInput)
        {
            string query = "INSERT INTO TransactionTableMaster(M_Id,Event,Status) Output Inserted.TransactionId VALUES(@m_id,@event,@status)";
            SqlParameter[] parameters = {
                new SqlParameter("@m_id",machineID),
                new SqlParameter("@event",eventInput),
                new SqlParameter("status",statusInput)};
            return Convert.ToInt32(DataBaseAccess.ExecuteScalar(query, parameters));
        }
        public static int deleteTransactionData(int t_id)
        {
            string query = "DELETE FROM TransactionTableMaster WHERE TransactionId = @t_id";
            SqlParameter[] parameters = { new SqlParameter("@t_id", t_id) };
            return DataBaseAccess.ExecuteNonQuery(query, parameters);
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
