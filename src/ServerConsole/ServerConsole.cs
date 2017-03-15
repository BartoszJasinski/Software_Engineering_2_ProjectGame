using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;
using Server.Connection;

namespace ServerConsole
{
    class ServerConsole
    {
        static void Main(string[] args)
        {
            IConnectionEndpoint endpoint = new ConnectionEndpoint(12196);

            CommunicationServer server = new CommunicationServer(endpoint);

            server.Start();
        }
    }
}
