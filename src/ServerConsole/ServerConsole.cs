using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.IO.Console;
using Server;
using Server.Connection;

namespace ServerConsole
{
    class ServerConsole
    {
        static void Main(string[] args)
        {
            ServerCommandLineOptions options = CommandLineParser.ParseArgs<ServerCommandLineOptions>(args, new ServerCommandLineOptions());


            IConnectionEndpoint endpoint = new ConnectionEndpoint(options.Port);

            CommunicationServer server = new CommunicationServer(endpoint);

            server.Start();
        }
    }
}
