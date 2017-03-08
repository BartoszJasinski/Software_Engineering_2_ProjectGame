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
            CommunicationServer server = new CommunicationServer(int.Parse(args[0]));

            server.Listen();
        }
    }
}
