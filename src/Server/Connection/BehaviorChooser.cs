using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using System.Threading;

namespace Server.Connection
{
    public static class BehaviorChooser
    {
        private static object joinLock = new object();

        public static void HandleMessage(RegisterGame request, CommunicationServer server, Socket handler)
        {

            if (request == null)
                return;

            Game.Game g = new Game.Game(gameId: server.RegisteredGames.NextGameId(), name: request.NewGameInfo.gameName,
                bluePlayers: request.NewGameInfo.blueTeamPlayers,
                redPlayers: request.NewGameInfo.redTeamPlayers, gameMaster: handler
                );
            try
            {
                server.RegisteredGames.RegisterGame(g);
            }
            catch
            {
                return;
            }

            ConfirmGameRegistration gameRegistration = new ConfirmGameRegistration() { gameId = (ulong)g.Id };

            var response = XmlMessageConverter.ToXml(gameRegistration);
            server.ConnectionEndpoint.SendFromServer(handler, response);
        }

        public static void HandleMessage(GetGames request, CommunicationServer server, Socket handler)
        {
            List<GameInfo> gi = new List<GameInfo>();
            foreach (var game in server.RegisteredGames)
            {
                gi.Add(new GameInfo()
                {
                    blueTeamPlayers = game.BlueTeamPlayersCount,
                    redTeamPlayers = game.RedTeamPlayersCount,
                    gameName = game.Name
                });
            }
            var rg = new RegisteredGames() {GameInfo = gi.ToArray()};
            server.ConnectionEndpoint.SendFromServer(handler, XmlMessageConverter.ToXml(rg));
        }

        public static void HandleMessage(JoinGame request, CommunicationServer server, Socket handler)
        {
            if (request == null)
                return;

            Game.IGame g = server.RegisteredGames.GetGameByName(request.gameName);
            if (g == null)
            {
                ConsoleDebug.Error("Game with specified name not found");
                return;
            }

            lock(joinLock)
            {
                request.playerId = server.IdForNewClient();
                request.playerIdSpecified = true;
                server.Clients.Add(request.playerId, handler);
            }

            var response = XmlMessageConverter.ToXml(request);

                server.ConnectionEndpoint.SendFromServer(g.GameMaster, response);
                return;
        }

        public static void HandleMessage(PlayerGood request, CommunicationServer server, Socket handler)
        {
            if (request == null)
                return;

            var response = XmlMessageConverter.ToXml(request);
            server.ConnectionEndpoint.SendFromServer(server.Clients[request.playerId], response);
        }

        public static void HandleMessage(GameGood request, CommunicationServer server, Socket handler)
        {
            if (request == null)
                return;

            var response = XmlMessageConverter.ToXml(request);
            server.ConnectionEndpoint.SendFromServer(server.RegisteredGames.GetGameById((int)request.gameId).GameMaster, response);
        }

        public static void HandleMessage(object message, CommunicationServer server, Socket handler)
        {
            ConsoleDebug.Warning("Unknown type");

        }
    }
}
