using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMaster
{
    public static class Utils
    {
        private static Random rng = new Random();

        public static string GenerateGuid()
        {
            string alphabet = "abcdefABCDEF0123456789";
            StringBuilder sb = new StringBuilder();
            generatePart(sb, 8, alphabet);
            sb.Append("-");
            generatePart(sb, 4, alphabet);
            sb.Append("-");
            generatePart(sb, 4, alphabet);
            sb.Append("-");
            generatePart(sb, 4, alphabet);
            sb.Append("-");
            generatePart(sb, 12, alphabet);
            return sb.ToString();
        }

        private static void generatePart(StringBuilder sb, int length, string alphabet)
        {
            for (int i = 0; i < length; i++)
            {
                sb.Append(alphabet[rng.Next(alphabet.Length)]);
            }
        }
    }
}
