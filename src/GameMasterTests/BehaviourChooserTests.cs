using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMaster.Net;
using Common.Config;
using Common.Schema;
using System.Linq;
using Common.SchemaWrapper;
using GameMaster.Logic;
using System.Net.Sockets;

namespace GameMasterTests
{
    [TestClass]
    public class BehaviourChooserTests
    {

        private GameMasterClient newGameMaster(GameMaster.Net.Game game)
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
            settings.GameDefinition.Goals = new Common.Config.GoalField[]
            {
                new Common.Config.GoalField()
                {
                    x = 0,
                    y = 0
                }
            };
            return new GameMasterClient(new ConnectionMock(), settings, new MockLogger(), game, new Ranking());
        }

        private Common.SchemaWrapper.Player addPlayer(GameMasterClient gm,ulong id, PlayerType role, Common.Schema.TeamColour team, Common.Schema.Location location, GameMaster.Net.Game game)
        {
            var message = new JoinGame()
            {
                playerIdSpecified = true,
                playerId = id,
                preferredRole = role,
                preferredTeam = team
            };
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType()));
            var player = game.Players.Where(p => p.Id == id).Single();
            player.Location = location;
            var oldField = game.Board.Fields.Cast<Common.SchemaWrapper.Field>().Where(f => f.PlayerId == id).Single();
            oldField.PlayerId = null;
            game.Board.Fields[location.x, location.y].PlayerId = id;

            return player;
        }

        private Common.SchemaWrapper.Piece addPiece(Common.Schema.Location location, GameMaster.Net.Game game)
        {

            var field = game.Board.Fields[location.x, location.y];
            game.PlaceNewPiece(field as Common.SchemaWrapper.TaskField);
            return game.Pieces.Last();
        }

        [TestMethod]
        public void GivenANewGame_WhenAPlayerJoins_PlayerGetsDesiredTeamAndRole()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var message = new JoinGame()
            {
                playerIdSpecified = true,
                playerId = 2,
                preferredRole = PlayerType.leader,
                preferredTeam = Common.Schema.TeamColour.blue
            };
            //Act
           gm.MessageHandler.HandleMessage(message, new System.Net.Sockets.Socket(new SocketType(), new ProtocolType()));
            var player = game.Players.Single();
            //Assert
            Assert.AreEqual(player.Id, message.playerId);
            Assert.AreEqual(player.Team.Color, message.preferredTeam);
            Assert.IsNotNull(player as Leader);
        }

        [TestMethod]
        public void GivenANewGame_WhenAPlayerJoinsTeamWithLeader_PlayerGetsDesiredTeamAndMemberRole()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var message = new JoinGame()
            {
                playerIdSpecified = true,
                playerId = 1,
                preferredRole = PlayerType.leader,
                preferredTeam = Common.Schema.TeamColour.blue
            };
            gm.MessageHandler.HandleMessage(message, new System.Net.Sockets.Socket(new SocketType(), new ProtocolType()));
            message.playerId = 2;
            //Act
            gm.MessageHandler.HandleMessage(message, new System.Net.Sockets.Socket(new SocketType(), new ProtocolType()));
            var player = game.Players.Where(p => p.Id == message.playerId).Single();
            //Assert
            Assert.AreEqual(player.Team.Color, message.preferredTeam);
            Assert.IsNull(player as Leader);
        }

        [TestMethod]
        public void GivenANewGame_WhenAPlayerJoinsFullTeam_PlayerGetsOtherTeam()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var message = new JoinGame()
            {
                playerIdSpecified = true,
                playerId = 1,
                preferredRole = PlayerType.leader,
                preferredTeam = Common.Schema.TeamColour.blue
            };
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType()));
            message.playerId = 2;
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType()));
            message.playerId = 3;
            //Act
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType()));
            var player = game.Players.Where(p => p.Id == message.playerId).Single();
            //Assert
            Assert.AreNotEqual(player.Team.Color, message.preferredTeam);
            Assert.IsNotNull(player as Leader);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlayerMoves_PlayerPositionGetsUpdated()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var initialLocation = new Common.Schema.Location() { x = 1, y = 3 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, initialLocation, game);
            var message = new Move()
            {
                directionSpecified = true,
                direction = MoveType.up,
                playerGuid = player.Guid
            };
            //Act
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType()));
            //Assert
            Assert.IsNull(game.Board.Fields[initialLocation.x, initialLocation.y].PlayerId);
            Assert.AreEqual(game.Board.Fields[initialLocation.x, initialLocation.y + 1].PlayerId,
                player.Id);
            Assert.AreEqual(player.Location.y, initialLocation.y + 1);
            Assert.AreEqual(player.Location.x, initialLocation.x);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlayerMovesOutOfBoard_PlayerPositionDoesNotChange()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var initialLocation = new Common.Schema.Location() { x = 0, y = 3 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, initialLocation, game);
            var message = new Move()
            {
                directionSpecified = true,
                direction = MoveType.left,
                playerGuid = player.Guid
            };
            //Act
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType())).Wait();
            //Assert
            Assert.AreEqual(game.Board.Fields[initialLocation.x, initialLocation.y].PlayerId,
                player.Id);
            Assert.AreEqual(player.Location.y, initialLocation.y);
            Assert.AreEqual(player.Location.x, initialLocation.x);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlayerMovesIntoEnemyGoalArea_PlayerPositionDoesNotChange()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var initialLocation = new Common.Schema.Location() { x = 0, y = 6 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, initialLocation, game);
            var message = new Move()
            {
                directionSpecified = true,
                direction = MoveType.up,
                playerGuid = player.Guid
            };
            //Act
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType())).Wait();
            //Assert
            Assert.AreEqual(game.Board.Fields[initialLocation.x, initialLocation.y].PlayerId,
                player.Id);
            Assert.AreEqual(player.Location.y, initialLocation.y);
            Assert.AreEqual(player.Location.x, initialLocation.x);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlayerMovesIntoAnotherPlayer_PlayerPositionDoesNotChange()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var initialLocation = new Common.Schema.Location() { x = 0, y = 3 };
            var initialLocation2 = new Common.Schema.Location() { x = 0, y = 4 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, initialLocation, game);
            var player2 = addPlayer(gm, 2, PlayerType.leader, Common.Schema.TeamColour.red, initialLocation2, game);
            var message = new Move()
            {
                directionSpecified = true,
                direction = MoveType.up,
                playerGuid = player.Guid
            };
            //Act
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType())).Wait();
            //Assert
            Assert.AreEqual(game.Board.Fields[initialLocation.x, initialLocation.y].PlayerId,
                player.Id);
            Assert.AreEqual(player.Location.y, initialLocation.y);
            Assert.AreEqual(player.Location.x, initialLocation.x);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlacingAPiece_PieceGetsAdded()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var location = new Common.Schema.Location() { x = 1, y = 4 };
            //Act
            var piece = addPiece(location, game);
            //Assert
            Assert.IsNotNull((game.Board.Fields[location.x, location.y]
                as Common.SchemaWrapper.TaskField).PieceId);
            Assert.AreEqual(piece.Location.x, location.x);
            Assert.AreEqual(piece.Location.y, location.y);
        }

        [TestMethod]
        public void GivenANewGame_WhenPlacingAPieceOnAnother_OldPieceDisappears()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var location = new Common.Schema.Location() { x = 1, y = 4 };
            var oldPiece = addPiece(location, game);
            //Act
            var piece = addPiece(location, game);
            //Assert
            Assert.IsFalse(game.Pieces.Where(p => p.Id == oldPiece.Id).Any());
        }

        [TestMethod]
        public void GivenANewGame_WhenPickingUpPiece_PieceDisappearsFromBoardAndGetsPlayerId()
        {
            //Arrange
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);
            var location = new Common.Schema.Location() { x = 1, y = 4 };
            var player = addPlayer(gm, 1, PlayerType.leader, Common.Schema.TeamColour.blue, location, game);
            var piece = addPiece(location, game);
            var message = new PickUpPiece()
            {
                playerGuid = player.Guid
            };
            //Act
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType())).Wait();
            //Assert
            Assert.IsNull((game.Board.Fields[location.x, location.y]
                as Common.SchemaWrapper.TaskField).PieceId);
            Assert.AreEqual(game.Pieces.Where(p => p.Id == piece.Id).Single().PlayerId, player.Id);
        }

        [TestMethod]
        public void HandleMessage_ConfirmGameRegistration_NewGameId()
        {
            GameMaster.Net.Game game = new GameMaster.Net.Game();
            var gm = newGameMaster(game);

            var message = new ConfirmGameRegistration
            {
                gameId = 4
            };
            gm.MessageHandler.HandleMessage(message, new Socket(new SocketType(), new ProtocolType()));

            Assert.AreEqual((ulong)4, game.gameId);
        }

    }
}
