using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.IO.Console;
using Server;
using Server.Connection;
using Common.Config;

namespace ServerConsole
{
    class ServerConsole
    {
        static void Main(string[] args)
        {
            ServerCommandLineOptions options = CommandLineParser.ParseArgs<ServerCommandLineOptions>(args, new ServerCommandLineOptions());

            CommunicationServerSettings settings = Configuration.FromFile<CommunicationServerSettings>(options.Conf);
            IConnectionEndpoint endpoint = new ConnectionEndpoint(options.Port);

            CommunicationServer server = new CommunicationServer(endpoint, settings);

            server.Start();
        }
    }
}
