using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;

namespace GameMaster.Logic
{
    public class Team
    {
        public TeamColour Color { get; set; }
        public uint MaxPlayerCount { get; set; }
        //Leader is part of Player list
        public List<Player> Players { get; set; } 
    }
}
