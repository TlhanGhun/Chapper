using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateCheck.Converter
{
    public class prettyVersion
    {
        public static string getNiceVersionString(string fullString)
        {
            while (fullString.Length > 3 && fullString.EndsWith(".0"))
            {
                fullString = fullString.Substring(0, fullString.Length - 2);
            }
            return fullString;
        }
    }
}