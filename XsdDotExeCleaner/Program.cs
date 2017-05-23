using System;
using System.Collections.Generic;
using System.IO;

namespace XsdDotExeCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: XsdDotExeCleaner.exe inputFile outputFile");
                return;
            }

            var inputFileName = args[0];
            var outputFileName = args[1];

            var filters = new List<Filter>
            {
                // Unnecessary attributes
                new DeleteLineFilter("DebuggerStepThrough"),
                new DeleteLineFilter("GeneratedCode"),
                new DeleteLineFilter("DesignerCategory"),

                // Useless comment
                new DeleteLineFilter("<remarks/>"),

                // [XmlElementAttribute()] -> [XmlElement]
                new ReplaceFilter("Attribute()", ""),
                // [XmlElementAttribute(...)] -> [XmlElement(...)]
                new ReplaceFilter("Attribute(", "("),

                // Simplify type names
                new ReplaceFilter("[System.Xml.Serialization.", "["),
                new ReplaceFilter("System.Serializable", "Serializable"),
                new ReplaceFilter("System.DateTime", "DateTime"),
                new ReplaceFilter("System.ComponentModel.DefaultValue", "DefaultValue"),

                SimpleFilters.RemoveExtraLines,
                SimpleFilters.RemovePrivateFields,
                SimpleFilters.SimpleProperties,
                SimpleFilters.FixThisField,
                SimpleFilters.FixSpecifiedPropertyCasing,

                RenameFilter.Apply,
            };

            var lines = GetLines(inputFileName);

            foreach (var filter in filters)
                lines = filter(lines);

            Save(lines, outputFileName);
        }

        private static IEnumerable<string> GetLines(string inputFileName)
        {
            using (var input = new StreamReader(inputFileName))
            {
                for (var line = input.ReadLine(); line != null; line = input.ReadLine())
                    yield return line;
            }
        }

        private static void Save(IEnumerable<string> lines, string outputFileName)
        {
            using (var output = new StreamWriter(outputFileName))
            {
                foreach (var line in lines)
                    output.WriteLine(line);
            }
        }
    }
}
