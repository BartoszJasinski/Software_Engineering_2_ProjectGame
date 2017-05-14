using System;
using System.IO;
using System.Linq;
using System.Threading;
using Common.Schema;
using Game = GameMaster.Net.Game;

namespace GameMaster.Log
{
    class Logger : ILogger
    {
        static ReaderWriterLock locker = new ReaderWriterLock();
        private StreamWriter writer;
        private bool gameEnded;

        public Logger(string filename = "gamemaster.log")
        {
            string gmLogDirName = System.IO.Directory.GetCurrentDirectory() + @"\GMLogs";
            System.IO.Directory.CreateDirectory(gmLogDirName);
            filename = gmLogDirName + @"\" + filename;
            writer = new StreamWriter(filename, true);

            if (new FileInfo(filename).Length == 0)
            {
                writer.WriteLine("Type,Timestamp,\"Game ID\",\"Player ID\",\"Player GUID\",\"Colour\",\"Role\"");
            }
            writer.Flush();
            gameEnded = false;
        }

        public void Dispose()
        {
            writer.Dispose();
        }

        public void Log(params string[] s)
        {
            
            try
            {
                locker.AcquireWriterLock(int.MaxValue);
                writer.WriteLine(String.Join(",", s));
                writer.Flush();
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
        }

        public void Log(GameMessage msg, Common.SchemaWrapper.Player p)
        {
            if (gameEnded)
                return;
            string[] s = {
                msg.GetType().ToString().Split('.').Last(),
                DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                msg.gameId.ToString(),
                p.Id.ToString(),
                msg.playerGuid,
                p.Team.Color.ToString(),
                p.Team.Leader?.Guid == msg.playerGuid ? "leader" : "member"
            };
            Log(s);
        }

        public void LogEndGame(Game game, TeamColour winner)
        {
            if (gameEnded)
                return;
            gameEnded = true;
            string time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            foreach (var pl in game.Players.Where(p => p.Team.Color == winner))
            {
                string[] s = {
                    "Victory",
                    time,
                    game.gameId.ToString(),
                    pl.Id.ToString(),
                    pl.Guid,
                    pl.Team.Color.ToString(),
                    pl.Team.Leader?.Guid == pl.Guid ? "leader" : "member"
                };
                Log(s);
            }
            foreach (var pl in game.Players.Where(p => p.Team.Color != winner))
            {
                string[] s = {
                    "Defeat",
                    time,
                    game.gameId.ToString(),
                    pl.Id.ToString(),
                    pl.Guid,
                    pl.Team.Color.ToString(),
                    pl.Team.Leader?.Guid == pl.Guid ? "leader" : "member"
                };
                Log(s);
            }
        }
    }
}