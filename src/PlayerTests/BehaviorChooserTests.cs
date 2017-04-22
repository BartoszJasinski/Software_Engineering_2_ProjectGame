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
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions());
            player.Fields = new Common.SchemaWrapper.Field[10, 10];
            Data data = new Data() { TaskFields = new TaskField[] { new TaskField() { playerId = 0, x = 4, y = 5, distanceToPiece = 5 }, new TaskField() { playerId = 0, x = 2, y = 3, distanceToPiece = 7 } } };
            Player.Net.BehaviorChooser.HandleMessage(data, new Player.Net.PlayerMessageHandleArgs(null, null, null, null, player));

            Assert.AreEqual((uint)5, (player.Fields[4, 5] as Common.SchemaWrapper.TaskField).DistanceToPiece);
            Assert.AreEqual((uint)7, (player.Fields[2, 3] as Common.SchemaWrapper.TaskField).DistanceToPiece);
        }

        [TestMethod]
        public void HandleMessageData_GoalField_UpdateSomeFields()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions());
            player.Fields = new Common.SchemaWrapper.Field[10, 10];
            Data data = new Data() { GoalFields = new GoalField[] { new GoalField() { playerId = 0, x = 4, y = 5, team = TeamColour.blue }, new GoalField() { playerId = 0, x = 2, y = 3, team = TeamColour.red } } };
            Player.Net.BehaviorChooser.HandleMessage(data, new Player.Net.PlayerMessageHandleArgs(null, null, null, null, player));

            Assert.AreEqual(TeamColour.blue, (player.Fields[4, 5] as Common.SchemaWrapper.GoalField).Team);
            Assert.AreEqual(TeamColour.red, (player.Fields[2, 3] as Common.SchemaWrapper.GoalField).Team);
        }

        [TestMethod]
        public void HandleMessage_Game()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions());
            Game game = new Game() { Board = new GameBoard() { goalsHeight = 4, tasksHeight = 4, width = 3 } };
            Player.Net.BehaviorChooser.HandleMessage(game, new Player.Net.PlayerMessageHandleArgs(null, null, null, null, player));

            Assert.AreEqual((uint)3, player.Board.width);
            Assert.AreEqual((uint)4, player.Board.tasksHeight);
            Assert.AreEqual((uint)4, player.Board.goalsHeight);
            Assert.AreEqual(3, player.Fields.GetLength(0));
            Assert.AreEqual(12, player.Fields.GetLength(1));

        }

        [TestMethod]
        public void HandleMessage_ConfirmJoiningGame()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions());
            ConfirmJoiningGame message = new ConfirmJoiningGame() { gameId = 4, playerId = 2, privateGuid = "test", PlayerDefinition = new Common.Schema.Player() { team = TeamColour.red } };
            Player.Net.BehaviorChooser.HandleMessage(message, new Player.Net.PlayerMessageHandleArgs(null, null, null, null, player));

            Assert.AreEqual((ulong)2, player.Id);
            Assert.AreEqual((ulong)4, player.GameId);
            Assert.AreEqual("test", player.Guid);
            Assert.AreEqual(TeamColour.red, player.Team);
        }

        [TestMethod]
        public void HandleMessageData_Pieces_UpdateInfoAboutPiece()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions());
            player.Id = 3;
            player.Fields = new Common.SchemaWrapper.TaskField[10, 10];
            player.Fields[3, 3] = new Common.SchemaWrapper.TaskField();
            player.Location = new Location() { x = 3, y = 3 };
            player.Pieces = new Piece[] { };
            Data data = new Data() { Pieces = new Piece[] { new Piece() { playerId = 3, playerIdSpecified = true, id = 1 } } };
            Player.Net.BehaviorChooser.HandleMessage(data, new Player.Net.PlayerMessageHandleArgs(null, null, null, null, player));

            Assert.AreEqual((ulong)1, player.Pieces[0].id);
        }

        //[TestMethod]
        //public void HandleMessageData_Pieces_Distance()
        //{
        //    PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions());
        //    player.Id = 3;
        //    player.Fields = new Common.SchemaWrapper.TaskField[10, 10];
        //    player.Fields[3, 3] = new Common.SchemaWrapper.TaskField();
        //    player.Fields[5, 3] = new Common.SchemaWrapper.TaskField();
        //    player.Location = new Location() { x = 3, y = 3 };
        //    Data data = new Data() { TaskFields = new Common.Schema.TaskField[] { new Common.Schema.TaskField() { distanceToPiece = 0 } } } ;
        //    Player.Net.BehaviorChooser.HandleMessage(data, new Player.Net.PlayerMessageHandleArgs(null, null, null, null, player));
        //    player.Location = new Location() { x = 5, y = 3 };

        //    Assert.AreEqual((uint)2, (player.Fields[5, 3] as Common.SchemaWrapper.TaskField).DistanceToPiece);
        //}



    }
}
