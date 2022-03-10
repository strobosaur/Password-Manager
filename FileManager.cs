using System;
using System.IO;

namespace Password_Manager
{
    class FileManager
    {
        // FUNTION WRITE FILE
        #region write file
        public void WriteFile(string path, string input = "", bool overwrite = true)
        {
            try {
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, input);
                } 
                else if (overwrite) 
                {
                    File.WriteAllText(path, input);
                }
            } catch (Exception e) {
                Console.WriteLine($"Write file failed.\n\nException thrown: {e.Message}");
            }
        }
        #endregion

        // FUNTION READ FILE
        #region read file
        public string ReadFile(string path)
        {
            string result = "";

            try {
                if (File.Exists(path))
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string line = "";
                        while((line = sr.ReadLine()) != null)
                        {
                            result += line;
                        }
                    }
                } else {
                    throw new Exception($"File does not exist ({path})");
                }
            } catch (Exception e) {
                Console.WriteLine($"Read file failed.\n\nException thrown: {e.Message}");
            }

            return result;
        }
        #endregion
    }
}
