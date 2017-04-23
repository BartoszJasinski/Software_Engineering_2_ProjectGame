using Common.Connection;
using Common.IO.Console;
using GameMaster.Net;
using Common.Config;
using GameMaster.Log;

namespace GameMaster
{
    class GameMaster
    {
        static void Main(string[] args)
        {
            AgentCommandLineOptions options = CommandLineParser.ParseArgs<AgentCommandLineOptions>(args, new AgentCommandLineOptions());

            GameMasterSettings settings = Configuration.FromFile<GameMasterSettings>(options.Conf);
            GameMasterClient client = new GameMasterClient(new Connection(options.Address, options.Port), settings, 
                new Logger(UniqueNameGenerator.GetUniqueName(settings.GameDefinition.GameName)));
            client.Connect();
            client.Disconnect();
        }
    }
}
