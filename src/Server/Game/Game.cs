namespace Server.Game
{
    public class Game : IGame
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BlueTeamPlayersCount { get; set; }
        public int RedTeamPlayersCount { get; set; }

        public Game(int gameId = 0, string name = "", int bluePlayers = 0, int redPlayers = 0)
        {
            Id = gameId;
            Name = name;
            BlueTeamPlayersCount = bluePlayers;
            RedTeamPlayersCount = redPlayers;
        }
    }
}