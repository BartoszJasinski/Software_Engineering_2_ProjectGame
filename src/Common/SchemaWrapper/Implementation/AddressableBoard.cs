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

        private const int NO_PIECE = -1;

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

        public IEnumerable<TaskField> GetTaskFields()
        {

            var tF = Fields.Cast<Field>().Where(f => f is TaskField);
            var taskFields = tF.Cast<TaskField>();
            return taskFields.ToList();
        }

        public IEnumerable<GoalField> GetGoalFields()
        {

            var gF = Fields.Cast<Field>().Where(f => f is GoalField);
            var goalFields = gF.Cast<GoalField>();
            return goalFields.ToList();
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
            var possibleGoalFields = possibleFields.Cast<GoalField>().Where(f => f.Team == teamColour && f.Type == GoalFieldType.goal);
            return possibleGoalFields.ToList();
        }

        public Field GetEmptyPositionForPlayer(TeamColour team)
        {
            //no player, TaskField or our GoalField
            var possibleFields = Fields.Cast<Field>().Where(f => (f is TaskField || ( f is GoalField && !IsInEnemyGoalArea(f.Y, team))) && f.PlayerId == null);
            //maybe random is a bad idea (unfair?)
            return possibleFields.RandomElementUsing(rng);

        }


        public bool IsInEnemyGoalArea(long y, TeamColour myTeam)
        {
            if (myTeam == TeamColour.blue) //we are blue, enemy is red and on top
                return y >= Height - GoalsHeight;
            //we are red, enemy is blue and on the bottom
            return y < GoalsHeight;
        }

        public void UpdateDistanceToPiece(IList<Piece> pieces)
        {
            foreach (var field in Fields.Cast<Field>().Where(f => f is TaskField))
            {
                if (pieces.Where(p => p.PlayerId == null).Count() == 0)
                {
                    (field as TaskField).DistanceToPiece = NO_PIECE;
                }
                else
                {
                    //you need to cast to long, otherwise uint can wrap around -.-
                    var possiblePieces = pieces.Where(p => p.PlayerId == null);
                    var distances = possiblePieces.Select(p => Math.Abs((long)p.Location.x - (long)field.X) + Math.Abs((long)p.Location.y - (long)field.Y));
                    var distance = distances.Min();
                    (field as TaskField).DistanceToPiece = (int)distance;
                }
            }
        }


        #region constructors

        public AddressableBoard()
        {
            this.board = new GameBoard();
        }

        public AddressableBoard(GameBoard board)
        {
            this.board = board;
        }

        #endregion

    }
}
