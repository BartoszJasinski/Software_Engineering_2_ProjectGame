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
        public ulong Id { get; set; }
        public string Guid { get; }
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

        public Player(ulong id, string guid, Team team, uint x = 0, uint y = 0)
        {
            Id = id;
            Guid = guid;
            Team = team;
            Location = new Location();
            X = x;
            Y = y;
        }

        public Player(ulong id, string guid, Team team, Location location)
        {
            Id = id;
            Guid = guid;
            Team = team;
            Location = location;
        }
    }
}
