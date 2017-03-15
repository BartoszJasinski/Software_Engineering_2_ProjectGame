using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Common.Connection;
using Common.IO.Console;
using Common.IO.Net;

namespace MockPlayer
{
    public class MockPlayer
    {
        public static void Main(string[] args)
        {
            CommandLineOptions options = CommandLineParser.ParseArgs(args);

            AsynchronousClient client = new AsynchronousClient(new Connection(options.Address, options.Port));
            client.Connect();
            client.Disconnect();

        }
    }
}