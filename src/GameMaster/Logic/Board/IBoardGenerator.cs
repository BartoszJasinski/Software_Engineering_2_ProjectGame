using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Common.SchemaWrapper;
using Common.SchemaWrapper.Abstraction;

namespace GameMaster.Logic.Board
{
    public interface IBoardGenerator<out T> where T: ISchemaCompliantBoard
    {
        T CreateBoard();
    }
}
