using System;
using Common.Schema;
using System.Collections.Generic;

namespace Common.SchemaWrapper
{
    public class GoalField: Field
    {
        private Schema.GoalField goalSchemaField;

        public GoalField()
        {
            goalSchemaField = new Schema.GoalField();
        }
        public GoalField(Schema.GoalField goalSchemaField)
        {
            this.goalSchemaField = goalSchemaField;
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

        public override void AddFieldData(List<Schema.TaskField> taskFields, List<Schema.GoalField> goalFields)
        {
            goalFields.Add(goalSchemaField);
        }
    }
}
