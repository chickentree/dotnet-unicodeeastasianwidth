using System;
using System.Globalization;
using System.IO;
using ChickenTree.UnicodeEastAsianWidth;

namespace ChickenTree.GenUnicodeEastAsianWidth
{
    internal static class EastAsianWidthFile
    {
        internal static UnicodeEastAsianWidthNames[] FromStream(TextReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var result = new UnicodeEastAsianWidthNames[0x110000];

            Array.Fill(result, UnicodeEastAsianWidthNames.Neutral);

            while (true)
            {
                var line = reader.ReadLine();

                if (line is null)
                {
                    break;
                }

                line = line.Split('#', 2)[0].Trim();

                if (0 < line.Length)
                {
                    if (!Entry.TryParse(line, out var entry))
                    {
                        throw new InvalidOperationException();
                    }

                    for (var codePoint = entry.FirstCodePoint; codePoint <= entry.LastCodePoint; ++codePoint)
                    {
                        result[codePoint] = entry.Name;
                    }
                }
            }

            return result;
        }

        private static bool TryParseName(string? s, out UnicodeEastAsianWidthNames result)
        {
            switch (s)
            {
                case "F":
                    result = UnicodeEastAsianWidthNames.FullWidth;
                    return true;
                case "H":
                    result = UnicodeEastAsianWidthNames.HalfWidth;
                    return true;
                case "W":
                    result = UnicodeEastAsianWidthNames.Wide;
                    return true;
                case "Na":
                    result = UnicodeEastAsianWidthNames.Narrow;
                    return true;
                case "A":
                    result = UnicodeEastAsianWidthNames.Ambiguous;
                    return true;
                case "N":
                    result = UnicodeEastAsianWidthNames.Neutral;
                    return true;
            }

            result = default;
            return false;
        }

        internal readonly struct Entry : IEquatable<Entry>
        {
            internal Entry(int firstCodePoint, int lastCodePoint, UnicodeEastAsianWidthNames name)
            {
                if (0 > firstCodePoint || 0x10FFFF < firstCodePoint)
                {
                    throw new ArgumentOutOfRangeException(nameof(firstCodePoint));
                }
                if (0 > lastCodePoint || 0x10FFFF < lastCodePoint)
                {
                    throw new ArgumentOutOfRangeException(nameof(lastCodePoint));
                }
                if (firstCodePoint > lastCodePoint)
                {
                    throw new ArgumentException($"Must be greater than or equal to {nameof(firstCodePoint)}", nameof(lastCodePoint));
                }

                FirstCodePoint = firstCodePoint;
                LastCodePoint = lastCodePoint;
                Name = name;
            }

            internal int FirstCodePoint { get; }
            internal int LastCodePoint { get; }
            internal UnicodeEastAsianWidthNames Name { get; }

            public override bool Equals(object? obj)
            {
                return obj is Entry entry && Equals(entry);
            }

            public bool Equals(Entry other)
            {
                return FirstCodePoint == other.FirstCodePoint &&
                       LastCodePoint == other.LastCodePoint &&
                       Name == other.Name;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(FirstCodePoint, LastCodePoint, Name);
            }

            internal static bool TryParse(string? s, out Entry result)
            {
                if (s is null)
                {
                    result = default;
                    return false;
                }

                var fields = s.Split(';', 2);

                if (2 != fields.Length)
                {
                    result = default;
                    return false;
                }

                var range = fields[0].Split("..", 2);

                if (!int.TryParse(range[0], NumberStyles.HexNumber, null, out var first) || !int.TryParse(range[^1], NumberStyles.HexNumber, null, out var last) || !TryParseName(fields[1], out var name))
                {
                    result = default;
                    return false;
                }

                result = new Entry(first, last, name);
                return true;
            }
        }
    }
}
