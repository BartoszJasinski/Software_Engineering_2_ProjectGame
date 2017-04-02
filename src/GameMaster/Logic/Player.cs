using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;

namespace GameMaster.Logic
{
    public class Player
    {
        public Player(int id, Team team, uint x = 0, uint y = 0)
        {
            Id = id;
            Team = team;
            X = x;
            Y = y;
        }

        public Player(int id, Team team, Location location)
        {
            Id = id;
            Team = team;
            Location = location;
        }

        public int Id { get; set; }
        public Team Team { get; set; }
        public Location Location { get; set; }
        public uint X
        {
            get { return Location.x; }
            set { Location.x = value;  }
        }
        public uint Y
        {
            get { return Location.y; }
            set { Location.y = value;  }
        }
    }
}
