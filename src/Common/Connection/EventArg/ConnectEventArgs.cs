using System;
using System.Net.Sockets;

namespace Common.Connection.EventArg
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
