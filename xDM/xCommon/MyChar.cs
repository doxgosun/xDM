using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon
{
    public class MyChar
    {
        public static string GetString(char c)
        {
            switch (c)
            {
                case '\0': return null;
                case '\a': return @"\a";
                case '\b': return @"\b";
                case '\f': return @"\f";
                case '\r': return @"\r";
                case '\n': return @"\n";
                case '\t': return @"\t";
                case '\v': return @"\v";
                default:
                    return c.ToString();
            }
        }

        public static char GetChar(string str)
        {
            if (str == null || str == "") return '\0';
            switch (str)
            {
                case @"\a": return '\a';
                case @"\b": return '\b';
                case @"\f": return '\f';
                case @"\r": return '\r';
                case @"\n": return '\n';
                case @"\t": return '\t';
                case @"\v": return '\v';
                default:
                    if (str.Length > 1)
                    {
                        return str[1];
                    }
                    else
                    {
                        return str[0];
                    }
            }
        }
    }
}
