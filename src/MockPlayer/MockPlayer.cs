using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Common.Connection;
using MockGameMaster;
using MockPlayer.IO;

namespace MockPlayer
{
    public class MockPlayer
    {
        public static void Main(string[] args)
        {
         //   CommandLineOptions options = CommandLineParser.ParseArgs(args);
            AsynchronousClient client = new AsynchronousClient(new Connection("192.168.0.19", 12196));
            client.Connect();
            client.Disconnect();

        }
    }
}