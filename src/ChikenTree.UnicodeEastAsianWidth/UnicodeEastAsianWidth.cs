using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ChickenTree.UnicodeEastAsianWidth
{
    public static partial class UnicodeEastAsianWidth
    {
        public static UnicodeEastAsianWidthNames GetUnicodeEastAsianWidth(char ch)
        {
            return GetUnicodeEastAsianWidth((int)ch);
        }

        public static UnicodeEastAsianWidthNames GetUnicodeEastAsianWidth(int codePoint)
        {
            var index1 = codePoint >> Shift1;
            var lookup1 = Unsafe.Add(ref MemoryMarshal.GetReference(Level1), index1);
            var index2 = lookup1 + ((codePoint >> Shift2) & Mask2);
            var lookup2 = Unsafe.Add(ref MemoryMarshal.GetReference(Level2), index2);
            var index3 = lookup2 + (codePoint & Mask3);
            var lookup3 = Unsafe.Add(ref MemoryMarshal.GetReference(Level3), index3);
            return (UnicodeEastAsianWidthNames)lookup3;
        }
    }
}
