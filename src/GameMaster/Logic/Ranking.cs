using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.SchemaWrapper;
using Common.Schema;

namespace GameMaster.Logic
{
    public class Ranking : IRanking
    {
        private IDictionary<TeamColour, int> numberOfWinsDict = new Dictionary<TeamColour, int>();

        public void AddTeam(Team team)
        {
            if (!numberOfWinsDict.ContainsKey(team.Color))
                numberOfWinsDict[team.Color] = 0;
        }

        public void AddWinForTeam(Team team)
        {
            if (!numberOfWinsDict.ContainsKey(team.Color))
                numberOfWinsDict[team.Color] = 0;
            numberOfWinsDict[team.Color]++;
        }

        public void Print()
        {
            int i = 1;
            string[] rankingStrings = numberOfWinsDict.OrderByDescending(n => n.Value).Select(n => $"{i++}. {n.Key} Team - {n.Value} Wins").ToArray();
            Console.WriteLine(string.Join("\n", rankingStrings).DrawInConsoleBox());
        }
    }
}
