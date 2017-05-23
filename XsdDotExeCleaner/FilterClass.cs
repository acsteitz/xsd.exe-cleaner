using System.Collections.Generic;

namespace XsdDotExeCleaner
{
    internal abstract class FilterClass
    {
        protected abstract IEnumerable<string> Apply(IEnumerable<string> lines);

        public static implicit operator Filter(FilterClass filter)
        {
            return filter.Apply;
        }
    }
}
