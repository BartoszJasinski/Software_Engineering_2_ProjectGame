using System.Net.Sockets;

namespace Server.Game
{
    public class Game : IGame
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ulong BlueTeamPlayersCount { get; set; }
        public ulong RedTeamPlayersCount { get; set; }

        public Socket GameMaster { get; set; }

        public bool HasStarted { get; set; }

        public Game(int gameId=0, string name="", ulong bluePlayers=0, ulong redPlayers=0, Socket gameMaster=null)
        {
            Id = gameId;
            Name = name;
            BlueTeamPlayersCount = bluePlayers;
            RedTeamPlayersCount = redPlayers;
            GameMaster = gameMaster;
        }
    }
}