namespace Password_Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            PasswordManager manager = new PasswordManager(args);
            manager.HandleInput();
        }        
    }
}
