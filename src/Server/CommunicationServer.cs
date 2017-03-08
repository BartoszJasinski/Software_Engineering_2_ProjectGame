using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class CommunicationServer
    {
        private IConnectionEndpoint connectionEndpoint;

        public CommunicationServer(IConnectionEndpoint connectionEndpoint)
        {
            this.connectionEndpoint = connectionEndpoint;
        }

        public void Start()
        {
            connectionEndpoint.Listen();
        }
    }
}
