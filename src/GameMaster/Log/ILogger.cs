using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common.Schema;
using Wrapper = Common.SchemaWrapper;

namespace GameMaster.Log
{
    public interface ILogger : IDisposable
    {
        void Log(params string[] s);
        void Log(GameMessage msg, Wrapper.Player p);
    }

    class Logger : ILogger
    {
        private StreamWriter writer;
        public Logger(string filename="gamemaster.log")
        {

            writer = new StreamWriter(filename, true);
           
            if (new FileInfo(filename).Length == 0)
            {
                writer.WriteLine("Type,Timestamp,\"Game ID\",\"Player ID\",\"Player GUID\",\"Colour\",\"Role\"");
            }
            writer.Flush();

        }
        public void Dispose()
        {
            writer.Dispose();
        }

        public void Log(params string[] s)
        {
            writer.WriteLine(String.Join(",",s));
            writer.Flush();
        }

        public void Log(GameMessage msg,  Wrapper.Player p)
        {
            
            string[] s = new[]
            {
                msg.GetType().ToString().Split('.').Last(),
                DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                msg.gameId.ToString(),
                p.Id.ToString(),
                msg.playerGuid,
                p.Team.Color.ToString(),
                p.Team.Leader?.Guid==msg.playerGuid?"leader":"member"
            };
            Log(s);
        }
    }
}