using System;
using System.Collections.Generic;
using System.Linq;
using Common.Schema;
using Common.SchemaWrapper.Abstraction;

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

        public uint Height => TasksHeight + 2 * GoalsHeight;

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

        public IList<GoalField> GetNotOccupiedGoalFields(TeamColour teamColour)
        {
           // IList<GoalField> goals = new List<GoalField>();
            var possibleFields = Fields.Cast<GoalField>().Where(f => f.Y < GoalsHeight && f.Team == teamColour);
            return possibleFields.ToList();
        }

        public void UpdateDistanceToPiece(IList<Piece> pieces)
        {
            foreach(var field in Fields.Cast<Field>().Where(f => f is TaskField))
            {
                var distance = pieces.Select(p => Math.Abs(p.Location.x - field.X) + Math.Abs(p.Location.y - field.Y)).Min();
                (field as TaskField).DistanceToPiece = (uint)distance;
            }
        }


        #region constructors

        public AddressableBoard()
        {
            this.board = new GameBoard();
        }

        #endregion

    }
}
