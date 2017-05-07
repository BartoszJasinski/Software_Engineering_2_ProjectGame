using System;
using Common.SchemaWrapper;

namespace GameMaster.Logic.Board
{
    public class SimpleBoardGenerator : IBoardGenerator<AddressableBoard>
    {
        private uint boardWidth;
        private uint taskAreaHeight;
        private bool[,] goalLayout;

        public SimpleBoardGenerator(uint boardWidth, uint taskAreaHeight, uint goalAreaHeight, Common.Config.GoalField[] goalFields)
        {
            bool[,] layout = new bool[boardWidth, goalAreaHeight];
            foreach(var field in goalFields)
            {
                layout[field.x, field.y] = true;
            }
            this.boardWidth = boardWidth;
            this.taskAreaHeight = taskAreaHeight;
            this.goalLayout = layout;
        }

        public SimpleBoardGenerator(uint boardWidth, uint taskAreaHeight, bool[,] goalLayout)
        {
            this.boardWidth = boardWidth;
            this.taskAreaHeight = taskAreaHeight;
            this.goalLayout = goalLayout;
        }

        public AddressableBoard CreateBoard()
        {
            AddressableBoard board = new AddressableBoard();
            board.Width = boardWidth;
            board.TasksHeight = taskAreaHeight;
            board.GoalsHeight = (uint)goalLayout.GetLength(1);

            board.Fields = new Field[board.Width, board.TasksHeight + 2 * board.GoalsHeight];
            uint goalsOffset = board.GoalsHeight + board.TasksHeight;
            uint boardHeight = board.TasksHeight + 2 * board.GoalsHeight;

            DateTime timestamp = DateTime.Now;

            for (int y = 0; y < board.GoalsHeight; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board.Fields[x, y] = new GoalField()
                    {
                        X = (uint)x,
                        Y = (uint)y,
                        PlayerId = null,
                        Timestamp = timestamp,
                        Team = Common.Schema.TeamColour.blue,
                        Type = goalLayout[x,y] ? Common.Schema.GoalFieldType.goal : Common.Schema.GoalFieldType.nongoal
                    };
                    var xLocal = board.Width - x - 1;
                    var yLocal = board.GoalsHeight - y - 1 + goalsOffset;
                    board.Fields[xLocal, yLocal] = new GoalField()
                    {
                        X = (uint)xLocal,
                        Y = (uint)yLocal,
                        PlayerId = null,
                        Timestamp = timestamp,
                        Team = Common.Schema.TeamColour.red,
                        Type = goalLayout[x, y] ? Common.Schema.GoalFieldType.goal : Common.Schema.GoalFieldType.nongoal
                    };
                }
            }

            for (uint y = board.GoalsHeight; y < board.GoalsHeight + board.TasksHeight; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board.Fields[x,y] = new TaskField()
                    {
                        PieceId = null,
                        PlayerId = null,
                        X = (uint)x,
                        Y = y,
                        Timestamp = timestamp
                    };
                }
            }

            return board;
        }
    }
}
