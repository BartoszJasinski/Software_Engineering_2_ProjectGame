using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;

namespace Common.SchemaWrapper
{
    public class GoalField : Field
    {
        private Schema.GoalField goalSchemaField;

        public GoalField()
        {
            goalSchemaField = new Schema.GoalField();
        }

        public override Schema.Field SchemaField
        {
            get
            {
                return goalSchemaField;
            }

        }

        public GoalFieldType Type
        {
            get { return goalSchemaField.type; }
            set { goalSchemaField.type = value; }
        }

        public TeamColour Team
        {
            get { return goalSchemaField.team; }
            set { goalSchemaField.team = value; }
        }
    }
}
