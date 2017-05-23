using System.Collections.Generic;
using System.Linq;

namespace XsdDotExeCleaner
{
    /// <summary>
    /// Performs a simple find and replace.
    /// </summary>
    internal class ReplaceFilter : FilterClass
    {
        private readonly string _find;
        private readonly string _replacement;

        public ReplaceFilter(string find, string replacement)
        {
            _find = find;
            _replacement = replacement;
        }

        protected override IEnumerable<string> Apply(IEnumerable<string> lines)
        {
            return lines.Select(line => line.Replace(_find, _replacement));
        }
    }
}
