using System.Collections.Generic;
using Common.SchemaWrapper;
using TeamColour = Common.Schema.TeamColour;

namespace GameMaster.Logic
{
    //PlaceHolder Class we should refactor whole Logic i think, GMClient primarly
    class MakeDecision
    {
        public static bool endGame { get; set; } = false;

        public static void EndGame(AddressableBoard board, TeamColour teamColour)
        {
            IList<Common.SchemaWrapper.GoalField> goalFields = board.GetNotOccupiedGoalFields(teamColour);
            if (goalFields.Count == 0)
                endGame = true;
        }

        //We will use this version after refactorization probably
//        public static void EndGame(GameState gameState, TeamColour teamColour)
//        {
//            IList<Common.SchemaWrapper.GoalField> goalFields = gameState.board.GetNotOccupiedGoalFields(teamColour);
//            if (goalFields.Count == 0)
//                endGame = true;
//        }


    }
}
