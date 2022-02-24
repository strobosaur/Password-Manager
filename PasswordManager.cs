using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.IO;

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
            // aes = new AesCryptoServiceProvider();
            aes = Aes.Create();
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
            string iv = System.Convert.ToBase64String(aes.IV);

            // SECRET KEY
            byte[] bytes = new byte[32];
            rng.GetBytes(bytes);
            string secretKey = System.Convert.ToBase64String(bytes);
            
            do {
                System.Console.Write("Please input your master password (minimum 8 characters): ");
                masterPwd = "12345678";
                // masterPwd = Console.ReadLine();
                // if (masterPwd.Length < 8) {
                //     Console.WriteLine("Password too short...");
                // }
            } while (masterPwd.Length < 8);

            // CREATE CLIENT OUTPUT OBJECT
            clientDict.Add("masterPwd", masterPwd);
            clientDict.Add("iv", iv);
            clientOutput = JsonSerializer.Serialize(clientDict);

            // CREATE SERVER OUTPUT OBJECT
            serverDict.Add("SecretKey", secretKey);
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
        
        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
