using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Common.Connection;
using Common.IO.Console;
using Common.IO.Net;
using Common.Message;
using Common.Schema;
using MockPlayer.Net;


namespace MockPlayer
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