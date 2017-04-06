using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;

namespace Common.SchemaWrapper.Implementation
{
    class Piece
    {
        private Schema.Piece schemaPiece;

        public Piece(Schema.Piece schemaPiece)
        {
            this.SchemaPiece = schemaPiece;
        }

        public Schema.Piece SchemaPiece
        {
            get { return schemaPiece; }
            private set { schemaPiece = value; }
        }

        public ulong Id
        {
            get { return schemaPiece.id; }
            set { schemaPiece.id = value; }
        }

        public PieceType Type
        {
            get { return schemaPiece.type;  }
            set { schemaPiece.type = value; }
        }

        public DateTime TimeStamp
        {
            get { return schemaPiece.timestamp; }
            set { schemaPiece.timestamp = value; }
        }

        public ulong? PlayerId
        {
            get { return schemaPiece.playerIdSpecified ? schemaPiece.playerId : (ulong?)null; }
            set
            {
                if (value.HasValue)
                {
                    schemaPiece.playerIdSpecified = true;
                    schemaPiece.playerId = value.Value;
                }
                else
                    schemaPiece.playerIdSpecified = false;
            }
        }

        public Location Location { get; set; }
    }
}
