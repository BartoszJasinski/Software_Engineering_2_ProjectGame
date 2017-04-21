using Common.Schema;

namespace Common.SchemaWrapper.Abstraction
{
    public interface ISchemaCompliantBoard
    {
        GameBoard SchemaBoard { get; }

        uint Width { get; set; }
        uint TasksHeight { get; set; }
        uint GoalsHeight { get; set; }
    }
}
