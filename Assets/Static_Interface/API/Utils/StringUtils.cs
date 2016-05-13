using System.Collections.Generic;

namespace Static_Interface.API.Utils
{
    public  static class StringUtils
    {
        public static string[] ToArguments(string str)
        {
            List<string> arguments = new List<string>();
            str = str.Trim();

            string singleArgument = "";
            bool bInQuotes = false;
            foreach (char c in str)
            {
                if (c == '\'')
                {
                    bInQuotes = !bInQuotes;
                }

                if (c == ' ')
                {
                    if (bInQuotes)
                    {
                        singleArgument += c.ToString();
                    }
                    else
                    {
                        arguments.Add(singleArgument);
                        singleArgument = "";
                    }
                }
                else
                {
                    singleArgument += c.ToString();
                }
            }

            if (singleArgument.Length > 0)
            {
                arguments.Add(singleArgument);
            }

            return arguments.ToArray();
        }
    }
}