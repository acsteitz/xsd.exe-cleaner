using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XsdDotExeCleaner
{
    static class SimpleFilters
    {
        /// <summary>
        /// Properties are implemented with backing fields.  This will remove the backing field
        /// declaration.
        /// </summary>
        /// <example>
        /// <code>
        ///     private int thingField;
        ///     public int thing {
        ///         get {
        ///             return this.thingField;
        ///         }
        ///         set {
        ///             this.thingField = value;
        ///         }
        ///     }
        /// </code>
        /// 
        /// Will become:
        /// <code>
        ///     public int thing {
        ///         get {
        ///             return this.thingField;
        ///         }
        ///         set {
        ///             this.thingField = value;
        ///         }
        ///     }
        /// </code>
        /// </example>
        /// <seealso cref="SimpleProperties"/>
        /// <seealso cref="FixThisField"/>
        public static IEnumerable<string> RemovePrivateFields(IEnumerable<string> lines)
        {
            var enumerator = lines.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (Regex.IsMatch(enumerator.Current, @"^\s*private"))
                {
                    enumerator.MoveNext();
                    continue;
                }

                yield return enumerator.Current;
            }
        }

        /// <summary>
        /// Properties are implemented with explicit getters and setters.  This will implify them.
        /// </summary>
        /// <example>
        /// <code>
        ///     public int thing {
        ///         get {
        ///             return this.thingField;
        ///         }
        ///         set {
        ///             this.thingField = value;
        ///         }
        ///     }
        /// </code>
        /// 
        /// Will become:
        /// <code>
        ///     public int thing { get; set; }
        /// </code>
        /// </example>
        /// <seealso cref="RemovePrivateFields"/>
        /// <seealso cref="FixThisField"/>
        public static IEnumerable<string> SimpleProperties(IEnumerable<string> lines)
        {
            var enumerator = lines.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var match = Regex.Match(enumerator.Current, @"^(\s+public [^ ]+ [^ ]+ \{)");
                if (!match.Success)
                {
                    yield return enumerator.Current;
                    continue;
                }

                yield return $"{match.Value} get; set; }}";

                for (var i = 0; i < 7; i++)
                    enumerator.MoveNext();
            }
        }

        /// <summary>
        /// Fix references to backing fields in constructors when assigning default values.
        /// </summary>
        /// <example>
        /// <code>
        ///     public ClassName() {
        ///         this.thingField = 42;
        ///     }
        /// </code>
        /// 
        /// Will become:
        /// <code>
        ///     public ClassName() {
        ///         Thing = 42;
        ///     }
        /// </code>
        /// </example>
        /// <seealso cref="RemovePrivateFields"/>
        /// <seealso cref="SimpleProperties"/>
        public static IEnumerable<string> FixThisField(IEnumerable<string> lines)
        {
            MatchEvaluator matchEvaluator = match =>
            {
                var name = match.Groups[1].Captures[0].Value;
                return name.UppercaseFirst();
            };

            return lines.Select(line => Regex.Replace(line, @"this\.(\w+)Field", matchEvaluator));
        }

        /// <summary>
        /// Removes the extra line inside a class or enum.
        /// </summary>
        public static IEnumerable<string> RemoveExtraLines(IEnumerable<string> lines)
        {
            var enumerator = lines.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var match = Regex.Match(enumerator.Current, @"(class|enum).*\{$");
                if (!match.Success)
                {
                    yield return enumerator.Current;
                    continue;
                }

                yield return enumerator.Current;
                enumerator.MoveNext();
            }
        }

        /// <summary>
        /// Fixes the casing of properties indicating if another property is specified.
        /// </summary>
        public static IEnumerable<string> FixSpecifiedPropertyCasing(IEnumerable<string> lines)
        {
            MatchEvaluator matchEvaluator = match =>
            {
                var name = match.Groups[1].Captures[0].Value;
                return $"public bool {name.UppercaseFirst()}Specified";
            };

            return lines.Select(line => Regex.Replace(line, @"public bool (\w+)Specified", matchEvaluator));
        }
    }
}
