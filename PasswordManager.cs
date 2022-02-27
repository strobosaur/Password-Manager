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
            
            // GET USER PASSWORD INPUT
            do {
                System.Console.Write("Please input your master password (minimum 8 characters): ");
                masterPwd = "12345678";
                // masterPwd = Console.ReadLine();
                // if (masterPwd.Length < 8) {
                //     Console.WriteLine("Password too short...");
                // }
            } while (masterPwd.Length < 8);

            // SECRET KEY
            skBytes = new byte[16];
            rng.GetBytes(skBytes);
            secretKey = System.Convert.ToBase64String(skBytes);

            // PRINT SECRET KEY TO CONSOLE
            System.Console.WriteLine("Your generated Secret Key is:\n\n" + secretKey + "\n");

            // CREATE VAULT KEY
            key1 = new Rfc2898DeriveBytes(masterPwd, skBytes);

            // AES ENCRYPTION
            using (Aes aesAlg = Aes.Create())
            {
                // INITIALIZATION VECTOR
                aesAlg.GenerateIV();
                iv = System.Convert.ToBase64String(aesAlg.IV);

                // AES KEY
                aesAlg.Key = key1.GetBytes(32);

                // ENCRYPT VAULT
                vaultOutput = System.Convert.ToBase64String(EncryptStringToBytes_Aes("vault", aesAlg.Key, aesAlg.IV));
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
            
        }

        // FUNCTION COMMAND SET 
        private void cmdSet(string[] command)
        {
            
        }

        // FUNCTION COMMAND DELETE 
        private void cmdDelete(string[] command)
        {
            
        }

        // FUNCTION COMMAND SECRET 
        private void cmdSecret(string[] command)
        {
            
        }
        
        // FUNCTION BASE64 ENCODE
        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        // FUNCTION BASE64 DECODE
        public static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
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
