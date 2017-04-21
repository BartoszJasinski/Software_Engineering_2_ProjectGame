using System.Collections.Generic;
using System.Linq;
using Common.Schema;

namespace Common.Message
{
    public class DataMessageBuilder
    {
        private ulong playerId;
        private bool finished;
        private TaskField[] taskFields;
        private Location playerLocation;
        private Piece[] pieces;
        private GoalField[] goalFields;

        public DataMessageBuilder(ulong playreId, bool gameFinished = false)
        {
            this.playerId = playreId;
            finished = gameFinished;
        }

        public DataMessageBuilder SetWrapperTaskFields(IEnumerable<SchemaWrapper.TaskField> taskFields)
        {
            var tF = new List<TaskField>();
            foreach (var schemaWrapperTaskField in taskFields)
                tF.Add((TaskField)schemaWrapperTaskField.SchemaField);
            this.taskFields = tF.ToArray();

            return this;
        }

        public DataMessageBuilder SetWrapperGoalFields(IEnumerable<SchemaWrapper.GoalField> goalFields)
        {
            var gF = new List<GoalField>();
            foreach (var schemaWrapperGoalField in goalFields)
                gF.Add((GoalField)schemaWrapperGoalField.SchemaField);
            this.goalFields = gF.ToArray();

            return this;
        }

        public DataMessageBuilder SetWrapperPieces(IEnumerable<SchemaWrapper.Piece> pieces)
        {
            var p = new List<Piece>();
            foreach (var piece in pieces)
                p.Add(piece.SchemaPiece);
            this.pieces = p.ToArray();

            return this;
        }


        public DataMessageBuilder SetTaskFields(IEnumerable<TaskField> taskFields)
        {
            this.taskFields = taskFields.ToArray();
            return this;
        }

        public DataMessageBuilder AddTaskField(TaskField taskField)
        {
            if (taskFields == null)
                taskFields = new TaskField[] {taskField};
            else
            {
                var l = new List<TaskField>(taskFields);
                l.Add(taskField);
                taskFields = l.ToArray();
            }
            return this;
        }

        public DataMessageBuilder SetPlayerLocation(Location loc)
        {
            this.playerLocation = loc;
            return this;
        }

        public DataMessageBuilder SetPieces(IEnumerable<Piece> pieces)
        {
            this.pieces = pieces.ToArray();
            return this;
        }

        public DataMessageBuilder AddPiece(Piece piece)
        {
            if (pieces == null)
                pieces = new []{ piece };
            else
            {
                var l = new List<Piece>(pieces);
                l.Add(piece);
                pieces = l.ToArray();
            }
            return this;
        }

        public DataMessageBuilder SetGoalFields(IEnumerable<GoalField> goalFields)
        {
            this.goalFields = goalFields.ToArray();
            return this;
        }

        public DataMessageBuilder AddGoalField(GoalField goalField)
        {
            if (goalFields == null)
                goalFields = new[] { goalField };
            else
            {
                var l = new List<GoalField>(goalFields);
                l.Add(goalField);
                goalFields = l.ToArray();
            }
            return this;
        }

        public Data Build()
        {
            Data d = new Data();
            d.gameFinished = finished;
            d.playerId = playerId;
            if (taskFields != null)
                d.TaskFields = taskFields;
            if (this.goalFields != null)
                d.GoalFields = goalFields;
            if (pieces != null)
                d.Pieces = pieces;
            if (playerLocation != null)
                d.PlayerLocation = playerLocation;
            return d;
        }

        public string GetXml()
        {
            return XmlMessageConverter.ToXml(Build());
        }

    }
}
