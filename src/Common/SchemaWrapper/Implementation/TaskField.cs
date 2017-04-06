using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;

namespace Common.SchemaWrapper.Implementation
{
    public class TaskField : Field
    {
        private Schema.TaskField schemaTaskField;

        public override Schema.Field SchemaField
        {
            get
            {
                return schemaTaskField;
            }
        }

        public uint DistanceToPiece
        {
            get { return schemaTaskField.distanceToPiece; }
            set { schemaTaskField.distanceToPiece = value; }
        }


        public ulong? PieceId
        {
            get { return schemaTaskField.pieceIdSpecified ? schemaTaskField.pieceId : (ulong?)null; }
            set
            {
                if (value.HasValue)
                {
                    schemaTaskField.pieceIdSpecified = true;
                    schemaTaskField.pieceId = value.Value;
                }
                else
                    schemaTaskField.pieceIdSpecified = false;
            }
        }

        public TaskField()
        {
            schemaTaskField = new Schema.TaskField();
        }
    }
}
