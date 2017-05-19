using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Player.Net;

namespace Player.Strategy
{
    public interface IController
    {
        IController Possess(PlayerClient player);
    }
}
