using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;

namespace Common.Utils.Tests
{
    [TestClass()]
    public class PreferredTeamTests
    {
        [TestMethod()]
        [ExpectedException(typeof(KeyNotFoundException),
            "Such team colour enum does not exist")]
        public void GetTeamColourTest()
        {
            Assert.AreEqual(TeamColour.blue, PreferredTeam.GetTeamColour("blue"));
            Assert.AreEqual(TeamColour.red, PreferredTeam.GetTeamColour("red"));

            Assert.AreEqual(TeamColour.blue, PreferredTeam.GetTeamColour("BLUE"));
            Assert.AreEqual(TeamColour.red, PreferredTeam.GetTeamColour("RED"));

            Assert.AreEqual(TeamColour.blue, PreferredTeam.GetTeamColour("BlUe"));
            Assert.AreEqual(TeamColour.red, PreferredTeam.GetTeamColour("ReD"));

            Assert.AreNotEqual(TeamColour.red, PreferredTeam.GetTeamColour("this_is_not_a_colour"));
            Assert.AreNotEqual(TeamColour.red, PreferredTeam.GetTeamColour(""));
        }
    }
}