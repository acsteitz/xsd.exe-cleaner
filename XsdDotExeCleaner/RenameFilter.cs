using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XsdDotExeCleaner
{
    internal static class RenameFilter
    {
        private const string Indentation = "    ";

        private static readonly Regex OmniAttribute = new Regex(@"\[Xml(Element|Array|Attribute)(?:\((""[^""]+"")?(?:, )?(.*)\))?\]");
        private static readonly Regex OmniProperty = new Regex(@"public (?!(?:partial class|class|enum) )([A-Za-z0-9_]+(?:\[\])?) ([a-z]\w*)(?<!Specified) ?(\{.*)$");

        private delegate IEnumerable<string> RenameProcessor(string[] lines, out int linesUsed);

        public static IEnumerable<string> Apply(IEnumerable<string> lineEnumerable)
        {
            var enumerator = lineEnumerable.GetEnumerator();

            var lines = new string[3];
            for (var i = 0; i < lines.Length; i++)
            {
                if (!enumerator.MoveNext())
                    throw new Exception();
                lines[i] = enumerator.Current;
            }

            var renameProcessor = new List<RenameProcessor>
            {
                X3Line,
                X2Line,
            };

            while (lines[0] != null)
            {
                var linesUsed = 0;

                foreach (var processor in renameProcessor)
                {
                    var output = processor(lines, out linesUsed);
                    if (linesUsed == 0)
                        continue;
                    foreach (var line in output)
                        yield return line;
                    break;
                }

                if (linesUsed == 0)
                {
                    yield return lines[0];
                    linesUsed = 1;
                }

                var i = 0;
                for (; linesUsed + i < lines.Length; i++)
                    lines[i] = lines[linesUsed + i];
                for (; i < lines.Length; i++)
                    lines[i] = enumerator.MoveNext() ? enumerator.Current : null;
            }
        }

        private static IEnumerable<string> X2Line(string[] lines, out int linesUsed)
        {
            // 0: [XmlAttribute(...)]
            // 1: public Thing name { get; set; }

            // 0: [XmlElement(...)]
            // 1: public Thing name { get; set; }

            // 0: [XmlArray(...)]
            // 1: public Thing[] name { get; set; }

            if (!EnoughNonNull(lines, 2))
            {
                linesUsed = 0;
                return null;
            }

            var reformatted = TryReformat(lines[0], lines[1]);
            if (reformatted == null)
            {
                linesUsed = 0;
                return null;
            }

            linesUsed = 2;
            return reformatted;
        }

        private static IEnumerable<string> X3Line(string[] lines, out int linesUsed)
        {
            // 0: [XmlAttribute(...)]
            // 1: ...
            // 2: public Thing name { get; set; }

            // 0: [XmlElement(...)]
            // 1: ...
            // 2: public Thing name { get; set; }

            // 0: [XmlElement(...)]
            // 1: ...
            // 2: public Thing[] name { get; set; }

            // 0: [XmlArray(...)]
            // 1: ...
            // 2: public Thing[] name { get; set; }

            if (!EnoughNonNull(lines, 3))
            {
                linesUsed = 0;
                return null;
            }

            var reformatted = TryReformat(lines[0], lines[2]);
            if (reformatted == null)
            {
                linesUsed = 0;
                return null;
            }

            linesUsed = 3;
            reformatted.Insert(1, lines[1]);
            return reformatted;
        }

        private static List<string> TryReformat(string attributeLine, string propertyLine)
        {
            var attrMatch = OmniAttribute.Match(attributeLine);
            var propMatch = OmniProperty.Match(propertyLine);
            if (!attrMatch.Success || !propMatch.Success)
                return null;

            var attributeTypeValue = attrMatch.Groups[1].Captures[0].Value;
            var nameOverride = attrMatch.Groups[2];
            var options = attrMatch.Groups[3];

            var type = propMatch.Groups[1].Captures[0].Value;
            var originalName = propMatch.Groups[2].Captures[0].Value;
            var remainder = propMatch.Groups[3].Captures[0].Value;

            var serializedName = nameOverride.Success ? nameOverride.Value : $@"""{originalName}""";
            var newName = originalName.UppercaseFirst();
            var newOptions = options.Success && !String.IsNullOrWhiteSpace(options.Value) ? ", " + options.Value : String.Empty;

            return new List<string>
            {
                $@"{Indentation}[Xml{attributeTypeValue}({serializedName}{newOptions})]",
                $"{Indentation}public {type} {newName} {remainder}",
            };
        }

        private static bool EnoughNonNull(string[] lines, int n)
        {
            if (n > lines.Length)
                throw new ArgumentOutOfRangeException(nameof(n));

            for (var i = 0; i < n; i++)
                if (lines[i] == null)
                    return false;
            return true;
        }
    }
}
