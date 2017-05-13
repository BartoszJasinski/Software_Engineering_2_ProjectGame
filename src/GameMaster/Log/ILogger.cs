using System;
using Common.Schema;
using Game = GameMaster.Net.Game;
using Wrapper = Common.SchemaWrapper;

namespace GameMaster.Log
{
    public interface ILogger : IDisposable
    {
        void Log(params string[] s);
        void Log(GameMessage msg, Wrapper.Player p);
        void LogEndGame(Game game, TeamColour winner);
    }
}