using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Common.SchemaWrapper.Abstraction;

namespace Common.SchemaWrapper
{
    public class AddressableBoard : ISchemaCompliantBoard
    {
        private GameBoard board;

        public Field[,] Fields { get; set; }

        public uint Width
        {
            get { return board.width; }
            set { board.width = value; }
        }

        public uint TasksHeight
        {
            get { return board.tasksHeight; }
            set { board.tasksHeight = value; }
        }

        public uint GoalsHeight
        {
            get { return board.goalsHeight; }
            set { board.goalsHeight = value; }
        }

        public GameBoard SchemaBoard
        {
            get { return board; }
        }

        #region constructors

        public AddressableBoard()
        {
            this.board = new GameBoard();
        }

        #endregion

    }
}
