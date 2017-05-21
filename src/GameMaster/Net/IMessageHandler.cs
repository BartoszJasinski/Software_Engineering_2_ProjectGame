using Common.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameMaster.Net
{
    public interface IMessageHandler
    {
        void HandleMessage(ConfirmGameRegistration message, Socket handler);
        void HandleMessage(JoinGame message, Socket handler);
        Task HandleMessage(Move message, Socket handler);
        Task HandleMessage(Discover message, Socket handler);
        Task HandleMessage(PickUpPiece message, Socket handler);
        void HandleMessage(TestPiece message, Socket handler);
        Task HandleMessage(PlacePiece message, Socket handler);
        Task HandleMessage(AuthorizeKnowledgeExchange message, Socket handler);
        void HandleMessage(PlayerDisconnected message, Socket handler);
        void HandleMessage(object message, Socket handler);
        void PrintBoard();
        event EventHandler<EndGameEventArgs> OnGameEnd;
        GameMasterClient GameMasterClient { get; set; }
        void Clear();
    }
}
