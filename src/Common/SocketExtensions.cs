using System.Net;
using System.Net.Sockets;

namespace Common
{
    public static class SocketExtensions
    { 
        public static IPAddress GetRemoteAddress(this Socket socket)
        {
            return (socket.RemoteEndPoint as IPEndPoint)?.Address;
        } 
    }
}
