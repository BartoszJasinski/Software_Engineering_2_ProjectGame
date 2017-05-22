using Common.SchemaWrapper;
using System;
using System.Net.Sockets;

namespace GameMaster.Net
{
    public class EndGameEventArgs : EventArgs
    {
        public EndGameEventArgs(Team winnerTeam, Team loserTeam)
        {
            WinnerTeam = winnerTeam;
            LoserTeam = loserTeam;
        }

        public Team WinnerTeam { get; set; }
        public Team LoserTeam { get; set; }

        public Socket Handler { get; set; }
    }
}