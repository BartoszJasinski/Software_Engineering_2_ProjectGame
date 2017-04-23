using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMaster.Log
{
    class UniqueNameGenerator
    {
        public static string GetUniqueName()
        {
            return "gamemaster_" + DateTime.Now.ToString("yyyy-MM-ddTHH_mm_ss") + ".csv";
        }
    }
}
