using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;

namespace Common.SchemaWrapper.Abstraction
{
    public interface ISchemaCompliantField
    {
        Field SchemaField { get; }

        DateTime Timestamp { get; set; }
        ulong PlayerId { get; set; }
        bool PlayerIdSpecified { get; set; }
        uint X { get; set; }
        uint Y { get; set; }
    }
}
