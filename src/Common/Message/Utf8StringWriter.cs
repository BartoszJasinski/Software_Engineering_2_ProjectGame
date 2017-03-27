using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Message
{
    public class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder sb) : base(sb)
        {
                
        }

        public override Encoding Encoding
        {
            get { return new UTF8Encoding(false); }
        }
    }

    public class Utf8StringReader : StringReader
    {
        public Utf8StringReader(string s) : base(s)
        {

        }

    }
}
