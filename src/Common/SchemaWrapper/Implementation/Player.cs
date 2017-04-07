using Common.Schema;

namespace Common.SchemaWrapper
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
            set { Location.x = value; }
        }
        public uint Y
        {
            get { return Location.y; }
            set { Location.y = value; }
        }
        public virtual Common.Schema.Player SchemaPlayer
        {
            get
            {
                return new Common.Schema.Player()
                {
                    id = Id,
                    team = Team.Color,
                    type = PlayerType.member
                };
            }
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
