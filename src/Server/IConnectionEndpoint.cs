using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public interface IConnectionEndpoint
    {
        int Port { get; set; }
        void Listen();
    }
}
