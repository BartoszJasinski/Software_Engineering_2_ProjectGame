using System.Net.Sockets;
using Common.Config;
using Common.Connection;

namespace Player.Net
{
    internal class PlayerMessageHandleArgs
    {
        public IConnection Connection;
        public Socket Socket;
        public PlayerSettings Settings;

        public PlayerMessageHandleArgs(IConnection connection=null, Socket socket=null, PlayerSettings settings=null)
        {
            Connection = connection;
            Socket = socket;
            Settings = settings;
        }
    }
}
