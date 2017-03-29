using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Message;
using Common.Schema;

namespace Server.Connection
{
    public static class BehaviorChooser
    {
        public static void HandleMessage(RegisterGame request, CommunicationServer server, Socket handler)
        {

            if (request == null)
                return;

            Game.Game g = new Game.Game(gameId: server.RegisteredGames.NextGameId(), name: request.NewGameInfo.gameName,
                bluePlayers: request.NewGameInfo.blueTeamPlayers,
                redPlayers: request.NewGameInfo.redTeamPlayers
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

        public static void HandleMessage(object message, CommunicationServer server, Socket handler)
        {
            Console.WriteLine("Unknown type");

        }
    }
}
