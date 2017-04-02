using System.Net.Sockets;
using Common.Config;
using Common.Connection;
using Common.IO.Console;

namespace Player.Net
{
    internal class PlayerMessageHandleArgs
    {
        public IConnection Connection;
        public Socket Socket;
        public PlayerSettings Settings;
        public AgentCommandLineOptions Options;

        public PlayerMessageHandleArgs(IConnection connection = null, Socket socket = null,
            PlayerSettings settings = null, AgentCommandLineOptions options = null)
        {
            Options = options;
            Connection = connection;
            Socket = socket;
            Settings = settings;
        }
    }
}