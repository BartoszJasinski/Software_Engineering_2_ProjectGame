using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Connection;
using Common.IO.Console;
using Server.Connection;
using Common.Schema;
using Player.Net;

namespace PlayerTests
{
    [TestClass]
    public class BehaviorChooserTests
    {
        [TestMethod]
        public void HandleMessageData_TaskField_UpdateSomeFields()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions());
            player.TaskFields = new Common.SchemaWrapper.TaskField[10, 10];
            Data data = new Data() { TaskFields = new TaskField[] { new TaskField() { playerId = 0, x = 4, y = 5, distanceToPiece = 5 }, new TaskField() { playerId = 0, x = 2, y = 3, distanceToPiece = 7 } } };
            Player.Net.BehaviorChooser.HandleMessage(data, new Player.Net.PlayerMessageHandleArgs(null, null, null, null, player));

            Assert.AreEqual((uint)5, player.TaskFields[4, 5].DistanceToPiece);
            Assert.AreEqual((uint)7, player.TaskFields[2, 3].DistanceToPiece);
        }

        [TestMethod]
        public void HandleMessageData_GoalField_UpdateSomeFields()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions());
            player.GoalFields = new Common.SchemaWrapper.GoalField[10, 10];
            Data data = new Data() { GoalFields = new GoalField[] { new GoalField() { playerId = 0, x = 4, y = 5, team = TeamColour.blue }, new GoalField() { playerId = 0, x = 2, y = 3, team = TeamColour.red } } };
            Player.Net.BehaviorChooser.HandleMessage(data, new Player.Net.PlayerMessageHandleArgs(null, null, null, null, player));

            Assert.AreEqual(TeamColour.blue, player.GoalFields[4, 5].Team);
            Assert.AreEqual(TeamColour.red, player.GoalFields[2, 3].Team);
        }

        [TestMethod]
        public void HandleMessageData_Pieces_UpdateInfoAboutPiece()
        {
            PlayerClient player = new PlayerClient(new ConnectionMock(), new Common.Config.PlayerSettings(), new AgentCommandLineOptions());
            player.TaskFields = new Common.SchemaWrapper.TaskField[10, 10];
            player.TaskFields[3, 3] = new Common.SchemaWrapper.TaskField();
            player.Location = new Location() { x = 3, y = 3 };
            Data data = new Data() { Pieces = new Piece[] { new Piece() { id = 3} } };
            Player.Net.BehaviorChooser.HandleMessage(data, new Player.Net.PlayerMessageHandleArgs(null, null, null, null, player));

            Assert.AreEqual((ulong)3, player.TaskFields[3, 3].PieceId);
        }



    }
}
