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

        private static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

    }
}
