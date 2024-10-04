using Server.DbAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.View
{
    public class TransactionServices
    {
        public static void addTransaction()
        {

            Console.WriteLine("Inserting into Transaction Table...\nEnter 0 to return to the main menu or provide a Machine ID:");

            string? machineIdInput = Console.ReadLine();
            if (machineIdInput == "0") return;

            int machineId;
            while (string.IsNullOrEmpty(machineIdInput) || !int.TryParse(machineIdInput, out machineId) || !DataBaseAccess.IsMachineIdValid(machineId))
            {
                Console.WriteLine("Please provide a valid Machine ID that exists in the MachineTableMaster:");
                machineIdInput = Console.ReadLine();
                if (machineIdInput == "0") return;
            }

            // Event Validation
            Console.WriteLine("Enter Event (or press Enter to use default 'Ping'):");
            string? eventInput = Console.ReadLine();
            if (string.IsNullOrEmpty(eventInput)) eventInput = "Ping";  // Use default if empty

            // Status Validation
            Console.WriteLine("Enter Status:Success or Failure");
            string? statusInput = Console.ReadLine();
            while (statusInput != "Success" && statusInput != "Failure")
            {
                Console.WriteLine("Invalid status. Please re-enter:");
                statusInput = Console.ReadLine();
            }

            try
            {
                int TransactionID = DataBaseAccess.InsertTransactionData(machineId, eventInput, statusInput);
                Console.WriteLine("Transaction data successfully inserted with ID." + TransactionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static void deleteTransaction()
        {
            try
            {
                Console.WriteLine("Enter the Transaction Id");
                int t_id = 0;
                int.TryParse(Console.ReadLine(), out t_id);
                while (t_id == 0)
                {
                    Console.WriteLine("Enter a valid Transaction Id");
                    int.TryParse(Console.ReadLine(), out t_id);

                }
                int result = DataBaseAccess.deleteMachineData(t_id);
                if (result > 0)
                    Console.WriteLine($"Machine with Id:{t_id} deleted");
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

        }
        public static void displayTransaction()
        {
            DataBaseAccess.displayTransactionData();
        }
    }
}
