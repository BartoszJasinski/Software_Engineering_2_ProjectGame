using Common.Schema;
using Player.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player.Net
{
    public interface IGame
    {
        Common.SchemaWrapper.Field[,] Fields { get; set; }
        ulong GameId { get; set; }
        ulong Id { get; set; }
        Common.Schema.Player[] Players {  set; get; }
        Piece CarriedPiece{  get; }
        GameBoard Board { get; set; }
        string Guid { get;set; }
        Common.Schema.TeamColour Team { get; set; }
        Common.Schema.Location Location { get; set; }
        PlayerType Type { get; set; }
        IList<Piece> Pieces { get; set; }

        void HandleMessage(RegisteredGames message);
        void HandleMessage(ConfirmJoiningGame message);
        void HandleMessage(RejectJoiningGame message);
        void HandleMessage(Common.Schema.Game message);
        void HandleMessage(Data message);
        void HandleMessage(KnowledgeExchangeRequest message);
        void HandleMessage(AcceptExchangeRequest message);
        void HandleMessage(RejectKnowledgeExchange message);
        void HandleMessage(object message);
        State BiuldDfa();
    }
}
