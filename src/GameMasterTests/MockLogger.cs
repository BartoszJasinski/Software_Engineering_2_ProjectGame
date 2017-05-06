using GameMaster.Log;
using Common.Schema;

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
    }
}
