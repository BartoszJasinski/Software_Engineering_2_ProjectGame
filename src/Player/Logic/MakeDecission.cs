using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;

namespace Player.Logic
{
    //TODO It is temporary class for now we have to decide on Logic architecture in Player and GM. 
    //imho we should have basic message handling in BehaviourChooser and pass handled data to specific classes in Logic packages
    class MakeDecission
    {
        void RegisteredGames(RegisteredGames registeredGames, PlayerData playerData)
        {
            foreach (var game in registeredGames.GameInfo)
            {
                if (playerData.prefferedTeam == "blue")
                    if (game.blueTeamPlayers > 0)
                        //TODO how we should send this JoinGame message?
                        //send JoinGame
                        ;
                    else ;
                else ;//.....
            }


        }
    }
}
