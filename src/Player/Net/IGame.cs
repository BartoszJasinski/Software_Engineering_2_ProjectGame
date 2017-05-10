using Common.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player.Net
{
    public interface IGame
    {
        void HandleMessage(RegisteredGames message);
        void HandleMessage(ConfirmJoiningGame message);
        void HandleMessage(RejectJoiningGame message);
        void HandleMessage(Game message);
        void HandleMessage(Data message);
        void HandleMessage(KnowledgeExchangeRequest message);
        void HandleMessage(AcceptExchangeRequest message);
        void HandleMessage(RejectKnowledgeExchange message);
        void HandleMessage(object message);
    }
}
