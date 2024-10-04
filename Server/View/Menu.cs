using Server.DbAccess;

namespace Server.View
{
    public class Menu
    {
        public static void display(int userId)
        {
            while (true)
            {
                Console.WriteLine("1. Display Machine Table");
                Console.WriteLine("2. Display Transaction Table");
                Console.WriteLine("3. Display User Table");
                Console.WriteLine("4. Insert into Machine Table");
                Console.WriteLine("5. Delete from Machine Table");
                Console.WriteLine("6. Insert into Transaction Table");
                Console.WriteLine("7. Delete from Transaction Table");
                Console.WriteLine("8. Insert into User Table");
                Console.WriteLine("9. Delete from User Table");
                Console.WriteLine("10. Logout");

                int option;
                while (!int.TryParse(Console.ReadLine(), out option))
                {
                    Console.WriteLine("Invalid input. Enter again.");
                }
                switch (option)
                {
                    case 10:
                        Console.Clear();
                        Console.WriteLine("You have SuccessFully Logged Out!!!");
                        return;

                    case 1:
                        MachineServices.displayMachine();
                        break;

                    case 2:
                        TransactionServices.displayTransaction();
                        break;

                    case 3:
                        if (UserAccess.IsUserAdmin(userId))
                        {
                            UserServices.displayusertable();
                        }
                        else
                        {
                            Console.WriteLine("You are not an admin.");
                        }

                        break;

                    case 4:
                        if (UserAccess.IsUserAdmin(userId))
                        {
                            MachineServices.addMachine();
                        }
                        else
                        {
                            Console.WriteLine("You are not an admin.");
                        }

                        break;

                    case 5:
                        if (UserAccess.IsUserAdmin(userId))
                        {
                            MachineServices.deleteMachine();
                        }
                        else
                        {
                            Console.WriteLine("You are not an admin.");
                        }

                        break;

                    case 6:
                        if (UserAccess.IsUserAdmin(userId))
                        {
                            TransactionServices.addTransaction();
                        }
                        else
                        {
                            Console.WriteLine("You are not an admin.");
                        }

                        break;

                    case 7:
                        if (UserAccess.IsUserAdmin(userId))
                        {
                            TransactionServices.deleteTransaction();
                        }
                        else
                        {
                            Console.WriteLine("You are not an admin.");
                        }
                        break;

                    case 8:
                        if (UserAccess.IsUserAdmin(userId))
                        {
                            Console.WriteLine("Inserting into User Table...Enter 0 to return to the main menu or provide a User Name: ");
                            UserServices.addUser();
                        }
                        else
                        {
                            Console.WriteLine("You are not an admin.");
                        }
                        break;

                    case 9:
                        if (UserAccess.IsUserAdmin(userId))
                        {
                            UserServices.deleteuser();
                        }
                        else
                        {
                            Console.WriteLine("You are not an admin.");
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid Input");
                        break;
                }
            }
        }
    }
}
