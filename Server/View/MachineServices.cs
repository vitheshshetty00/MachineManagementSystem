using Server.DbAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.Utils.InputValidators;

namespace Server.View
{
    public class MachineServices
    {
        public static void addMachine()
        {
            Console.WriteLine("Inserting into Machine Table...\nEnter 0 to return to the main menu or provide a Machine Name");
            string? machineName = Console.ReadLine();
            if (machineName == "0") return;
            while (string.IsNullOrEmpty(machineName))
            {
                Console.WriteLine("Machine Name cannot be empty");
                machineName = Console.ReadLine();
            }
            Console.WriteLine("Enter IP Address");
            string? IpAddress = Console.ReadLine();
            while (string.IsNullOrEmpty(IpAddress) || ! IsValidIPAddress(IpAddress))
            {
                Console.WriteLine("Enter valid IP Address");
                IpAddress = Console.ReadLine();
            }
            Console.WriteLine("Enter Port number");
            string? port = Console.ReadLine();
            while (string.IsNullOrEmpty(port) || !IsNumeric(port) || (port.Length > 0 && port.Length <= 4))
            {
                Console.WriteLine("Enter valid Port number");
                port = Console.ReadLine();
            }
            Console.WriteLine("Enter Image Path");
            string? imageData = Console.ReadLine();
            while (string.IsNullOrEmpty(imageData) || !IsValidImagePath(imageData))
            {
                Console.WriteLine("Enter Image Path");
                imageData = Console.ReadLine();
            }
            string imgData = ConvertImageToBase64String(imageData);

            MachineAccess.InsertMachineData(machineName, IpAddress, port, imgData);
            //Image.saveImages();
        }

        public static void deleteMachine()
        {
            try
            {
                Console.WriteLine("Enter the Machine Id");
                int machineId = 0;
                int.TryParse(Console.ReadLine(), out machineId);
                while (machineId == 0)
                {
                    Console.WriteLine("Enter a valid MachineId");
                    int.TryParse(Console.ReadLine(), out machineId);

                }
                int result = MachineAccess.deleteMachineData(machineId);
                if (result > 0)
                    Console.WriteLine($"Machine with Id:{machineId} deleted");
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

        }

        public static void displayMachine()
        {
            MachineAccess.displayMachineData();
        }
    }
}
