using System.Collections.Generic;
using System.Net.Sockets;

namespace Server.Game
{
    public interface IGame
    {
        int Id { get; set; }
        string Name { get; set; }
        ulong BlueTeamPlayersCount { get; set; }
        ulong RedTeamPlayersCount { get; set; }

        Socket GameMaster { get; set; }

        bool HasStarted { get; set; }

        IList<IPlayer> Players { get; set; }
    }
}