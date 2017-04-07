using System.Collections.Generic;
using Common.GameInfo;
using Common.Schema;
using TeamColour = Common.Schema.TeamColour;

namespace GameMaster.Logic
{
    //PlaceHolder Class we should refactor whole Logic i think, GMClient primarly
    class MakeDecision
    {
        private bool endGame = false;
        public void EndGame(GameState gameState, TeamColour teamColour)
        {
            IList<Common.SchemaWrapper.GoalField> goalFields = gameState.board.GetNotOccupiedGoalFields(teamColour);
            if (goalFields.Count == 0)
                endGame = true;
        }

        public void FillGameFinished(GameGood message)
        {
            //TODO we should make something simmilar to BehoviourChooser if we want to use this function to fill game finished field in 
            
        }
    }
}
