using System.Collections.Generic;
using Common.SchemaWrapper;

namespace Common.GameInfo
{
    public class GameState
    {
        public AddressableBoard board { get; }
        public List<Piece> pieces { get; }

        public GameState(AddressableBoard board)
        {
            this.board = board;
        }

        public void AddPiece(Piece piece)
        {
            pieces.Add(piece);
        }

    }
}
