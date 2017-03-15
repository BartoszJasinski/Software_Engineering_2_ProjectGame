using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace MockPlayer.IO
{
    public class CommandLineOptions
    {
        [Option('a', "address", Required = true, HelpText = "server IPv4 address or IPv6 address or host name")]
        public string Address { get; set; }

        [Option('p', "port", HelpText = "server port number")]
        public int Port { get; set; }

//        [HelpOption(HelpText = "Display this help screen.")]
//        public string GetUsage()
//        {
//            var usage = new StringBuilder();
//            usage.AppendLine("Quickstart Application 1.0");
//            usage.AppendLine("Read user manual for usage instructions...");
//            return usage.ToString();
//        }
    }
}
