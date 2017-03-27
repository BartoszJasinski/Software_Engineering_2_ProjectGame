using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
