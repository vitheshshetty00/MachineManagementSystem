using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shared.Utils
{
    public static class InputValidators
    {
        public static string PromptForValidString(string message, int maxLength = 256)
        {
            string? input;
            do
            {
                Console.Write(message);
                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || input.Length > maxLength)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("   - Input must be a non-empty string "));
                    Console.ResetColor();
                }
            } while (string.IsNullOrWhiteSpace(input) || input.Length > maxLength);

            return input;
        }

        public static int PromptForValidInt(string message)
        {
            int number;
            string? input;
            do
            {
                Console.Write(message);
                input = Console.ReadLine();
                if (!int.TryParse(input, out number))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("   - Input must be an Integer.");
                    Console.ResetColor();
                }
            } while (!int.TryParse(input, out number));

            return number;
        }

        public static string PromtAndValidateEmail(string message)
        {
            string? email;
            do
            {
                Console.Write(message);
                email = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(email) || !ValidateEmail(email))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("   - Invalid email address format.");
                    Console.ResetColor();
                }
            } while (string.IsNullOrWhiteSpace(email) || !ValidateEmail(email));

            return email;
        }

        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }


        public static string GetInput(string prompt, string errorMessage)
        {
            string input;
            while (true)
            {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) return input;
                Console.WriteLine(errorMessage);
            }
        }

        public static string GetImage(string prompt)
        {
            Console.WriteLine("Enter Image Path");
            string? imageData = Console.ReadLine();
            while (string.IsNullOrEmpty(imageData) || !IsValidImagePath(imageData))
            {
                Console.WriteLine("Enter Image Path");
                imageData = Console.ReadLine();
            }
            string imgData = ConvertImageToBase64String(imageData);
            return imgData;
        }

        public static string ConvertImageToBase64String(string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }

        public static bool IsValidImagePath(string imagePath)
        {
            // Check if the file exists
            if (!File.Exists(imagePath))
            {
                return false;
            }

            // Get the file extension
            string extension = Path.GetExtension(imagePath)?.ToLower();

            // Define a list of valid image extensions
            string[] validExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };

            // Check if the file extension is valid
            return validExtensions.Contains(extension);
        }


        public static string GetValidIpAddress()
        {
            string ipAddress;
            while (true)
            {
                Console.Write("Enter IP Address: ");
                ipAddress = Console.ReadLine();
                if (IsValidIp(ipAddress)) return ipAddress;
                Console.WriteLine("Invalid IP Address. Please enter a valid IP.");
            }
        }

        public static int GetValidInteger(string prompt, string errorMessage, bool allowZero = false)
        {
            int result;
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out result) && (allowZero || result != 0)) return result;
                Console.WriteLine(errorMessage);
            }
        }

        public static DateTime GetValidDateTime(string prompt)
        {
            DateTime dateTime;
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParseExact(Console.ReadLine(), "dd-MM-yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dateTime))
                    return dateTime;
                Console.WriteLine("Invalid input for date. Please use the format DD-MM-YYYY.");
            }
        }

        public static bool IsValidIp(string ipAddress)
        {
            string[] parts = ipAddress.Split('.');


            if (parts.Length != 4) return false;

            foreach (string part in parts)
            {

                if (!int.TryParse(part, out int num)) return false;


                if (num < 0 || num > 255) return false;


                if (part.Length > 1 && part.StartsWith("0")) return false;
            }


            return true;
        }



        public static int GetValidPort(string port)
        {
            int ans;
            // Loop until a valid 4-digit integer is entered
            Console.WriteLine("Enter the valid input for it");
            while (!int.TryParse(Console.ReadLine(), out ans) || ans < 1000 || ans > 9999)
            {
                Console.WriteLine("Invalid input, please re-enter a 4-digit port number:");
            }
            return ans;
        }
        public static string GetPassword(string prompt)
        {
            Console.Write(prompt);
            StringBuilder password = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b"); // Remove '*' on backspace
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            return password.ToString();
        }

        public static bool IsValidPassword(string password)
        {
            // At least 8 characters, 1 uppercase, 1 lowercase
            return password.Length >= 8 &&
                   Regex.IsMatch(password, "[A-Z]") &&   // At least one uppercase letter
                   Regex.IsMatch(password, "[a-z]");     // At least one lowercase letter
        }

    }
}
