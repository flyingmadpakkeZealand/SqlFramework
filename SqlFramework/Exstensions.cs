using System;
using System.Collections.Generic;
using System.Text;

namespace SqlFramework
{
    internal static class Exstensions
    {
        internal static string TrimParam(this string fullParameter)
        {
            int count = 0;
            char nextChar;
            do
            {
                nextChar = fullParameter[++count];
            } while (nextChar >= '0' && nextChar <= '9' || nextChar == Setup.Pchar);

            return fullParameter.Remove(0, count);
        }
    }
}
