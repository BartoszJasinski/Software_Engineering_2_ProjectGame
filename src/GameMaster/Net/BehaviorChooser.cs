using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using Logic = GameMaster.Logic;
using System.Net.Sockets;

namespace GameMaster.Net
{
    static class BehaviorChooser/*: IMessageHandler<ConfirmGameRegistration>*/
    {
        

        public static void HandleMessage(ConfirmGameRegistration message, GameMasterClient gameMaster, Socket handler)
        {
            ConsoleDebug.Good("I get gameId = " + message.gameId);
            gameMaster.Id = message.gameId;
        }

        public static void HandleMessage(JoinGame message, GameMasterClient gameMaster, Socket handler)
        {
            var selectedTeam = message.preferredTeam == TeamColour.blue ? gameMaster.TeamBlue : gameMaster.TeamRed;
            var otherTeam = message.preferredTeam == TeamColour.blue ? gameMaster.TeamRed : gameMaster.TeamBlue;

            if (selectedTeam.IsFull)
                selectedTeam = otherTeam;

            var role = message.preferredRole;
            if(role == PlayerType.leader)
            {
                if (selectedTeam.HasLeader)
                    role = PlayerType.member;
            }

            var guid = Utils.GenerateGuid();

            if (role == PlayerType.leader)
                selectedTeam.AddLeader(new Logic.Leader(message.playerId, guid, selectedTeam));
            else
                selectedTeam.Players.Add(new Logic.Player(message.playerId, guid, selectedTeam));

            var answer = new ConfirmJoiningGame();
            answer.playerId = message.playerId;
            answer.privateGuid = guid;
            answer.gameId = gameMaster.Id;
            answer.PlayerDefinition = new Player()
            {
                id = message.playerId,
                team = selectedTeam.Color,
                type = role
            };

            var answerString = XmlMessageConverter.ToXml(answer);
            gameMaster.Connection.SendFromClient(handler, answerString);
        }



        public static void HandleMessage(object message, GameMasterClient gameMaster, Socket handler)
        {
            ConsoleDebug.Warning("Unknown Type");
        }

        
    }
}
