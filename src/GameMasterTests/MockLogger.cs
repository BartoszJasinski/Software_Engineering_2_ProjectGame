using GameMaster.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Common.SchemaWrapper;

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

        public void Log(GameGood msg, Common.SchemaWrapper.Player p)
        {

        }
    }
}
