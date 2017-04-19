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

        private GameMasterClient newGameMaster()
        {
            var settings = new GameMasterSettings();
            settings.ActionCosts = new GameMasterSettingsActionCosts()
            {
                DiscoverDelay = 0,
                KnowledgeExchangeDelay = 0,
                MoveDelay = 0,
                PickUpDelay = 0,
                PlacingDelay = 0,
                TestDelay = 0
            };
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
        
        private Common.SchemaWrapper.Player addPlayer(GameMasterClient gm, ulong id, PlayerType role, Common.Schema.TeamColour team, Common.Schema.Location location)
        {
            var message = new JoinGame()
            {
                playerIdSpecified = true,
                playerId = id,
                preferredRole = role,
                teamColour = team
            };
            BehaviorChooser.HandleMessage(message, gm, null);
            var player = gm.Players.Where(p => p.Id == id).Single();
            player.Location = location;
            var oldField = gm.Board.Fields.Cast<Common.SchemaWrapper.Field>().Where(f => f.PlayerId == id).Single();
            oldField.PlayerId = null;
            gm.Board.Fields[location.x, location.y].PlayerId = id;

            return player;
        }

        private Common.SchemaWrapper.Piece addPiece(GameMasterClient gm, Common.Schema.Location location)
        {
            var field = gm.Board.Fields[location.x, location.y];
            gm.PlaceNewPiece(field as Common.SchemaWrapper.TaskField);
            return gm.Pieces.Last();
        }

        [TestMethod]
        public void GivenANewGame_WhenAPlayerJoins_PlayerGetsDesiredTeamAndRole()
        {
            //Arrange
            var gm = newGameMaster();
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
            var gm = newGameMaster();
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

        [TestMethod]
        public void GivenANewGame_WhenAPlayerJoinsFullTeam_PlayerGetsOtherTeam()
        {
            //Arrange
            var gm = newGameMaster();
            var message = new JoinGame()
            {
                playerIdSpecified = true,
                playerId = 1,
                preferredRole = PlayerType.leader,
                teamColour = Common.Schema.TeamColour.blue
            };
            BehaviorChooser.HandleMessage(message, gm, null);
            message.playerId = 2;
            BehaviorChooser.HandleMessage(message, gm, null);
            message.playerId = 3;
            //Act
            BehaviorChooser.HandleMessage(message, gm, null);
            var player = gm.Players.Where(p => p.Id == message.playerId).Single();
            //Assert
            Assert.AreNotEqual(player.Team.Color, message.teamColour);
            Assert.IsNotNull(player as Leader);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlayerMoves_PlayerPositionGetsUpdated()
        {
            //Arrange
            var gm = newGameMaster();
            var initialLocation = new Common.Schema.Location() { x = 1, y = 3 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, initialLocation);
            var message = new Move()
            {
                directionSpecified = true,
                direction = MoveType.up,
                playerGuid = player.Guid
            };
            //Act
            BehaviorChooser.HandleMessage(message, gm, null).Wait();
            //Assert
            Assert.IsNull(gm.Board.Fields[initialLocation.x, initialLocation.y].PlayerId);
            Assert.AreEqual(gm.Board.Fields[initialLocation.x, initialLocation.y + 1].PlayerId,
                player.Id);
            Assert.AreEqual(player.Location.y, initialLocation.y + 1);
            Assert.AreEqual(player.Location.x, initialLocation.x);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlayerMovesOutOfBoard_PlayerPositionDoesNotChange()
        {
            //Arrange
            var gm = newGameMaster();
            var initialLocation = new Common.Schema.Location() { x = 0, y = 3 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, initialLocation);
            var message = new Move()
            {
                directionSpecified = true,
                direction = MoveType.left,
                playerGuid = player.Guid
            };
            //Act
            BehaviorChooser.HandleMessage(message, gm, null).Wait();
            //Assert
            Assert.AreEqual(gm.Board.Fields[initialLocation.x, initialLocation.y].PlayerId,
                player.Id);
            Assert.AreEqual(player.Location.y, initialLocation.y);
            Assert.AreEqual(player.Location.x, initialLocation.x);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlayerMovesIntoEnemyGoalArea_PlayerPositionDoesNotChange()
        {
            //Arrange
            var gm = newGameMaster();
            var initialLocation = new Common.Schema.Location() { x = 0, y = 6 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, initialLocation);
            var message = new Move()
            {
                directionSpecified = true,
                direction = MoveType.up,
                playerGuid = player.Guid
            };
            //Act
            BehaviorChooser.HandleMessage(message, gm, null).Wait();
            //Assert
            Assert.AreEqual(gm.Board.Fields[initialLocation.x, initialLocation.y].PlayerId,
                player.Id);
            Assert.AreEqual(player.Location.y, initialLocation.y);
            Assert.AreEqual(player.Location.x, initialLocation.x);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlayerMovesIntoAnotherPlayer_PlayerPositionDoesNotChange()
        {
            //Arrange
            var gm = newGameMaster();
            var initialLocation = new Common.Schema.Location() { x = 0, y = 3 };
            var initialLocation2 = new Common.Schema.Location() { x = 0, y = 4 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, initialLocation);
            var player2 = addPlayer(gm, 2, PlayerType.leader, Common.Schema.TeamColour.red, initialLocation2);
            var message = new Move()
            {
                directionSpecified = true,
                direction = MoveType.up,
                playerGuid = player.Guid
            };
            //Act
            BehaviorChooser.HandleMessage(message, gm, null).Wait();
            //Assert
            Assert.AreEqual(gm.Board.Fields[initialLocation.x, initialLocation.y].PlayerId,
                player.Id);
            Assert.AreEqual(player.Location.y, initialLocation.y);
            Assert.AreEqual(player.Location.x, initialLocation.x);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlacingAPiece_PieceGetsAdded()
        {
            //Arrange
            var gm = newGameMaster();
            var location = new Common.Schema.Location() { x = 1, y = 4 };
            //Act
            var piece = addPiece(gm, location);
            //Assert
            Assert.IsNotNull((gm.Board.Fields[location.x, location.y]
                as Common.SchemaWrapper.TaskField).PieceId);
            Assert.AreEqual(piece.Location.x, location.x);
            Assert.AreEqual(piece.Location.y, location.y);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlacingAPieceOnAnother_OldPieceDisappears()
        {
            //Arrange
            var gm = newGameMaster();
            var location = new Common.Schema.Location() { x = 1, y = 4 };
            var oldPiece = addPiece(gm, location);
            //Act
            var piece = addPiece(gm, location);
            //Assert
            Assert.IsFalse(gm.Pieces.Where(p => p.Id == oldPiece.Id).Any());
        }

        [TestMethod]
        public void GivenANewGame_WhenPickingUpPiece_PieceDisappearsFromBoardAndGetsPlayerId()
        {
            //Arrange
            var gm = newGameMaster();
            var location = new Common.Schema.Location() { x = 1, y = 4 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, location);
            var piece = addPiece(gm, location);
            var message = new PickUpPiece()
            {
                playerGuid = player.Guid
            };
            //Act
            BehaviorChooser.HandleMessage(message, gm, null).Wait();
            //Assert
            Assert.IsNull((gm.Board.Fields[location.x, location.y]
                as Common.SchemaWrapper.TaskField).PieceId);
            Assert.AreEqual(gm.Pieces.Where(p => p.Id == piece.Id).Single().PlayerId, player.Id);
        }



    }
}
