using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.SchemaWrapper;

namespace GameMaster.Logic.Board
{
    public class RandomGoalBoardGenerator : IBoardGenerator<AddressableBoard>
    {
        private uint boardWidth;
        private uint tasksHeight;
        private uint goalHeight;
        private Random rand;

        public RandomGoalBoardGenerator(uint boardWidth, uint tasksHeight, uint goalHeight, int seed)
        {
            rand = new Random(seed);
            this.boardWidth = boardWidth;
            this.tasksHeight = tasksHeight;
            this.goalHeight = goalHeight;
        }

        public int Seed { set { rand = new Random(value); } }

        public AddressableBoard CreateBoard()
        {
            bool[,] goalLayout = new bool[boardWidth, goalHeight];
            for (int y = 0; y < goalHeight; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    goalLayout[x, y] = rand.NextDouble() > 0.5;
                }
            }

            return new SimpleBoardGenerator(boardWidth, tasksHeight, goalLayout).CreateBoard();
        }
    }
}
