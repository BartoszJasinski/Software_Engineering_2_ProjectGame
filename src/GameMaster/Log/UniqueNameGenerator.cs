using System;

namespace GameMaster.Log
{
    class UniqueNameGenerator
    {
        public static string GetUniqueName(string gameName)
        {
            gameName = gameName.Replace(' ', '_');
            return gameName + "_" + DateTime.Now.ToString("yyyy-MM-ddTHH_mm_ss") + ".csv";
        }
    }
}
