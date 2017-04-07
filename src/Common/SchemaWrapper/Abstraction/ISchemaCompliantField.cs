using System;

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
