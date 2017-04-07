using System;
using System.IO;
using System.Linq;
using System.Threading;
using Common.Schema;

namespace GameMaster.Log
{
    class Logger : ILogger
    {
        static ReaderWriterLock locker = new ReaderWriterLock();
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

        public void Log(GameGood msg,  Common.SchemaWrapper.Player p)
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