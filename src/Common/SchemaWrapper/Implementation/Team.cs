using System.Collections.Generic;
using Common.Schema;

namespace Common.SchemaWrapper
{
    public class Team
    {
        public TeamColour Color { get; set; }
        public uint MaxPlayerCount { get; set; }
        //Leader is part of Player list
        public List<Player> Players { get; }
        //additional reference to Leader
        public Leader Leader { get; private set; }

        public Team(TeamColour color, uint maxPlayerCount)
        {
            Color = color;
            MaxPlayerCount = maxPlayerCount;
            Players = new List<Player>();
            Leader = null;
        }

        //use this method to add leader, do not do it manually
        public void AddLeader(Leader leader)
        {
            Players.Add(leader);
            Leader = leader;
        }

        public bool IsFull => MaxPlayerCount == Players.Count;
        public bool HasLeader => Leader != null;

    }
}
