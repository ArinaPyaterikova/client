namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int port = 30000;
            ClientGame game = new ClientGame("10.102.9.193", port);
            game.Start();
        }
    }
}
