using System;
using System.IO;

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

            return 0;
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
