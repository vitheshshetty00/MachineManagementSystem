using System.Data;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Client.DbAccess;
using Microsoft.Data.SqlClient;

namespace Client.Network
{
    public class NetworkTester
    {
        public void PerformNetworkTests()
        {
            DataTable ipPortTable = GetIpPortTable();

            foreach (DataRow row in ipPortTable.Rows)
            {
                string ipAddress = row["IP"].ToString();
                int port = Convert.ToInt32(row["Port"]);
                string machineId = row["MachineId"].ToString();

                bool isPingSuccessful = PingTest(ipAddress);
                bool isTelnetSuccessful = TelnetTest(ipAddress, port);

                LogTransaction(machineId, "Ping", isPingSuccessful ? "Success" : "Failure");
                LogTransaction(machineId, "Telnet", isTelnetSuccessful ? "Success" : "Failure");
            }
        }

        private DataTable GetIpPortTable()
        {
            DataTable ipPortTable = new DataTable();
            SqlConnection connection = DbConnectionManager.GetConnection();
            try
            {
                string query = "SELECT MachineId, IP, Port FROM MachineTableMaster";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(ipPortTable);
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error While fetching IP and Port: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While fetching IP and Port: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }

            return ipPortTable;
        }

        private bool PingTest(string ipAddress)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(ipAddress, 1000);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ping test failed for {ipAddress}: {ex.Message}");
                return false;
            }
        }

        private bool TelnetTest(string ipAddress, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(ipAddress, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    return success && client.Connected;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Telnet test failed for {ipAddress}:{port}: {ex.Message}");
                return false;
            }
        }

        private void LogTransaction(string machineId, string eventType, string status)
        {
            SqlConnection connection = DbConnectionManager.GetConnection();
            try
            {
                string query = "INSERT INTO TransactionTableMaster (M_Id, Event, Timestamp, Status) VALUES (@M_Id, @Event, @Timestamp, @Status)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@M_Id", machineId);
                    command.Parameters.AddWithValue("@Event", eventType);
                    command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    command.Parameters.AddWithValue("@Status", status);

                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error While logging transaction: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error While logging transaction: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }
    }
}

