using System;

namespace XsdDotExeCleaner
{
    static class StringExtensions
    {
        public static string UppercaseFirst(this string text)
        {
            return Char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}
