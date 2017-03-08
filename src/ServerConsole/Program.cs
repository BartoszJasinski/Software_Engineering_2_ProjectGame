using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;

namespace ServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            IConnectionEndpoint endpoint = new ConnectionEndpoint(int.Parse(args[0]));

            CommunicationServer server = new CommunicationServer(endpoint);

            server.Start();
        }
    }
}
