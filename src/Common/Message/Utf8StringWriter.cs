using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Message
{
    class Utf8StringWriter: StringWriter
    {
        public Utf8StringWriter(StringBuilder stringBuilder): base(stringBuilder)
        {
        }
        
        
        // Use UTF8 encoding but write no BOM to the wire
        public override Encoding Encoding
        {
            get { return new UTF8Encoding(false); } // in real code I'll cache this encoding.
        }



    }
}
