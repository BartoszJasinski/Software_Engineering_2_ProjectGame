using System;
using System.Text;

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

        public static string DrawInConsoleBox(this string s)
        {
            string ulCorner = "╔";
            string llCorner = "╚";
            string urCorner = "╗";
            string lrCorner = "╝";
            string vertical = "║";
            string horizontal = "═";

            string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);


            int longest = 0;
            foreach (string line in lines)
            {
                if (line.Length > longest)
                    longest = line.Length;
            }
            int width = longest + 2; // 1 space on each side


            string h = string.Empty;
            for (int i = 0; i < width; i++)
                h += horizontal;

            // box top
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ulCorner + h + urCorner);

            // box contents
            foreach (string line in lines)
            {
                double dblSpaces = (((double)width - (double)line.Length) / (double)2);
                int iSpaces = Convert.ToInt32(dblSpaces);

                if (dblSpaces > iSpaces) // not an even amount of chars
                {
                    iSpaces += 1; // round up to next whole number
                }

                string beginSpacing = "";
                string endSpacing = "";
                for (int i = 0; i < iSpaces; i++)
                {
                    endSpacing += " ";

                    if (!(iSpaces > dblSpaces && i == iSpaces - 1)) // if there is an extra space somewhere, it should be at the end
                    {
                        beginSpacing += " ";
                    }
                }
                // add the text line to the box
                sb.AppendLine(vertical + beginSpacing + line + endSpacing + vertical);
            }

            // box bottom
            sb.AppendLine(llCorner + h + lrCorner);

            // the finished box
            return sb.ToString();
        }

    }
}
