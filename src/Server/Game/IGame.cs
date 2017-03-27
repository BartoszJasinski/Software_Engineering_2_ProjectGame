namespace Server.Game
{
    public interface IGame
    {
        int Id { get; set; }
        string Name { get; set; }
        int BlueTeamPlayersCount { get; set; }
        int RedTeamPlayersCount { get; set; }
    }
}