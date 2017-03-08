using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IConnetion
    {
        void StartClient(int port, string ipString);
        void ConnectCallback(IAsyncResult ar);
        void Receive(Socket client);
        void ReceiveCallback(IAsyncResult ar);
        void Send(Socket client, String data);
        void SendCallback(IAsyncResult ar);


    }
}
