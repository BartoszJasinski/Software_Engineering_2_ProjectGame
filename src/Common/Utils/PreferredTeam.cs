using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;

namespace Common.Utils
{
    public class PreferredTeam
    {
        private static Dictionary<string, TeamColour> teamColours;

        static PreferredTeam()
        {
            teamColours = new Dictionary<string, TeamColour>();
            teamColours.Add("blue", TeamColour.blue);
            teamColours.Add("red", TeamColour.red);
        }

        public static TeamColour GetTeamColour(string colour)
        {
            TeamColour prefferedTeam = teamColours[colour.ToLower()];
            return prefferedTeam;

        }

    }
}
