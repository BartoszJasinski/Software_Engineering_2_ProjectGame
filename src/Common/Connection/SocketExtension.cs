using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common.Connection
{
    public static class SocketExtension
    {
        public static IPAddress GetRemoteEndPointAddress(this Socket sk)
        {
            return (sk.RemoteEndPoint as IPEndPoint)?.Address;
        }
    }
}
