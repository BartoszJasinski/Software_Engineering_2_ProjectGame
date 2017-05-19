using Common.Schema;
using System.Collections.Generic;

namespace Player.Net
{
    public interface IGame
    {
        Common.SchemaWrapper.Field[,] Fields { get; set; }
        ulong GameId { get; set; }
        ulong Id { get; set; }
        Common.Schema.Player[] Players { set; get; }
        GameBoard Board { get; set; }
        void PrintBoard();
        void UpdateFields(Common.SchemaWrapper.Field[] newTaskFields);
        string Guid { get; set; }
        Common.Schema.TeamColour Team { get; set; }
        Common.Schema.Location Location { get; set; }
        PlayerType Type { get; set; }
        IList<Piece> Pieces { get; set; }

    }
}
