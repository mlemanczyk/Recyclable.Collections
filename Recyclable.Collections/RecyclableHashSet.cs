using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal sealed class RecyclableHashSet<T> : IEnumerable<T>, IDisposable
        where T : notnull
    {
        internal static readonly bool _needsClearing = !typeof(T).IsValueType;

        internal readonly int _blockSize;
        internal readonly int _blockSizeMinus1;
        internal readonly byte _blockShift;

#nullable disable
        internal Entry[][] _entries;
#nullable restore
        internal int[] _buckets;
        internal int _count;
        internal bool _disposed;

        internal struct Entry
        {
            public int HashCode;
            public int Next;
            public T Value;
        }

        public RecyclableHashSet(int blockSize = RecyclableDefaults.BlockSize)
        {
            if (!BitOperations.IsPow2((uint)blockSize))
            {
                blockSize = (int)BitOperations.RoundUpToPowerOf2((uint)blockSize);
            }

            _blockSize = blockSize;
            _blockSizeMinus1 = blockSize - 1;
            _blockShift = (byte)(31 - BitOperations.LeadingZeroCount((uint)blockSize));

            _entries = 4 >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<Entry[]>.RentShared(4)
                : new Entry[4][];
            _entries[0] = blockSize >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<Entry>.RentShared(blockSize)
                : new Entry[blockSize];
            _buckets = new int[4];
            Array.Fill(_buckets, -1);
        }

        public int Count => _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item)
        {
            var hash = item.GetHashCode() & int.MaxValue;
            if (_count >= (_buckets.Length * 3) / 4)
            {
                ResizeBuckets(_buckets.Length * 2);
            }

            EnsureCapacity(_count + 1);

            int bucket = hash & (_buckets.Length - 1);
            int index = _buckets[bucket];
            while (index >= 0)
            {
                ref var check = ref GetEntry(index);
                if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, item))
                {
                    return false;
                }

                index = check.Next;
            }

            int newIndex = _count++;
            ref var entry = ref GetEntry(newIndex);
            entry.HashCode = hash;
            entry.Value = item;
            entry.Next = _buckets[bucket];
            _buckets[bucket] = newIndex;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => FindIndex(item, out _) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            if (_count == 0)
            {
                return false;
            }

            var hash = item.GetHashCode() & int.MaxValue;
            int bucket = hash & (_buckets.Length - 1);
            int prev = -1;
            int index = _buckets[bucket];
            while (index >= 0)
            {
                ref var entry = ref GetEntry(index);
                if (entry.HashCode == hash && EqualityComparer<T>.Default.Equals(entry.Value, item))
                {
                    if (prev < 0)
                    {
                        _buckets[bucket] = entry.Next;
                    }
                    else
                    {
                        GetEntry(prev).Next = entry.Next;
                    }

                    int lastIndex = _count - 1;
                    if (index != lastIndex)
                    {
                        MoveEntry(lastIndex, index);
                    }

                    ClearEntry(lastIndex);
                    _count--;

                    return true;
                }

                prev = index;
                index = entry.Next;
            }

            return false;
        }

        public void Clear()
        {
            if (_count == 0)
            {
                Array.Fill(_buckets, -1);
                return;
            }

            int blockIndex = 0;
            if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
            {
                while (blockIndex < _entries.Length && _entries[blockIndex] != null)
                {
                    RecyclableArrayPool<Entry>.ReturnShared(_entries[blockIndex]!, _needsClearing);
                    _entries[blockIndex++] = null!;
                }
            }
            else
            {
                if (_needsClearing)
                {
                    while (blockIndex < _entries.Length && _entries[blockIndex] != null)
                    {
                        Array.Clear(_entries[blockIndex]!, 0, _entries[blockIndex]!.Length);
                        _entries[blockIndex++] = null!;
                    }
                }
                else
                {
                    while (blockIndex < _entries.Length && _entries[blockIndex] != null)
                    {
                        _entries[blockIndex++] = null!;
                    }
                }
            }

            _count = 0;
            Array.Fill(_buckets, -1);
        }

        public Enumerator GetEnumerator() => new(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Clear();
            if (_entries.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<Entry[]>.ReturnShared(_entries, false);
            }

            if (_buckets.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<int>.ReturnShared(_buckets, false);
            }

            _entries = Array.Empty<Entry[]>();
            _buckets = Array.Empty<int>();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private ref Entry GetEntry(int index)
        {
            return ref _entries[index >> _blockShift][index & _blockSizeMinus1];
        }

        private void ClearEntry(int index)
        {
            ref var entry = ref GetEntry(index);
            entry.HashCode = 0;
            entry.Next = -1;
            if (_needsClearing)
            {
                entry.Value = default!;
            }
        }

        private void MoveEntry(int sourceIndex, int targetIndex)
        {
            ref var source = ref GetEntry(sourceIndex);
            ref var target = ref GetEntry(targetIndex);
            target = source;

            int bucket = source.HashCode & (_buckets.Length - 1);
            int cur = _buckets[bucket];
            int prev = -1;
            while (cur != sourceIndex)
            {
                prev = cur;
                cur = GetEntry(cur).Next;
            }

            if (prev < 0)
            {
                _buckets[bucket] = targetIndex;
            }
            else
            {
                GetEntry(prev).Next = targetIndex;
            }
        }

        private void EnsureCapacity(int min)
        {
            int requiredBlocks = (min + _blockSizeMinus1) >> _blockShift;
            if (_entries.Length < requiredBlocks)
            {
                var newBlocks = requiredBlocks >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<Entry[]>.RentShared(requiredBlocks)
                    : new Entry[requiredBlocks][];

                Array.Copy(_entries, newBlocks, _entries.Length);

                if (_entries.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<Entry[]>.ReturnShared(_entries, false);
                }

                _entries = newBlocks;
            }

            for (int i = 0; i < requiredBlocks; i++)
            {
                if (_entries[i] == null)
                {
                    _entries[i] = _blockSize >= RecyclableDefaults.MinPooledArrayLength
                        ? RecyclableArrayPool<Entry>.RentShared(_blockSize)
                        : new Entry[_blockSize];
                }
            }
        }

        private void ResizeBuckets(int newSize)
        {
            int[] newBuckets = newSize >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<int>.RentShared(newSize)
                : new int[newSize];
            Array.Fill(newBuckets, -1);
            for (int i = 0; i < _count; i++)
            {
                ref var entry = ref GetEntry(i);
                int bucket = entry.HashCode & (newSize - 1);
                entry.Next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }

            if (_buckets.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<int>.ReturnShared(_buckets, false);
            }

            _buckets = newBuckets;
        }

        private int FindIndex(T item, out int hash)
        {
            if (_count == 0)
            {
                hash = 0;
                return -1;
            }

            hash = item.GetHashCode() & int.MaxValue;
            int bucket = hash & (_buckets.Length - 1);
            int index = _buckets[bucket];
            while (index >= 0)
            {
                ref var entry = ref GetEntry(index);
                if (entry.HashCode == hash && EqualityComparer<T>.Default.Equals(entry.Value, item))
                {
                    return index;
                }

                index = entry.Next;
            }

            return -1;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly RecyclableHashSet<T> _set;
            private int _index;
            private T? _current;

            internal Enumerator(RecyclableHashSet<T> set)
            {
                _set = set;
                _index = 0;
                _current = default;
            }

            public T Current => _current!;

            object IEnumerator.Current => _current!;

            public bool MoveNext()
            {
                if (_index >= _set._count)
                {
                    return false;
                }

                _current = _set.GetEntry(_index++).Value;
                return true;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public void Dispose()
            {
            }
        }
    }
}
