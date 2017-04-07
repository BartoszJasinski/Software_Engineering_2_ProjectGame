using System.Net.Sockets;
using Common.Config;
using Common.Connection;
using Common.IO.Console;

namespace Player.Net
{
    public class PlayerMessageHandleArgs
    {
        public IConnection Connection;
        public Socket Socket;
        public PlayerSettings Settings;
        public AgentCommandLineOptions Options;
        public PlayerClient PlayerClient;

        public PlayerMessageHandleArgs(IConnection connection = null, Socket socket = null,
            PlayerSettings settings = null, AgentCommandLineOptions options = null, PlayerClient playerClient =  null)
        {
            Options = options;
            Connection = connection;
            Socket = socket;
            Settings = settings;
            PlayerClient = playerClient;
        }
    }
}