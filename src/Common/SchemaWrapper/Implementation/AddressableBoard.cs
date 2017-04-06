using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Common.SchemaWrapper.Abstraction;
using Common;

namespace Common.SchemaWrapper
{
    public class AddressableBoard: ISchemaCompliantBoard
    {
        private GameBoard board;
        private Random rng = new Random();

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

        public TaskField GetRandomEmptyFieldInTaskArea()
        {
            //TODO this can overwrite existing Pieces
            var possibleFields = Fields.Cast<Field>().Where(f => f.Y >= GoalsHeight && f.Y < GoalsHeight + TasksHeight && f.PlayerId == null);
            if (possibleFields.Count() == 0)
                return null;
            return (TaskField)possibleFields.RandomElementUsing(rng);
        }

        #region constructors

        public AddressableBoard()
        {
            this.board = new GameBoard();
        }

        #endregion

    }
}
