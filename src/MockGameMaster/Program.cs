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
            AsynchronousClient.StartClient(12612, "192.168.111.196");
        }
    }
}
