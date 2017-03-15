using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class SocketExtensions
    { 
        public static IPAddress GetRemoteAddress(this Socket socket)
        {
            return (socket.RemoteEndPoint as IPEndPoint).Address;
        } 
    }
}
