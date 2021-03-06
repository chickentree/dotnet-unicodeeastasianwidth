using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChickenTree.GenUnicodeEastAsianWidth
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            FileInfo? input = null;
            FileInfo? output = null;
            var optind = 0;

            while (optind < args.Length)
            {
                var next = args[optind++];

                switch (next)
                {
                    case "-i":
                    case "--input":
                        {
                            var optargs = args.AsSpan(optind);
                            if (optargs.IsEmpty)
                            {
                                PrintUsage();
                                return 1;
                            }
                            input = new FileInfo(optargs[0]);
                            optind += 1;
                        }
                        break;
                    case "-o":
                    case "--output":
                        {
                            var optargs = args.AsSpan(optind);
                            if (optargs.IsEmpty)
                            {
                                PrintUsage();
                                return 1;
                            }
                            output = new FileInfo(optargs[0]);
                            optind += 1;
                        }
                        break;
                    default:
                        PrintUsage();
                        return 1;
                }
            }

            if (input is null)
            {
                input = new FileInfo("EastAsianWidth.txt");
            }
            if (output is null)
            {
                output = new FileInfo("UnicodeEastAsianWidthData.cs");
            }

            var data = EastAsianWidthFile.FromStream(input.OpenText()).Cast<byte>().ToArray();
            const int Planes = 17;
            (int size, int level2Bits, int level3Bits, ICollection<int> level1, ICollection<int> level2, ICollection<byte> level3)? best = null;

            for (var level3Bits = 0; level3Bits <= 16; ++level3Bits)
            {
                for (var level2Bits = 0; level2Bits <= 16 - level3Bits; ++level2Bits)
                {
                    var level1Bits = 16 - level2Bits - level3Bits;
                    var c3 = new DataTable<byte>(level3Bits);
                    var c2 = new DataTable<int>(level2Bits);
                    var level2RowData = new int[c2.Block];
                    var level1Data = new int[Planes << level1Bits];

                    var index = 0;

                    for (var i = 0; i < level1Data.Length; ++i)
                    {
                        for (var j = 0; j < level2RowData.Length; ++j)
                        {
                            level2RowData[j] = c3.AddRow(data, index, c3.Block);
                            index += c3.Block;
                        }
                        level1Data[i] = c2.AddRow(level2RowData);
                    }

                    ICollection<int> level1 = level1Data;
                    ICollection<int> level2 = c2.Data;
                    ICollection<byte> level3 = c3.Data;

                    var size = (level1.Count * sizeof(int)) + (level2.Count * sizeof(int)) + (level3.Count * sizeof(byte));

                    if (best is null || size < best.Value.size)
                    {
                        best = (size, level2Bits, level3Bits, level1, level2, level3);
                    }
                }
            }

            if (best is null)
            {
                throw new InvalidOperationException();
            }

            using (var writer = output.CreateText())
            {
                writer.WriteLine($"using System;");

                writer.WriteLine();

                writer.WriteLine("namespace ChickenTree.UnicodeEastAsianWidth");
                writer.WriteLine("{");

                writer.WriteLine("    public static partial class UnicodeEastAsianWidth");
                writer.WriteLine("    {");

                writer.WriteLine($"        private const int Shift1 = {best.Value.level2Bits + best.Value.level3Bits};");
                writer.WriteLine($"        private const int Shift2 = {best.Value.level3Bits};");
                writer.WriteLine($"        private const int Mask2 = {~(-1 << best.Value.level2Bits)};");
                writer.WriteLine($"        private const int Mask3 = {~(-1 << best.Value.level3Bits)};");

                writer.WriteLine();

                writer.WriteLine($"        private static ReadOnlySpan<int> Level1 => new int[{best.Value.level1.Count}]");
                writer.WriteLine("        {");
                WriteCSV(writer, best.Value.level1, 12, 8);
                writer.WriteLine("        };");

                writer.WriteLine();

                writer.WriteLine($"        private static ReadOnlySpan<int> Level2 => new int[{best.Value.level2.Count}]");
                writer.WriteLine("        {");
                WriteCSV(writer, best.Value.level2, 12, 8);
                writer.WriteLine("        };");

                writer.WriteLine();

                writer.WriteLine($"        private static ReadOnlySpan<byte> Level3 => new byte[{best.Value.level3.Count}]");
                writer.WriteLine("        {");
                WriteCSV(writer, best.Value.level3.Select(value => (int)value), 12, 8);
                writer.WriteLine("        };");

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            return 0;
        }

        private static void WriteCSV(TextWriter writer, IEnumerable<int> source, int indent = 0, int? chunk = default)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (0 > indent)
            {
                throw new ArgumentOutOfRangeException(nameof(indent));
            }
            if (0 >= chunk)
            {
                throw new ArgumentOutOfRangeException(nameof(chunk));
            }

            var enumerator = source.GetEnumerator();
            var b = enumerator.MoveNext();

            while (b)
            {
                var i = 0;

                writer.Write(new string(' ', indent));

                do
                {
                    writer.Write(string.Format("0x{0:x2},", enumerator.Current));

                    b = enumerator.MoveNext();
                    i += 1;

                    if (!b || i >= chunk)
                    {
                        break;
                    }

                    writer.Write(" ");
                }
                while (b);

                writer.WriteLine();
            }
        }

        private static void PrintUsage()
        {
            PrintUsage(Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]));
        }

        private static void PrintUsage(string programName)
        {
            if (programName is null)
            {
                throw new ArgumentNullException(nameof(programName));
            }

            Console.WriteLine($"Usage: {programName} [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -i, --input <INPUT>   EastAsianWidth.txt");
            Console.WriteLine("  -o, --output <OUTPUT> UnicodeEastAsianWidthData.cs");
            Console.WriteLine("  -h, --help            Print this help message and exit");
        }
    }
}
