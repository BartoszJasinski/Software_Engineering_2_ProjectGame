using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMaster.Net;
using Common.Config;
using Common.Schema;
using System.Linq;
using System.Net.Sockets;
using Common.SchemaWrapper;

namespace GameMasterTests
{
    [TestClass]
    public class BehaviourChooserTests
    {

        private GameMasterClient NewGameMaster()
        {
            var settings = new GameMasterSettings();
            settings.ActionCosts = new GameMasterSettingsActionCosts();
            settings.GameDefinition = new GameMasterSettingsGameDefinition()
            {
                BoardWidth = "3",
                PlacingNewPiecesFrequency = uint.MaxValue,
                GoalAreaLength = "2",
                TaskAreaLength = "5",
                NumberOfPlayersPerTeam = "2"
            };
            return new GameMasterClient(new ConnectionMock(), settings, new MockLogger());
        }

        [TestMethod]
        public void GivenANewGame_WhenAPlayerJoins_PlayerGetsDesiredTeamAndRole()
        {
            //Arrange
            var gm = NewGameMaster();
            var message = new JoinGame()
            {
                playerIdSpecified = true,
                playerId = 2,
                preferredRole = PlayerType.leader,
                teamColour = Common.Schema.TeamColour.blue
            };
            //Act
            BehaviorChooser.HandleMessage(message, gm, null);
            var player = gm.Players.Single();
            //Assert
            Assert.AreEqual(player.Id, message.playerId);
            Assert.AreEqual(player.Team.Color, message.teamColour);
            Assert.IsNotNull(player as Leader);
        }

        [TestMethod]
        public void GivenANewGame_WhenAPlayerJoinsTeamWithLeader_PlayerGetsDesiredTeamAndMemberRole()
        {
            //Arrange
            var gm = NewGameMaster();
            var message = new JoinGame()
            {
                playerIdSpecified = true,
                playerId = 1,
                preferredRole = PlayerType.leader,
                teamColour = Common.Schema.TeamColour.blue
            };
            BehaviorChooser.HandleMessage(message, gm, null);
            message.playerId = 2;
            //Act
            BehaviorChooser.HandleMessage(message, gm, null);
            var player = gm.Players.Where(p => p.Id == message.playerId).Single();
            //Assert
            Assert.AreEqual(player.Team.Color, message.teamColour);
            Assert.IsNull(player as Leader);
        }
    }
}
