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
        private string[] input;
        private FileManager fileManager;
        private RandomNumberGenerator rng;

        // CONSTRUCTUR 1
        #region constructor
        public PasswordManager(string[] args)
        {
            input = args;
            fileManager = new FileManager();
            rng = RandomNumberGenerator.Create();
        }
        #endregion

        // FUNCTION HANDLE INPUT
        #region handle input
        public void HandleInput()
        {
            try {
                // DETERMINE OPERATION
                switch (input[0].ToLower())
                {
                    case "init":
                    if (input.Length == 3)
                        cmdInit(input);
                    else
                        Console.WriteLine("Invalid INIT input format.");
                    break;

                    case "create":
                    if (input.Length == 3)
                        cmdCreate(input);
                    else
                        Console.WriteLine("Invalid CREATE input format.");
                    break;

                    case "get":
                    if ((input.Length == 3) || (input.Length == 4))
                        cmdGet(input);
                    else
                        Console.WriteLine("Invalid GET input format.");
                    break;

                    case "set":
                    if ((input.Length == 4) || (input.Length == 5))
                        cmdSet(input);
                    else
                        Console.WriteLine("Invalid SET input format.");
                    break;

                    case "delete":
                    if (input.Length == 4)
                        cmdDelete(input);
                    else
                        Console.WriteLine("Invalid DELETE input format.");
                    break;

                    case "secret":
                    if (input.Length == 2)
                        cmdSecret(input);
                    else
                        Console.WriteLine("Invalid SECRET input format.");
                    break;

                    default:
                    Console.WriteLine("Invalid command input");
                    break;
                }
            } catch (Exception e) {
                Console.WriteLine($"Something went wrong.\n\nException thrown: {e.Message}");
            }
        }
        #endregion

        // FUNCTION COMMAND INIT 
        #region init
        private void cmdInit(string[] command)
        {
            string masterPwd;
            string iv;
            string secretKey;
            string vaultOutput;
            string clientOutput;
            string serverOutput;
            string emptyVault;
            byte[] skBytes;

            Rfc2898DeriveBytes key1;

            Dictionary<string, string> clientDict = new Dictionary<string, string>();
            Dictionary<string, string> serverDict = new Dictionary<string, string>();
            Dictionary<string, string> vaultDict = new Dictionary<string, string>();
            
            //Console.WriteLine("Running INIT command\n");

            // GET USER PASSWORD INPUT
            //masterPwd = "12345678";
            masterPwd = PasswordPrompt();

            // GENERATE SECRET KEY
            skBytes = new byte[16];
            rng.GetBytes(skBytes);
            secretKey = Convert.ToBase64String(skBytes);

            // PRINT SECRET KEY TO CONSOLE
            //Console.WriteLine($"Your generated Secret Key is:\n\n{secretKey}\n");
            Console.WriteLine(secretKey);

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
                emptyVault = JsonSerializer.Serialize(vaultDict);
                vaultOutput = Convert.ToBase64String(EncryptStringToBytes_Aes(emptyVault, aesAlg.Key, aesAlg.IV));
            }

            // CREATE CLIENT OUTPUT OBJECT
            clientDict.Add("secret", secretKey);
            clientOutput = JsonSerializer.Serialize(clientDict);

            // CREATE SERVER OUTPUT OBJECT
            serverDict.Add("vault", vaultOutput);
            serverDict.Add("iv", iv);
            serverOutput = JsonSerializer.Serialize(serverDict);

            // CREATE CLIENT FILE
            fileManager.WriteFile(command[1], clientOutput);

            // CREATE SERVER VAULT FILE
            fileManager.WriteFile(command[2], serverOutput);
        }
        #endregion

        // FUNCTION COMMAND CREATE
        #region create
        private void cmdCreate(string[] command)
        {
            string masterPwd;
            string secretKey;
            string clientOutput;

            Dictionary<string, string> clientDict = new Dictionary<string, string>();
            Dictionary<string, string> serverDict = new Dictionary<string, string>();
            Dictionary<string, string> vaultDict = new Dictionary<string, string>();
            
            //Console.WriteLine("Running CREATE command\n");

            // MASTER PASSWORD PROMPT
            masterPwd = PasswordPrompt();

            // SECRET KEY PROMPT
            //secretKey = "ke3e3olAxRkxklKBge7H/w==";
            secretKey = PasswordPrompt("Secret Key");

            // ATTEMPT DECRYPTION
            try {
                // ACCESS SERVER FILE
                serverDict = AccessServerFile(command[2]);

                // ACCESS SERVER VAULT
                vaultDict = AccessServerVault(command[2], masterPwd, secretKey);

                // CREATE CLIENT OUTPUT OBJECT
                clientDict.Add("secret", secretKey);
                clientOutput = JsonSerializer.Serialize(clientDict);

                // CREATE CLIENT FILE
                fileManager.WriteFile(command[1], clientOutput);
            }
            // FAILURE
            catch (Exception e) {
                Console.WriteLine($"CREATE command failed.\n\nException thrown: {e.Message}");
            }            
        }
        #endregion

        // FUNCTION COMMAND GET 
        #region get
        private void cmdGet(string[] command)
        {
            string masterPwd;

            Dictionary<string, string> clientDict;
            Dictionary<string, string> serverDict;
            Dictionary<string, string> vaultDict;
            
            //Console.WriteLine("Running GET command\n");
            
            // MASTER PASSWORD PROMPT
            //masterPwd = "12345678";
            masterPwd = PasswordPrompt();

            try {
                // ACCESS CLIENT FILE
                clientDict = AccessClientFile(command[1]);

                // ACCESS SERVER FILE
                serverDict = AccessServerFile(command[2]);

                // ACCESS SERVER VAULT
                vaultDict = AccessServerVault(command[2], masterPwd, clientDict["secret"]);

                // DETERMINE OPERATION
                if (command.Length == 3) {
                    
                    // PRINT ALL KEYS IN VAULT
                    foreach (var item in vaultDict)
                    {
                        Console.WriteLine(item.Key);
                    }
                } else if (command.Length == 4) {

                    // PRINT CHOSEN PROP PASSWORD
                    Console.WriteLine(vaultDict[command[3]]);
                }
            } catch (Exception e){
                Console.WriteLine($"GET command failed.\n\nException thrown: {e.Message}");
            }
        }
        #endregion

        // FUNCTION COMMAND SET
        #region set
        private void cmdSet(string[] command)
        {
            string masterPwd;
            string propPwd;
            string secretKey;

            Dictionary<string, string> clientDict;
            Dictionary<string, string> serverDict;
            Dictionary<string, string> vaultDict;

            //Console.WriteLine("Running SET command\n");

            // MASTER PASSWORD PROMPT
            //masterPwd = "12345678";
            masterPwd = PasswordPrompt();

            // ATTEMPT OPERATION
            try {
                // ACCESS CLIENT FILE
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
                    vaultDict[command[3]] = propPwd;

                    // RE-ENCRYPT PASSWORD VAULT
                    WriteServerFile(command[2], vaultDict, masterPwd, secretKey);
                } 
                else if ((command.Length == 5) && ((command[4] == "-g") || (command[4] == "--generate")))
                {
                    // GENERATE PASSWORD
                    propPwd = GeneratePassword();

                    // STORE IN DICTIONARY
                    vaultDict[command[3]] = propPwd;

                    // RE-ENCRYPT PASSWORD VAULT
                    WriteServerFile(command[2], vaultDict, masterPwd, secretKey);
                } else {
                    Console.WriteLine("Invalid SET command format.");
                }
            // FAILURE
            } catch (Exception e) {
                Console.WriteLine($"SET command failed.\n\nException thrown: {e.Message}");
            }
        }
        #endregion

        // FUNCTION COMMAND DELETE 
        #region delete
        private void cmdDelete(string[] command)
        {
            string masterPwd;
            string secretKey;

            Dictionary<string, string> clientDict;
            Dictionary<string, string> serverDict;
            Dictionary<string, string> vaultDict;

            //Console.WriteLine("Running DELETE command\n");

            // MASTER PASSWORD PROMPT
            //masterPwd = "12345678";
            masterPwd = PasswordPrompt();

            try {
                // ACCESS CLIENT FILE
                clientDict = AccessClientFile(command[1]);

                // ACCESS SERVER FILE
                serverDict = AccessServerFile(command[2]);

                // ACCESS SERVER VAULT
                vaultDict = AccessServerVault(command[2], masterPwd, clientDict["secret"]);

                // GET SECRET KEY
                secretKey = clientDict["secret"];

                // PERFORM OPERATION
                vaultDict.Remove(command[3]);

                // RE-ENCRYPT PASSWORD VAULT
                WriteServerFile(command[2], vaultDict, masterPwd, secretKey);
            // FAILURE
            } catch (Exception e) {
                Console.WriteLine($"DELETE command failed.\n\nException thrown: {e.Message}");
            }
        }
        #endregion

        // FUNCTION COMMAND SECRET 
        #region secret
        private void cmdSecret(string[] command)
        {
            try {
                // PRINT CLIENT SECRET KEY
                Dictionary<string, string> clientDict = AccessClientFile(command[1]);
                Console.WriteLine(clientDict["secret"]);
            // FAILURE
            } catch (Exception e) {
                Console.WriteLine($"SECRET command failed.\n\nException thrown: {e.Message}");
            }
        }
        #endregion

        // FUNCTION PASSWORD PROMPT
        #region password prompt
        private string PasswordPrompt(string value = "Master Password", int minLength = 1)
        {
            string outString;

            // CREATE PROMPT STRING
            string prompt = $"Please input your {value}";

            // CHECK FOR ADDITION TO PROMPT STRING
            if (minLength > 1)
                prompt += $" (minimum length {minLength})";

            prompt += ": ";

            // GET USER PASSWORD INPUT
            do {
                Console.Write(prompt);

                // READ CONSOLE INPUT
                //masterPwd = "12345678";
                outString = Console.ReadLine();

                // CHECK PASSWORD LENGTH
                if (outString.Length < minLength) {
                    Console.WriteLine($"{value} too short...");
                }
                Console.WriteLine();
            } while (outString.Length < minLength);

            return outString;
        }
        #endregion

        // FUNCTION GENERATE PASSWORD
        #region generate password
        private string GeneratePassword(int length = 20)
        {
            Random random = new Random();
            string[] validChars = {"abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "1234567890"};
            string output = "";
            int j;

            // ADD RANDOM VALID CHAR TO OUT STRING
            for(int i = 0; i < length; i++)
            {
                j = random.Next(validChars.Length);
                output += validChars[j][random.Next(validChars[j].Length)];
            }

            return output;
        }
        #endregion

        // FUNCTION ACCESS CLIENT FILE
        #region access client file
        private Dictionary<string, string> AccessClientFile(string path)
        {
            // CHECK IF FILE EXISTS
            if (!File.Exists(path)) {
                throw new FileNotFoundException($"Client file not found at path: {path}.");
            }

            string clientString = fileManager.ReadFile(path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(clientString);
        }
        #endregion

        // FUNTION ACCESS SERVER FILE
        #region access server file
        private Dictionary<string, string> AccessServerFile(string path)
        {
            // CHECK IF FILE EXISTS
            if (!File.Exists(path)) {
                throw new FileNotFoundException($"Server file not found at path: {path}.");
            }

            string clientString = fileManager.ReadFile(path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(clientString);
        }
        #endregion

        // FUNTION ACCESS SERVER VAULT
        #region access server vault
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
        #endregion

        // FUNCTION WRITE SERVER FILE
        #region write server file
        private void WriteServerFile(string path, Dictionary<string, string> vaultDict, string masterPwd, string secretKey)
        {
            string vaultString;
            string vaultOutput;
            string serverOutput;
            Dictionary<string, string> serverDict = AccessServerFile(path);

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
            serverDict["vault"] = vaultOutput;
            serverOutput = JsonSerializer.Serialize(serverDict);

            // CREATE SERVER VAULT FILE
            fileManager.WriteFile(path, serverOutput);
        }
        #endregion

        // FUNCTION ENCRYPT STRING TO BYTES © MICROSOFT
        #region encrypt
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted = new byte[0];

            try {
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
            } catch (Exception e){
                throw new Exception($"Encryption failed.\n\nException thrown: {e.Message}");
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
        #endregion

        // FUNCTION DECRYPT BYTES TO STRING © MICROSOFT
        #region decrypt
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

            try {
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
            } catch (Exception e) {
                throw new Exception($"Decryption failed.\n\nException thrown: {e.Message}");
            }

            return plaintext;
        }
        #endregion
    }
}
