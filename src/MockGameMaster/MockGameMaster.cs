using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Connection;
using Common.IO.Console; 
using Common.IO.Net;

namespace MockGameMaster
{
    class MockGameMaster
    {
        //adres post metoda connect
        static void Main(string[] args)
        {
            CommandLineOptions options = CommandLineParser.ParseArgs(args);

            AsynchronousClient client = new AsynchronousClient(new Connection(options.Address, options.Port));
            client.Connect();
            client.Disconnect();
        }
    }
}
