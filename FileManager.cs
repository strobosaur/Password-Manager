using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Password_Manager
{
    class FileManager
    {
        string pathClient = @"client.txt";
        string pathServer = @"server.txt";

        void WriteFile(string input, string path)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, input);
            }
        }

        string ReadFile(string path)
        {
            string result = "";

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
            }

            return result;
        }
    }
}
