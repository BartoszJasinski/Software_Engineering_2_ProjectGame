using System;

namespace Common.SchemaWrapper
{
    public abstract class Field
    {

        public abstract Schema.Field SchemaField { get; }


        public uint X
        {
            get { return SchemaField.x; }
            set { SchemaField.x = value; }
        }

        public uint Y
        {
            get { return SchemaField.y; }
            set { SchemaField.y = value; }
        }

        public DateTime Timestamp
        {
            get { return SchemaField.timestamp; }
            set { SchemaField.timestamp = value; }
        }

        public ulong? PlayerId
        {
            get { return SchemaField.playerIdSpecified ? SchemaField.playerId : (ulong?)null; }
            set
            {
                if (value.HasValue)
                {
                    SchemaField.playerIdSpecified = true;
                    SchemaField.playerId = value.Value;
                }
                else
                    SchemaField.playerIdSpecified = false;
            }
        }

    }
}
