using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Piece = Common.SchemaWrapper.Piece;

namespace Player.Net
{
    public interface IPlayer
    {
        void Move(MoveType direction);
        void Discover();
        void DestroySham();
        bool? TestPiece();
        Piece Piece { get; }
    }
}
