using Common.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMaster.Logic
{
    public class Leader : Player
    {
        public Leader(int id, Team team, uint x = 0, uint y = 0) : base(id, team, x, y)
        {

        }

        public Leader(int id, Team team, Location location) : base(id, team, location)
        {

        }
    }
}
