using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Connection;

namespace MockGameMaster
{
    public class AsynchronousClient
    {
        private IConnection connection;

        public AsynchronousClient(IConnection connection)
        {
            this.connection = connection;
        }

        public void Connect()
        {
            connection.StartClient();
        }



    }//class
}//namespace
