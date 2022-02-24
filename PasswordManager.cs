using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace Password_Manager
{
    class PasswordManager
    {
        string[] input;

        InputManager inputManager;
        FileManager fileManager;

        private RandomNumberGenerator rng;
        private Rfc2898DeriveBytes rfc;
        private Aes aes;

        public PasswordManager(string[] args)
        {
            input = args;
            inputManager = new InputManager();
            fileManager = new FileManager();
            rng = RandomNumberGenerator.Create();
            aes = new AesCryptoServiceProvider();
        }

        public void HandleInput()
        {
            switch (input[0].ToLower())
            {
                case "init":
                cmdInit(input);
                break;

                case "create":
                cmdCreate(input);
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
                cmdSecret(input);
                break;

                default:
                Console.WriteLine("Invalid input");
                break;
            }            
        }

        // FUNCTION COMMAND INIT 
        private void cmdInit(string[] command)
        {
            Dictionary<string, string> clientDict = new Dictionary<string, string>();
            Dictionary<string, string> serverDict = new Dictionary<string, string>();
            string masterPwd = "";
            string clientOutput;
            string serverOutput;

            // INITIALIZATION VECTOR
            aes.GenerateIV();
            int iv = BitConverter.ToInt32(aes.IV);

            // SECRET KEY
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int secretKey = BitConverter.ToInt32(bytes, 0);
            
            do {
                System.Console.Write("Please input your master password (minimum 8 characters): ");
                masterPwd = Console.ReadLine();
                if (masterPwd.Length < 8) {
                    Console.WriteLine("Password too short...");
                }
            } while (masterPwd.Length < 8);

            // CREATE CLIENT OUTPUT OBJECT
            clientDict.Add("masterPwd", masterPwd);
            clientDict.Add("iv", Convert.ToString(iv));
            clientOutput = JsonSerializer.Serialize(clientDict);

            // CREATE SERVER OUTPUT OBJECT
            serverDict.Add("SecretKey", Convert.ToString(secretKey));
            serverOutput = JsonSerializer.Serialize(serverDict);

            // CREATE CLIENT VAULT FILE
            fileManager.WriteFile(command[1], clientOutput);

            // CREATE SERVER VAULT FILE
            fileManager.WriteFile(command[2], serverOutput);
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
