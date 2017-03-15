using CommandLine;

namespace Common.IO.Console
{
    public class CommandLineOptions
    {
        [Option('a', "address", Required = true, HelpText = "server IPv4 address or IPv6 address or host name")]
        public string Address { get; set; }

        [Option('p', "port", HelpText = "server port number")]
        public int Port { get; set; }


    }
}
