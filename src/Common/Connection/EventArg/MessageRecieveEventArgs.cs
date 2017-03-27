using System;
using System.Net.Sockets;

namespace Common.Connection.EventArg
{
    public class MessageRecieveEventArgs : EventArgs
    {
        public MessageRecieveEventArgs(string message, Socket handler)
        {
            Message = message;
            Handler = handler;
        }

        public string Message { get; set; }
        public Socket Handler { get; set; }
    }
}
