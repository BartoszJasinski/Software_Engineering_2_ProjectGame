using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common.EventArg
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
