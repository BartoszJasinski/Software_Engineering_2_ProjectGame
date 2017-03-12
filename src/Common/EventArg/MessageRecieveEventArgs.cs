using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common.EventArg
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
