using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Connection;
using Common.IO.Console; 
using Common.IO.Net;
using GameMaster.Net;

namespace GameMaster
{
    class MockGameMaster
    {
        //adres post metoda connect
        static void Main(string[] args)
        {
            AgentCommandLineOptions options = CommandLineParser.ParseArgs<AgentCommandLineOptions>(args, new AgentCommandLineOptions());

            MockGameMasterClient client = new MockGameMasterClient(new Connection(options.Address, options.Port));
            client.Connect();
            client.Disconnect();
        }
    }
}
