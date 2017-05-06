using System;
using Common.Schema;
using Wrapper = Common.SchemaWrapper;

namespace GameMaster.Log
{
    public interface ILogger : IDisposable
    {
        void Log(params string[] s);
        void Log(GameMessage msg, Wrapper.Player p);
    }
}