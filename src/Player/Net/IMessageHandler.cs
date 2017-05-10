using Common.Schema;
using Player.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player.Net
{
    public interface IMessageHandler
    {
        PlayerClient Player { get; set; }
        void HandleMessage(RegisteredGames message);
        void HandleMessage(ConfirmJoiningGame message);
        void HandleMessage(RejectJoiningGame message);
        void HandleMessage(Common.Schema.Game message);
        void HandleMessage(Data message);
        void HandleMessage(KnowledgeExchangeRequest message);
        void HandleMessage(AcceptExchangeRequest message);
        void HandleMessage(RejectKnowledgeExchange message);
        void HandleMessage(object message);
        void PrintBoard();
        State BiuldDfa();
    }
}
