using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal static class zRecyclableHashSetAddRange
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref RecyclableHashSet<T>.Entry GetEntry<T>(RecyclableHashSet<T> set, int index)
            where T : notnull
        {
            return ref set._entries[index >> set._blockShift][index & set._blockSizeMinus1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCapacity<T>(RecyclableHashSet<T> set, int min)
            where T : notnull
        {
            int requiredBlocks = (min + set._blockSizeMinus1) >> set._blockShift;
            if (set._entries.Length < requiredBlocks)
            {
                var newBlocks = requiredBlocks >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<RecyclableHashSet<T>.Entry[]>.RentShared(requiredBlocks)
                    : new RecyclableHashSet<T>.Entry[requiredBlocks][];

                Array.Copy(set._entries, newBlocks, set._entries.Length);

                if (set._entries.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<RecyclableHashSet<T>.Entry[]>.ReturnShared(set._entries, false);
                }

                set._entries = newBlocks;
            }

            for (int i = 0; i < requiredBlocks; i++)
            {
                if (set._entries[i] == null)
                {
                    set._entries[i] = set._blockSize >= RecyclableDefaults.MinPooledArrayLength
                        ? RecyclableArrayPool<RecyclableHashSet<T>.Entry>.RentShared(set._blockSize)
                        : new RecyclableHashSet<T>.Entry[set._blockSize];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResizeBuckets<T>(RecyclableHashSet<T> set, int newSize)
            where T : notnull
        {
            int[] newBuckets = newSize >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<int>.RentShared(newSize)
                : new int[newSize];
            Array.Fill(newBuckets, -1);

            for (int i = 0; i < set._count; i++)
            {
                ref var entry = ref GetEntry(set, i);
                int bucket = entry.HashCode & (newSize - 1);
                entry.Next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }

            if (set._buckets.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<int>.ReturnShared(set._buckets, false);
            }

            set._buckets = newBuckets;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, ReadOnlySpan<T> items)
            where T : notnull
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;

            for (int i = 0; i < count; i++)
            {
                T value = items[i];
                int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                    {
                        exists = true;
                        break;
                    }

                    index = check.Next;
                }

                if (exists)
                {
                    continue;
                }

                ref var entry = ref GetEntry(set, insertIndex);
                entry.HashCode = hash;
                entry.Value = value!;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, Span<T> items)
            where T : notnull
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;

            for (int i = 0; i < count; i++)
            {
                T value = items[i];
                int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                    {
                        exists = true;
                        break;
                    }

                    index = check.Next;
                }

                if (exists)
                {
                    continue;
                }

                ref var entry = ref GetEntry(set, insertIndex);
                entry.HashCode = hash;
                entry.Value = value!;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, T[] items)
            where T : notnull
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;

            for (int i = 0; i < count; i++)
            {
                T value = items[i];
                int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                    {
                        exists = true;
                        break;
                    }

                    index = check.Next;
                }

                if (exists)
                {
                    continue;
                }

                ref var entry = ref GetEntry(set, insertIndex);
                entry.HashCode = hash;
                entry.Value = value!;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, List<T> items)
            where T : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;
            Span<T> span = CollectionsMarshal.AsSpan(items);

            for (int i = 0; i < count; i++)
            {
                T value = span[i];
                int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                    {
                        exists = true;
                        break;
                    }

                    index = check.Next;
                }

                if (exists)
                {
                    continue;
                }

                ref var entry = ref GetEntry(set, insertIndex);
                entry.HashCode = hash;
                entry.Value = value!;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, RecyclableList<T> items)
            where T : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;
            T[] source = items._memoryBlock;

            for (int i = 0; i < count; i++)
            {
                T value = source[i];
                int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                    {
                        exists = true;
                        break;
                    }

                    index = check.Next;
                }

                if (exists)
                {
                    continue;
                }

                ref var entry = ref GetEntry(set, insertIndex);
                entry.HashCode = hash;
                entry.Value = value!;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, RecyclableHashSet<T> items)
            where T : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;

            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;

            for (int i = 0; i < count; i++)
            {
                T value = entries[i >> shift][i & mask].Value;
                int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                    {
                        exists = true;
                        break;
                    }

                    index = check.Next;
                }

                if (exists)
                {
                    continue;
                }

                ref var entry = ref GetEntry(set, insertIndex);
                entry.HashCode = hash;
                entry.Value = value!;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
            }

        set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, RecyclableLongList<T> items)
            where T : notnull
        {
            long longCount = items._longCount;
            if (longCount == 0)
            {
                return;
            }

            if (longCount > int.MaxValue - set._count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items exceeds {int.MaxValue}");
            }

            int count = (int)longCount;
            int fullBlocks = (int)(longCount >> items._blockSizePow2BitShift);
            int lastBlockLength = (int)(longCount & items._blockSizeMinus1);
            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;
            var blocks = items._memoryBlocks;
            int blockSize = items._blockSize;

            for (int block = 0; block < fullBlocks; block++)
            {
                var source = blocks[block];
                for (int j = 0; j < blockSize; j++)
                {
                    T value = source[j];
                    int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                    int bucket = hash & bucketMask;

                    int index = set._buckets[bucket];
                    bool exists = false;
                    while (index >= 0)
                    {
                        ref var check = ref GetEntry(set, index);
                        if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                        {
                            exists = true;
                            break;
                        }

                        index = check.Next;
                    }

                    if (exists)
                    {
                        continue;
                    }

                    ref var entry = ref GetEntry(set, insertIndex);
                    entry.HashCode = hash;
                    entry.Value = value!;
                    entry.Next = set._buckets[bucket];
                    set._buckets[bucket] = insertIndex;
                    insertIndex++;
                }
            }

            if (lastBlockLength > 0)
            {
                var source = blocks[fullBlocks];
                for (int j = 0; j < lastBlockLength; j++)
                {
                    T value = source[j];
                    int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                    int bucket = hash & bucketMask;

                    int index = set._buckets[bucket];
                    bool exists = false;
                    while (index >= 0)
                    {
                        ref var check = ref GetEntry(set, index);
                        if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                        {
                            exists = true;
                            break;
                        }

                        index = check.Next;
                    }

                    if (exists)
                    {
                        continue;
                    }

                    ref var entry = ref GetEntry(set, insertIndex);
                    entry.HashCode = hash;
                    entry.Value = value!;
                    entry.Next = set._buckets[bucket];
                    set._buckets[bucket] = insertIndex;
                    insertIndex++;
                }
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, RecyclableStack<T> items)
            where T : notnull
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            if (longCount > int.MaxValue - set._count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items exceeds {int.MaxValue}");
            }

            int count = (int)longCount;
            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;

            var sourceChunk = items._current;
            while (sourceChunk.Previous != null)
            {
                sourceChunk = sourceChunk.Previous;
            }

            while (sourceChunk != null)
            {
                var source = sourceChunk.Value;
                int sourceCount = sourceChunk.Index;
                for (int i = 0; i < sourceCount; i++)
                {
                    T value = source[i];
                    int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                    int bucket = hash & bucketMask;

                    int idx = set._buckets[bucket];
                    bool exists = false;
                    while (idx >= 0)
                    {
                        ref var check = ref GetEntry(set, idx);
                        if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                        {
                            exists = true;
                            break;
                        }

                        idx = check.Next;
                    }

                    if (exists)
                    {
                        continue;
                    }

                    ref var entry = ref GetEntry(set, insertIndex);
                    entry.HashCode = hash;
                    entry.Value = value!;
                    entry.Next = set._buckets[bucket];
                    set._buckets[bucket] = insertIndex;
                    insertIndex++;
                }

                sourceChunk = sourceChunk.Next!;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, RecyclableQueue<T> items)
            where T : notnull
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            if (longCount > int.MaxValue - set._count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items exceeds {int.MaxValue}");
            }

            int count = (int)longCount;
            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;

            var chunk = items._head;
            while (chunk != null)
            {
                int idx = chunk.Bottom;
                int top = chunk.Top;
                var source = chunk.Value;

                while (idx < top)
                {
                    T value = source[idx++];
                    int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                    int bucket = hash & bucketMask;

                    int checkIndex = set._buckets[bucket];
                    bool exists = false;
                    while (checkIndex >= 0)
                    {
                        ref var check = ref GetEntry(set, checkIndex);
                        if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                        {
                            exists = true;
                            break;
                        }

                        checkIndex = check.Next;
                    }

                    if (exists)
                    {
                        continue;
                    }

                    ref var entry = ref GetEntry(set, insertIndex);
                    entry.HashCode = hash;
                    entry.Value = value!;
                    entry.Next = set._buckets[bucket];
                    set._buckets[bucket] = insertIndex;
                    insertIndex++;
                }

                chunk = chunk.Next;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, IReadOnlyList<T> items)
            where T : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            int requiredCount = set._count + count;
            EnsureCapacity(set, requiredCount);

            int bucketsLength = set._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != set._buckets.Length)
            {
                ResizeBuckets(set, bucketsLength);
            }

            int bucketMask = set._buckets.Length - 1;
            int insertIndex = set._count;

            for (int i = 0; i < count; i++)
            {
                T value = items[i];
                int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                    {
                        exists = true;
                        break;
                    }

                    index = check.Next;
                }

                if (exists)
                {
                    continue;
                }

                ref var entry = ref GetEntry(set, insertIndex);
                entry.HashCode = hash;
                entry.Value = value!;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, IEnumerable<T> items)
            where T : notnull
        {
            foreach (T value in items)
            {
                int requiredCount = set._count + 1;
                if (requiredCount > set._entries.Length * set._blockSize)
                {
                    EnsureCapacity(set, requiredCount);
                }

                if (set._count >= (set._buckets.Length * 3) / 4)
                {
                    ResizeBuckets(set, set._buckets.Length << 1);
                }

                int hash = value?.GetHashCode() & int.MaxValue ?? 0;
                int bucketMask = set._buckets.Length - 1;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<T>.Default.Equals(check.Value, value))
                    {
                        exists = true;
                        break;
                    }

                    index = check.Next;
                }

                if (exists)
                {
                    continue;
                }

                int insertIndex = set._count++;
                ref var entry = ref GetEntry(set, insertIndex);
                entry.HashCode = hash;
                entry.Value = value!;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
            }
        }
    }
}
