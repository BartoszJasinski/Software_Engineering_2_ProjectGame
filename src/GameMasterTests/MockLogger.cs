using GameMaster.Log;
using Common.Schema;
using Game = GameMaster.Net.Game;

namespace GameMasterTests
{
    class MockLogger : ILogger
    {
        public void Dispose()
        {

        }

        public void Log(params string[] s)
        {

        }

        public void Log(GameMessage msg, Common.SchemaWrapper.Player p)
        {

        }

        public void LogEndGame(Game game, TeamColour winner)
        {
            
        }
    }
}
