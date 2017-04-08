using System;
using System.Collections.Generic;
using System.Linq;
using Common.Schema;
using Common.SchemaWrapper.Abstraction;
using Common.DebugUtils;

namespace Common.SchemaWrapper
{
    public class AddressableBoard: ISchemaCompliantBoard
    {
        private GameBoard board;
        private Random rng = new Random();

        private const uint NO_PIECE = uint.MaxValue;

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
            var possibleFields = Fields.Cast<Field>().Where(f => f is GoalField );
            var possibleGoalFields = Fields.Cast<GoalField>().Where(f => f.Y < GoalsHeight && f.Team == teamColour);
            return possibleGoalFields.ToList();
        }

        public Field GetEmptyPositionForPlayer(TeamColour team)
        {
            //no player, TaskField or our GoalField
            var possibleFields = Fields.Cast<Field>().Where(f => (f is TaskField || (f as GoalField).Team == team) && f.PlayerId == null);
            //maybe random is a bad idea (unfair?)
            return possibleFields.RandomElementUsing(rng);

        }

        public void UpdateDistanceToPiece(IList<Piece> pieces)
        {
            var notCarriedPieces = pieces.Where(p => !p.IsCarried).ToList();
            foreach (var field in Fields.Cast<Field>().Where(f => f is TaskField))
            {
                if (notCarriedPieces.Count == 0)
                {
                    (field as TaskField).DistanceToPiece = NO_PIECE;
                }
                else
                {
                    //you need to cast to long, otherwise uint can wrap around -.-
                    var distance = notCarriedPieces.Select(p => Math.Abs((long)p.Location.x - (long)field.X) + Math.Abs((long)p.Location.y - (long)field.Y)).Min();
                    (field as TaskField).DistanceToPiece = (uint)distance;
                }
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
