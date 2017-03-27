using System.IO;
using CommandLine;

namespace Common.IO.Console
{
    public class AgentCommandLineOptions: ICommandLineOptions
    {
        [Option('a', "address", Required = true, HelpText = "server IPv4 address or IPv6 address or host name")]
        public string Address { get; set; }

        [Option('p', "port", HelpText = "server port number")]
        public int Port { get; set; }

        [Option('c', "conf", Required = false, HelpText = "configuration file")]
        public string Conf { get; set; }

    }

 
}
