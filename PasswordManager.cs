using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Password_Manager
{
    class PasswordManager
    {
        string[] input;
        string initializationVector;
        InputManager inputManager;
        FileManager fileManager;
        private RandomNumberGenerator rng;
        private Rfc2898DeriveBytes rfc;
        private Aes aes;

        public PasswordManager(string[] args)
        {
            input = args;
            InputManager inputManager = new InputManager();
            FileManager fileManager = new FileManager();
        }

        public void HandleInput()
        {
            switch (input[0])
            {
                case "init":
                cmdInit(input);
                break;

                case "get":
                cmdGet(input);
                break;

                case "set":
                cmdSet(input);
                break;

                case "delete":
                cmdDelete(input);
                break;

                case "secret":
                cmdDelete(input);
                break;

                default:
                Console.WriteLine("Invalid input");
                break;
            }
            
        }

        private void cmdInit(string[] command)
        {

        }

        private void cmdCreate(string[] command)
        {
            
        }

        private void cmdGet(string[] command)
        {
            
        }

        private void cmdSet(string[] command)
        {
            
        }

        private void cmdDelete(string[] command)
        {
            
        }

        private void cmdSecret(string[] command)
        {
            
        }
    }
}
