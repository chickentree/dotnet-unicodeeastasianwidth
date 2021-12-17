using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace ChickenTree.GenUnicodeEastAsianWidth
{
    internal class DataTable<T>
    {
        private readonly Dictionary<ImmutableArray<T>, int> _cache;
        private readonly List<T> _data;

        internal DataTable(int bits)
        {
            if (0 > bits || 31 <= bits)
            {
                throw new ArgumentOutOfRangeException(nameof(bits));
            }

            _cache = new Dictionary<ImmutableArray<T>, int>(new StructuralComparer<ImmutableArray<T>>());
            _data = new List<T>();
            Block = 1 << bits;
            Bits = bits;
        }

        internal int Block { get; }
        internal int Bits { get; }
        internal ReadOnlyCollection<T> Data => _data.AsReadOnly();

        internal int AddRow(params T[] items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return AddRow(ImmutableArray.Create(items));
        }

        internal int AddRow(T[] items, int start, int length)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return AddRow(ImmutableArray.Create(items, start, length));
        }

        internal int AddRow(ImmutableArray<T> items)
        {
            if (Block != items.Length)
            {
                throw new ArgumentException($"Length should be {Block}", nameof(items));
            }

            if (!_cache.TryGetValue(items, out var index))
            {
                index = _data.Count;
                _cache[items] = index;

                _data.AddRange(items);
            }

            return index;
        }

        internal int[] Add(params T[] items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return Add(items, 0, items.Length);
        }

        internal int[] Add(T[] items, int start, int length)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (0 > start || items.Length < start)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (0 > length || items.Length < start + length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            length = (int)Math.DivRem(length, Block, out long remainder);

            if (0 != remainder)
            {
                throw new ArgumentException("Not in alignment", nameof(length));
            }

            var result = new int[length];

            for (var i = 0; i < length; ++i)
            {
                var index = AddRow(items, start, Block);
                result[i] = index;
                start += Block;
            }

            return result;
        }
    }
}
