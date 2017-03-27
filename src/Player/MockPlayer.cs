using Common.Connection;
using Common.IO.Console;
using Player.Net;

namespace Player
{
    public class MockPlayer
    {
        public static void Main(string[] args)
        {
            AgentCommandLineOptions options = CommandLineParser.ParseArgs<AgentCommandLineOptions>(args, new AgentCommandLineOptions());

            MockPlayerClient client = new MockPlayerClient(new Connection(options.Address, options.Port));
            client.Connect();
            client.Disconnect();

        }
    }
}