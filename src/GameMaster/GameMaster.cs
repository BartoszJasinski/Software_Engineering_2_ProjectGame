using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Connection;
using Common.IO.Console; 
using GameMaster.Net;

namespace GameMaster
{
    class GameMaster
    {
        //adres post metoda connect
        static void Main(string[] args)
        {
            AgentCommandLineOptions options = CommandLineParser.ParseArgs<AgentCommandLineOptions>(args, new AgentCommandLineOptions());

            GameMasterClient client = new GameMasterClient(new Connection(options.Address, options.Port));
            client.Connect();
            client.Disconnect();
        }
    }
}
