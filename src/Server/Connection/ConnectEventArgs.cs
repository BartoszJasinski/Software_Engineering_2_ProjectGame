using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Connection
{
    public class ConnectEventArgs : EventArgs
    {
        public ConnectEventArgs(Socket handler)
        {
            Handler = handler;
        }

        public Socket Handler { get; set; } 
    }
}
