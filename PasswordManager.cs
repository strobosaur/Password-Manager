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

        // CONSTRUCTUR 1
        public PasswordManager(string[] args)
        {
            input = args;

            inputManager = new InputManager();
            fileManager = new FileManager();

            rng = RandomNumberGenerator.Create();
        }

        // FUNCTION HANDLE INPUT
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
            string masterPwd;
            string iv;
            string secretKey;
            string vaultOutput;
            string clientOutput;
            string serverOutput;
            byte[] skBytes;

            Rfc2898DeriveBytes key1;

            Dictionary<string, string> clientDict = new Dictionary<string, string>();
            Dictionary<string, string> serverDict = new Dictionary<string, string>();
            Dictionary<string, string> vaultDict = new Dictionary<string, string>();
            
            Console.WriteLine("Running INIT command\n");

            // GET USER PASSWORD INPUT
            do {
                Console.Write("Please input your master password (minimum 8 characters): ");
                masterPwd = "12345678";
                // masterPwd = Console.ReadLine();
                // if (masterPwd.Length < 8) {
                //     Console.WriteLine("Password too short...");
                // }
                Console.WriteLine();
            } while (masterPwd.Length < 8);

            // SECRET KEY
            skBytes = new byte[16];
            rng.GetBytes(skBytes);
            secretKey = Convert.ToBase64String(skBytes);

            // PRINT SECRET KEY TO CONSOLE
            Console.WriteLine("Your generated Secret Key is:\n\n" + secretKey + "\n");

            // CREATE VAULT KEY
            key1 = new Rfc2898DeriveBytes(masterPwd, skBytes);

            // AES ENCRYPTION
            using (Aes aesAlg = Aes.Create())
            {
                // INITIALIZATION VECTOR
                aesAlg.GenerateIV();
                iv = Convert.ToBase64String(aesAlg.IV);

                // AES KEY
                aesAlg.Key = key1.GetBytes(32);

                // ENCRYPT VAULT
                string emptyVault = JsonSerializer.Serialize(vaultDict);
                vaultOutput = Convert.ToBase64String(EncryptStringToBytes_Aes(emptyVault, aesAlg.Key, aesAlg.IV));
            }            

            // CREATE CLIENT OUTPUT OBJECT
            clientDict.Add("secret", secretKey);
            clientOutput = JsonSerializer.Serialize(clientDict);

            // CREATE SERVER OUTPUT OBJECT
            serverDict.Add("vault", vaultOutput);
            serverDict.Add("iv", iv);
            serverOutput = JsonSerializer.Serialize(serverDict);

            // CREATE CLIENT VAULT FILE
            fileManager.WriteFile(command[1], clientOutput);

            // CREATE SERVER VAULT FILE
            fileManager.WriteFile(command[2], serverOutput);
        }

        // FUNCTION COMMAND CREATE 
        private void cmdCreate(string[] command)
        {
            
        }

        // FUNCTION COMMAND GET 
        private void cmdGet(string[] command)
        {
            string masterPwd;
            string secretKey;
            string clientString;
            string serverString;
            string vaultString;

            byte[] skBytes;
            byte[] ivBytes;
            byte[] vaultBytes;

            Rfc2898DeriveBytes key1;

            Dictionary<string, string> clientDict;
            Dictionary<string, string> serverDict;
            Dictionary<string, string> vaultDict;
            
            Console.WriteLine("Running GET command\n");
            
            // MASTER PASSWORD PROMPT
            Console.Write("Please input your Master Password: ");
            //masterPwd = System.Console.ReadLine();
            masterPwd = "12345678";
            Console.WriteLine();

            // SECRET KEY PROMPT
            Console.Write("Please input your Secret Key: ");
            //secretKey = System.Console.ReadLine();
            secretKey = "ke3e3olAxRkxklKBge7H/w==";

            // READ CLIENT & SERVER FILE CONTENTS
            clientString = fileManager.ReadFile(command[1]);
            serverString = fileManager.ReadFile(command[2]);

            // DECODE JSON
            clientDict = JsonSerializer.Deserialize<Dictionary<string, string>>(clientString);
            serverDict = JsonSerializer.Deserialize<Dictionary<string, string>>(serverString);

            // GET BYTES FROM BASE64STRINGS
            skBytes = Convert.FromBase64String(secretKey);
            ivBytes = Convert.FromBase64String(serverDict["iv"]);
            vaultBytes = Convert.FromBase64String(serverDict["vault"]);

            // CREATE VAULT KEY
            key1 = new Rfc2898DeriveBytes(masterPwd, skBytes);

            // ATTEMPT VAULT DECRYPTION
            vaultString = DecryptStringFromBytes_Aes(vaultBytes, key1.GetBytes(32), ivBytes);
            vaultDict = JsonSerializer.Deserialize<Dictionary<string, string>>(vaultString);
            //vaultDict = AccessServerVault(command[2], masterPwd, secretKey);

            // PRINT CONTENTS
            foreach (var item in vaultDict)
            {
                Console.WriteLine(item.Key + ": " + item.Value);
            }
        }

        // FUNCTION COMMAND SET 
        private void cmdSet(string[] command)
        {
            string masterPwd;
            string propPwd;
            string secretKey;

            Dictionary<string, string> clientDict;
            Dictionary<string, string> serverDict;
            Dictionary<string, string> vaultDict;

            Console.WriteLine("Running SET command\n");

            // MASTER PASSWORD PROMPT
            //masterPwd = PasswordPrompt();
            masterPwd = "12345678";

            // GET SECRET KEY
            clientDict = AccessClientFile(command[1]);

            // ACCESS SERVER FILE
            serverDict = AccessServerFile(command[2]);

            // ACCESS SERVER VAULT
            vaultDict = AccessServerVault(command[2], masterPwd, clientDict["secret"]);

            // GET SECRET KEY
            secretKey = clientDict["secret"];

            // PERFORM OPERATION
            if (command.Length == 4)
            {
                // ASK FOR PASSWORD TO STORE
                propPwd = PasswordPrompt("Prop Password");

                // STORE IN DICTIONARY
                vaultDict.Add(command[3], propPwd);
            } 
            else if ((command.Length == 5) && ((command[4] == "-g") || (command[4] == "--generate")))
            {
                // GENERATE PASSWORD
                propPwd = GeneratePassword();

                // STORE IN DICTIONARY
                //vaultDict.Add(command[3], propPwd);
                vaultDict[command[3]] = propPwd;
            }

            // RE-ENCRYPT PASSWORD VAULT
            WriteServerFile(command[2], vaultDict, masterPwd, secretKey);
        }

        // FUNCTION COMMAND DELETE 
        private void cmdDelete(string[] command)
        {
            
        }

        // FUNCTION COMMAND SECRET 
        private void cmdSecret(string[] command)
        {
            // PRINT CLIENT SECRET KEY
            Dictionary<string, string> clientDict = AccessClientFile(command[1]);
            Console.WriteLine(clientDict["secret"]);
        }

        // FUNTION MASTER PASSWORD PROMPT
        private string PasswordPrompt(string value = "Master Password (minimum 8 characters)")
        {
            string masterPwd = "";
            // GET USER PASSWORD INPUT
            do {
                Console.Write($"Please input your {value}: ");
                masterPwd = "12345678";
                // masterPwd = Console.ReadLine();
                // if (masterPwd.Length < 8) {
                //     Console.WriteLine("Password too short...");
                // }
                Console.WriteLine();
            } while (masterPwd.Length < 8);

            return masterPwd;
        }

        // FUNTION GENERATE PASSWORD
        private string GeneratePassword(int length = 20)
        {
            Random random = new Random();
            string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string output = "";

            for(int i = 0; i < length; i++)
            {
                output += validChars[random.Next(validChars.Length)];
            }

            int len = output.Length;

            return output;
        }

        // FUNCTION ACCESS CLIENT FILE
        private Dictionary<string, string> AccessClientFile(string path)
        {
            string clientString = fileManager.ReadFile(path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(clientString);
        }

        // FUNTION ACCESS SERVER FILE
        private Dictionary<string, string> AccessServerFile(string path)
        {
            string clientString = fileManager.ReadFile(path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(clientString);
        }

        // FUNTION ACCESS SERVER VAULT
        private Dictionary<string, string> AccessServerVault(string path, string masterPwd, string secretKey)
        {
            // READ SERVER FILE
            Dictionary<string, string> serverDict = AccessServerFile(path);

            // GET BYTES FROM BASE64STRINGS
            byte[] skBytes = Convert.FromBase64String(secretKey);
            byte[] ivBytes = Convert.FromBase64String(serverDict["iv"]);
            byte[] vaultBytes = Convert.FromBase64String(serverDict["vault"]);

            // RECREATE VAULT KEY
            Rfc2898DeriveBytes key1 = new Rfc2898DeriveBytes(masterPwd, skBytes);

            // ATTEMPT VAULT DECRYPTION
            return JsonSerializer.Deserialize<Dictionary<string, string>>(DecryptStringFromBytes_Aes(vaultBytes, key1.GetBytes(32), ivBytes));
        }

        // FUNCTION WRITE SERVER FILE
        private void WriteServerFile(string path, Dictionary<string, string> vaultDict, string masterPwd, string secretKey)
        {
            string vaultString;
            string vaultOutput;
            string serverOutput;
            string serverString = fileManager.ReadFile(path);
            Dictionary<string, string> serverDict = JsonSerializer.Deserialize<Dictionary<string, string>>(serverString);

            // CREATE VAULT KEY
            Rfc2898DeriveBytes key1 = new Rfc2898DeriveBytes(masterPwd, Convert.FromBase64String(secretKey));

            // AES ENCRYPTION
            using (Aes aesAlg = Aes.Create())
            {
                // AES KEY
                aesAlg.Key = key1.GetBytes(32);
                aesAlg.IV = Convert.FromBase64String(serverDict["iv"]);

                // ENCRYPT VAULT
                vaultString = JsonSerializer.Serialize(vaultDict);
                vaultOutput = Convert.ToBase64String(EncryptStringToBytes_Aes(vaultString, aesAlg.Key, aesAlg.IV));
            }

            // CREATE SERVER OUTPUT OBJECT
            //serverDict.Add("vault", vaultOutput);
            serverDict["vault"] = vaultOutput;
            serverOutput = JsonSerializer.Serialize(serverDict);

            // CREATE SERVER VAULT FILE
            fileManager.WriteFile(path, serverOutput);
        }

        // FUNCTION ENCRYPT STRING TO BYTES
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        // FUNCTION DECRYPT BYTES TO STRING
        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
