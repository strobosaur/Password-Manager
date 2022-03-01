using System;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace Password_Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] args2 = {"init", "client.json", "server.json"};
            string[] args3 = {"get", "client.json", "server.json"};
            string[] args4 = {"set", "client.json", "server.json", "hotmail.com", "-g"};
            string[] args5 = {"delete", "client.json", "server.json", "hotmail.com"};
            string[] args6 = {"create", "client2.json", "server.json"};
            string[] args7 = {"set", "client2.json", "server.json", "flashback.org", "-g"};

            // bool run = true;
            PasswordManager manager = new PasswordManager(args7);
            manager.HandleInput();

            // MAIN LOOP
            // do {
                // input = MainMenu();
                // manager = new PasswordManager(input);
                // run = (input[0] != "exit");
            // } while (run);
        }

        // FUNTION MAIN MENU
        static string[] MainMenu()
        {
            // VARIABLES
            bool inputOK = false;
            string input;
            string[] output = new string[0];
            string[] commands = {"init", "create", "get", "set", "delete", "secret", "exit"};

            // PRINT INSTRUCTIONS TO CONSOLE
            System.Console.WriteLine("Input commands in one of the following formats:\n\n"
                                    +"init <client> <server> {<pwd>}\n"
                                    +"create <client> <server> {<pwd>} {< secret>}\n"
                                    +"get <client> <server> [<prop>] {<pwd>}\n"
                                    +"set <client> <server> <prop> [-g] {<pwd>} {<value>}\n"
                                    +"delete <client> <server> <prop> {<pwd>}\n"
                                    +"secret <client>\n"
                                    +"exit\n\n");

            // TRY TO GET INPUT FROM USER
            do {
                try {
                    input = Console.ReadLine();
                    output = input.Split(' ');
                    output = StringArrToLower(output);

                    // CHECK IF COMMAND EXISTS
                    if (!commands.Contains(output[0])) 
                    {
                        throw new InvalidOperationException("Invalid command...");
                    } 
                    // CHECK IF LENGTH OK
                    else if (output.Length < 2) 
                    {
                        throw new InvalidOperationException("Invalid command format...");
                    } 
                    // EVERYTHING FINE
                    else 
                    {
                        inputOK = true;
                    }
                } 
                catch (Exception e) 
                {
                    System.Console.WriteLine($"Invalid input. Exception thrown: {e}");
                }
            } while (!inputOK);

            return output;
        }

        // FUNTION STRING ARRAY TO LOWER
        static string[] StringArrToLower(string[] input)
        {
            string temp;
            for(int i = 0; i < input.Length; i++)
            {
                temp = input[i].ToLower();
                input[i] = temp;
            }

            return input;
        }
    }
}
