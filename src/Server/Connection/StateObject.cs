using System.Net.Sockets;
using System.Text;

namespace Server.Connection
{
    public class StateObject
    {
        public Socket Socket { get; set; }

        public const int BUFFER_SIZE = 1024;

        public byte[] buffer = new byte[1024];

        public StringBuilder StringBuilder { get; set; }

        public StateObject()
        {
            StringBuilder = new StringBuilder();
        }
    }
}
