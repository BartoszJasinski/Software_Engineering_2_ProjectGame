using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.IO.Console;
using Common.Schema;
using Player.Net;
using Common.Schema;


namespace PlayerTests
{
    [TestClass]
    public class BehaviorChooserTests
    {
        [TestMethod]
        public void HandleMessageData_TaskField_UpdateSomeFields()
        {
            Player.Net.Game game = new Player.Net.Game();
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions(), game);
           
            player.Game.Fields = new Common.SchemaWrapper.Field[10, 10];
            Data data = new Data() { TaskFields = new TaskField[] { new TaskField() { playerId = 0, x = 4, y = 5, distanceToPiece = 5 }, new TaskField() { playerId = 0, x = 2, y = 3, distanceToPiece = 7 } } };
            player.Handler.HandleMessage(data);

            Assert.AreEqual((int)5, (player.Game.Fields[4, 5] as Common.SchemaWrapper.TaskField).DistanceToPiece);
            Assert.AreEqual((int)7, (player.Game.Fields[2, 3] as Common.SchemaWrapper.TaskField).DistanceToPiece);
        }

        [TestMethod]
        public void HandleMessageData_GoalField_UpdateSomeFields()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions(), new Player.Net.Game());
            player.Game.Fields = new Common.SchemaWrapper.Field[10, 10];
            Data data = new Data() { GoalFields = new GoalField[] { new GoalField() { playerId = 0, x = 4, y = 5, team = TeamColour.blue }, new GoalField() { playerId = 0, x = 2, y = 3, team = TeamColour.red } } };
            player.Handler.HandleMessage(data);

            Assert.AreEqual(TeamColour.blue, (player.Game.Fields[4, 5] as Common.SchemaWrapper.GoalField).Team);
            Assert.AreEqual(TeamColour.red, (player.Game.Fields[2, 3] as Common.SchemaWrapper.GoalField).Team);
        }

        [TestMethod]
        public void HandleMessage_Game()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions(), new Player.Net.Game());
            Common.Schema.Game game = new Common.Schema.Game() { Board = new GameBoard() { goalsHeight = 4, tasksHeight = 4, width = 3 } };
            player.Handler.HandleMessage(game);

            Assert.AreEqual((uint)3, player.Game.Board.width);
            Assert.AreEqual((uint)4, player.Game.Board.tasksHeight);
            Assert.AreEqual((uint)4, player.Game.Board.goalsHeight);
            Assert.AreEqual(3, player.Game.Fields.GetLength(0));
            Assert.AreEqual(12, player.Game.Fields.GetLength(1));

        }

        [TestMethod]
        public void HandleMessage_ConfirmJoiningGame()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions(), new Player.Net.Game());
            ConfirmJoiningGame message = new ConfirmJoiningGame() { gameId = 4, playerId = 2, privateGuid = "test", PlayerDefinition = new Common.Schema.Player() { team = TeamColour.red } };
            player.Handler.HandleMessage(message);

            Assert.AreEqual((ulong)2, player.Game.Id);
            Assert.AreEqual((ulong)4, player.Game.GameId);
            Assert.AreEqual("test", player.Game.Guid);
            Assert.AreEqual(TeamColour.red, player.Game.Team);
        }

        [TestMethod]
        public void HandleMessageData_Pieces_UpdateInfoAboutPiece()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions(), new Player.Net.Game());
            player.Game.Id = 3;
            player.Game.Fields = new Common.SchemaWrapper.TaskField[10, 10];
            player.Game.Fields[3, 3] = new Common.SchemaWrapper.TaskField();
            Data data = new Data() { Pieces = new Piece[] { new Piece() { id = 1 } } };
            player.Handler.HandleMessage(data);

            Assert.AreEqual((ulong)1, player.Game.Pieces[0].id);
        }

        [TestMethod]
        public void HandleMessageData_Pieces_NewCarriedPiece()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions(), new Player.Net.Game());
            player.Game.Id = 3;
            player.Game.Fields = new Common.SchemaWrapper.TaskField[10, 10];
            player.Game.Fields[3, 3] = new Common.SchemaWrapper.TaskField();
            Data data = new Data() { Pieces = new Common.Schema.Piece[] { new Common.Schema.Piece() { id = 1, playerId = 3, playerIdSpecified = true } } };
            player.Handler.HandleMessage(data);

            Assert.AreEqual((ulong)1, player.CarriedPiece.id);
        }

        [TestMethod]
        public void HandleMessageData_Location_ChangeLocation()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions(), new Player.Net.Game());
            player.Game.Id = 3;
            player.Game.Fields = new Common.SchemaWrapper.TaskField[10, 10];
            player.Game.Fields[3, 3] = new Common.SchemaWrapper.TaskField();
            Data data = new Data() { PlayerLocation = new Location { x = 4, y = 2 } };
            player.Handler.HandleMessage(data);

            Assert.AreEqual((uint)4, player.Game.Location.x);
            Assert.AreEqual((uint)2, player.Game.Location.y);
        }



    }
}
