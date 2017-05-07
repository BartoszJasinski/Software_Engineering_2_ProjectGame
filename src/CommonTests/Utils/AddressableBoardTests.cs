using Common.SchemaWrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTests.Utils
{
    [TestClass()]
    public class AddressableBoardTests
    {
        [TestMethod]
        public void AddressableBoard_Distance_TwoPieces()
        {
            AddressableBoard board = new AddressableBoard();
            board.Fields = new Common.SchemaWrapper.TaskField[10, 10];
            for( int i = 0; i < 10; i++)
            {
                for(int j = 2; j< 9; j++)
                {
                    board.Fields[i, j] = new TaskField() { X = (uint)i, Y = (uint)j };
                }
            }
            List<Piece> pieceList = new List<Piece>();
            pieceList.Add(new Piece() { Location = new Common.Schema.Location() { x = 3, y = 4 }, IsCarried = false, PlayerId = null });
            pieceList.Add(new Piece() { Location = new Common.Schema.Location() { x = 6, y = 6 }, IsCarried = false, PlayerId = null });
            board.UpdateDistanceToPiece(pieceList);

            Assert.AreEqual((int)1, (board.Fields[3, 5] as TaskField).DistanceToPiece);
        }
    }
}
