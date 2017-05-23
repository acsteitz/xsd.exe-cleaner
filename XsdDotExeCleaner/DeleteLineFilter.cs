using System.Collections.Generic;
using System.Linq;

namespace XsdDotExeCleaner
{
    /// <summary>
    /// Deletes lines from the output if they contain the specified text.
    /// </summary>
    internal class DeleteLineFilter : FilterClass
    {
        private readonly string _content;

        public DeleteLineFilter(string content)
        {
            _content = content;
        }

        protected override IEnumerable<string> Apply(IEnumerable<string> lines)
        {
            return lines.Where(line => !line.Contains(_content));
        }
    }
}
