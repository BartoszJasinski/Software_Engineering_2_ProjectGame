using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Common.SchemaWrapper;
using Common.SchemaWrapper.Abstraction;

namespace GameMaster.Logic.Board
{
    public class SimpleBoardGenerator : IBoardGenerator<AddressableBoard>
    {
        private uint boardWidth;
        private uint taskAreaHeight;
        private bool[,] goalLayout;

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
                        x = (uint)x,
                        y = (uint)y,
                        playerIdSpecified = false,
                        timestamp = timestamp,
                        team = TeamColour.red,
                        type = goalLayout[x,y] ? GoalFieldType.goal : GoalFieldType.nongoal
                    };
                    var xLocal = board.Width - x - 1;
                    var yLocal = board.GoalsHeight - y - 1 + goalsOffset;
                    board.Fields[xLocal, yLocal] = new GoalField()
                    {
                        x = (uint)xLocal,
                        y = (uint)yLocal,
                        playerIdSpecified = false,
                        timestamp = timestamp,
                        team = TeamColour.blue,
                        type = goalLayout[x, y] ? GoalFieldType.goal : GoalFieldType.nongoal
                    };
                }
            }

            for (uint y = board.GoalsHeight; y < board.GoalsHeight + board.TasksHeight; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board.Fields[x,y] = new TaskField()
                    {
                        pieceIdSpecified = false,
                        playerIdSpecified = false,
                        x = (uint)x,
                        y = y,
                        timestamp = timestamp
                    };
                }
            }

            return board;
        }
    }
}
