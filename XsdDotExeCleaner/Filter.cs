using System.Collections.Generic;

namespace XsdDotExeCleaner
{
    delegate IEnumerable<string> Filter(IEnumerable<string> lines);
}