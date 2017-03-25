using Common.Connection;
using Common.IO.Console;
using Player.Net;

namespace Player
{
    public class MockPlayer
    {
        public static void Main(string[] args)
        {
            CommandLineOptions options = CommandLineParser.ParseArgs(args);

            MockPlayerClient client = new MockPlayerClient(new Connection(options.Address, options.Port));
            client.Connect();
            client.Disconnect();

        }
    }
}