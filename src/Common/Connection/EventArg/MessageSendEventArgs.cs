using System;
using System.Net.Sockets;

namespace Common.Connection.EventArg
{
    public class MessageSendEventArgs : EventArgs
    {
        public MessageSendEventArgs(Socket handler)
        {
            Handler = handler;
        }

        public Socket Handler { get; set; }
    }
}
