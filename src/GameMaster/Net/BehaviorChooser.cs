﻿using System;
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
using System.Threading;
using GameMaster.Logic.Board;

namespace GameMaster.Net
{
    static class BehaviorChooser /*: IMessageHandler<ConfirmGameRegistration>*/
    {
        public static void HandleMessage(ConfirmGameRegistration message, GameMasterClient gameMaster, Socket handler)
        {
            ConsoleDebug.Good("I get gameId = " + message.gameId);
            gameMaster.Id = message.gameId;
        }

        public static void HandleMessage(JoinGame message, GameMasterClient gameMaster, Socket handler)
        {

            var selectedTeam = gameMaster.selectTeamForPlayer(message.preferredTeam);
            //both teams are full
            if (selectedTeam == null)
            {
                gameMaster.Connection.SendFromClient(handler,
                    XmlMessageConverter.ToXml(new RejectJoiningGame()
                    {
                        gameName = message.gameName,
                        playerId = message.playerId
                    }));
                return;
            }

            var role = message.preferredRole;
            if (role == PlayerType.leader)
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

            if (gameMaster.IsReady)
            {
                //TODO Load Board from config file
                var boardGenerator = new RandomGoalBoardGenerator(5, 5, 3, 123);
                var board = boardGenerator.CreateBoard().SchemaBoard;
                gameMaster.Board = board;
                var players = gameMaster.Players.Select(p => p.SchemaPlayer).ToArray();
                foreach (var player in gameMaster.Players)
                {
                    var startGame = new Game()
                    {
                        Board = board,
                        playerId = player.Id,
                        PlayerLocation = new Location() {x = 0, y = 0},
                        Players = players
                    };
                    var gameString = XmlMessageConverter.ToXml(startGame);
                    //ConsoleDebug.Message(gameString);
                    gameMaster.Connection.SendFromClient(handler, gameString);
                }
            }
        }

        public static void HandleMessage(Move message, GameMasterClient gameMaster, Socket handler)
        {
            int dx, dy;
            Data resp = new Data();
            ;

            Task.Delay(new TimeSpan(0,0,0,0, (int) gameMaster.Settings.ActionCosts.MoveDelay)).ContinueWith(_ =>
            {
                Logic.Player player = gameMaster.Players.First(p => message.playerGuid == p.Guid);
                resp.playerId = player.Id;
                dx = message.direction == MoveType.right ? 1 : (message.direction == MoveType.left ? -1 : 0);
                dy = message.direction == MoveType.up ? 1 : (message.direction == MoveType.down ? -1 : 0);
                //if moving behind borders, dont move
                if (!message.directionSpecified ||
                    (player.Location.x + dx < 0 || player.Location.x + dx >= gameMaster.Board.width) ||
                    (player.Location.y + dy < 0 ||
                     player.Location.y + dy >= gameMaster.Board.tasksHeight * 2 + gameMaster.Board.tasksHeight))
                {
                    resp.PlayerLocation = player.Location;
                    gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(resp));
                    return;
                }
                resp.PlayerLocation = new Location(){x=(uint) (player.Location.x+dx),y=(uint) (player.Location.y+dy)};
                gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(resp));
            });
        }


        public static void HandleMessage(object message, GameMasterClient gameMaster, Socket handler)
        {
            ConsoleDebug.Warning("Unknown Type");
        }
    }
}