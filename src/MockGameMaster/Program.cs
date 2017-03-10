using Common.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockGameMaster
{
    class Program
    {
        //adres post metoda connect
        static void Main(string[] args)
        {
            AsynchronousClient client = new AsynchronousClient(new Connection("192.168.0.19", 12612));
            client.Connect();
            client.Disconnect();
        }
    }
}
