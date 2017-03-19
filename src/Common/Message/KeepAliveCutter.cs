using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Message
{
    public class KeepAliveCutter
    {
        public static string Cut(string message)
        {
           return message.Remove(message.Length - 1);
        }
    }
}
