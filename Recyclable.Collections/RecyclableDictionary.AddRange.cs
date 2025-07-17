using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal static class zRecyclableDictionaryAddRange
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref RecyclableDictionary<TKey, TValue>.Entry GetEntry<TKey, TValue>(RecyclableDictionary<TKey, TValue> dictionary, int index)
            where TKey : notnull
        {
            return ref dictionary._entries[index >> dictionary._blockShift][index & dictionary._blockSizeMinus1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCapacity<TKey, TValue>(RecyclableDictionary<TKey, TValue> dictionary, int min)
            where TKey : notnull
        {
            int requiredBlocks = (min + dictionary._blockSizeMinus1) >> dictionary._blockShift;
            if (dictionary._entries.Length < requiredBlocks)
            {
                var newBlocks = requiredBlocks >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<RecyclableDictionary<TKey, TValue>.Entry[]>.RentShared(requiredBlocks)
                    : new RecyclableDictionary<TKey, TValue>.Entry[requiredBlocks][];

                Array.Copy(dictionary._entries, newBlocks, dictionary._entries.Length);

                if (dictionary._entries.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<RecyclableDictionary<TKey, TValue>.Entry[]>.ReturnShared(dictionary._entries, false);
                }

                dictionary._entries = newBlocks;
            }

            for (int i = 0; i < requiredBlocks; i++)
            {
                if (dictionary._entries[i] == null)
                {
                    dictionary._entries[i] = dictionary._blockSize >= RecyclableDefaults.MinPooledArrayLength
                        ? RecyclableArrayPool<RecyclableDictionary<TKey, TValue>.Entry>.RentShared(dictionary._blockSize)
                        : new RecyclableDictionary<TKey, TValue>.Entry[dictionary._blockSize];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResizeBuckets<TKey, TValue>(RecyclableDictionary<TKey, TValue> dictionary, int newSize)
            where TKey : notnull
        {
            var newBuckets = new int[newSize];
            Array.Fill(newBuckets, -1);
            for (int i = 0; i < dictionary._count; i++)
            {
                ref var entry = ref GetEntry(dictionary, i);
                int bucket = entry.HashCode & (newSize - 1);
                entry.Next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }

            dictionary._buckets = newBuckets;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, ReadOnlySpan<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;

            for (int i = 0; i < count; i++)
            {
                var kvp = items[i];
                TKey key = kvp.Key;
                TValue value = kvp.Value;
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, Span<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
            => AddRange(dictionary, (ReadOnlySpan<KeyValuePair<TKey, TValue>>)items);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue>[] items)
            where TKey : notnull
            => AddRange(dictionary, (ReadOnlySpan<KeyValuePair<TKey, TValue>>)items);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, RecyclableDictionary<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int shift = items._blockShift;
            int mask = items._blockSizeMinus1;
            var sourceBlocks = items._entries;
            int startIndex = dictionary._count;

            for (int i = 0; i < count; i++)
            {
                ref var sourceEntry = ref sourceBlocks[i >> shift][i & mask];
                TKey key = sourceEntry.Key;
                TValue value = sourceEntry.Value;
                int hash = sourceEntry.HashCode;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, in Array items)
            where TKey : notnull
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            KeyValuePair<TKey, TValue>[] buffer;
            int lower = items.GetLowerBound(0);

            if (items is KeyValuePair<TKey, TValue>[] array && lower == 0)
            {
                buffer = array;
            }
            else
            {
                buffer = count >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<KeyValuePair<TKey, TValue>>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)count)))
                    : new KeyValuePair<TKey, TValue>[count];

                Array.Copy(items, lower, buffer, 0, count);
            }

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;

            for (int i = 0; i < count; i++)
            {
                var kvp = buffer[i];
                TKey key = kvp.Key;
                TValue value = kvp.Value;
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;

            if (!ReferenceEquals(buffer, items) && count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<KeyValuePair<TKey, TValue>>.ReturnShared(buffer, RecyclableDictionary<TKey, TValue>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, List<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;
            var span = CollectionsMarshal.AsSpan(items);

            for (int i = 0; i < count; i++)
            {
                var kvp = span[i];
                TKey key = kvp.Key;
                TValue value = kvp.Value;
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, ICollection items)
            where TKey : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            KeyValuePair<TKey, TValue>[] buffer = count >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<KeyValuePair<TKey, TValue>>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)count)))
                : new KeyValuePair<TKey, TValue>[count];

            items.CopyTo(buffer, 0);

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;

            for (int i = 0; i < count; i++)
            {
                var kvp = buffer[i];
                TKey key = kvp.Key;
                TValue value = kvp.Value;
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<KeyValuePair<TKey, TValue>>.ReturnShared(buffer, RecyclableDictionary<TKey, TValue>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, ICollection<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            KeyValuePair<TKey, TValue>[] buffer = count >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<KeyValuePair<TKey, TValue>>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)count)))
                : new KeyValuePair<TKey, TValue>[count];

            items.CopyTo(buffer, 0);

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;

            for (int i = 0; i < count; i++)
            {
                var kvp = buffer[i];
                TKey key = kvp.Key;
                TValue value = kvp.Value;
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<KeyValuePair<TKey, TValue>>.ReturnShared(buffer, RecyclableDictionary<TKey, TValue>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, RecyclableList<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }
            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;
            var source = items._memoryBlock;

            for (int i = 0; i < count; i++)
            {
                var kvp = source[i];
                TKey key = kvp.Key;
                TValue value = kvp.Value;
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, RecyclableLongList<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            long longCount = items._longCount;
            if (longCount == 0)
            {
                return;
            }

            if (longCount > int.MaxValue - dictionary._count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items exceeds {int.MaxValue}");
            }

            int count = (int)longCount;
            int blockSize = items._blockSize;
            int fullBlocks = (int)(longCount >> items._blockSizePow2BitShift);
            int lastBlockLength = (int)(longCount & items._blockSizeMinus1);

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int destIndex = dictionary._count;
            var blocks = items._memoryBlocks;

            for (int block = 0; block < fullBlocks; block++)
            {
                var source = blocks[block];
                for (int j = 0; j < blockSize; j++)
                {
                    var kvp = source[j];
                    TKey key = kvp.Key;
                    TValue value = kvp.Value;
                    int hash = key.GetHashCode() & int.MaxValue;
                    int bucket = hash & bucketMask;

                    int index = dictionary._buckets[bucket];
                    while (index >= 0)
                    {
                        ref var check = ref GetEntry(dictionary, index);
                        if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                        {
                            throw new ArgumentException("An element with the same key already exists.", nameof(items));
                        }

                        index = check.Next;
                    }

                    ref var entry = ref GetEntry(dictionary, destIndex);
                    entry.HashCode = hash;
                    entry.Key = key;
                    entry.Value = value;
                    entry.Next = dictionary._buckets[bucket];
                    dictionary._buckets[bucket] = destIndex++;
                }
            }

            if (lastBlockLength > 0)
            {
                var source = blocks[fullBlocks];
                for (int j = 0; j < lastBlockLength; j++)
                {
                    var kvp = source[j];
                    TKey key = kvp.Key;
                    TValue value = kvp.Value;
                    int hash = key.GetHashCode() & int.MaxValue;
                    int bucket = hash & bucketMask;

                    int index = dictionary._buckets[bucket];
                    while (index >= 0)
                    {
                        ref var check = ref GetEntry(dictionary, index);
                        if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                        {
                            throw new ArgumentException("An element with the same key already exists.", nameof(items));
                        }

                        index = check.Next;
                    }

                    ref var entry = ref GetEntry(dictionary, destIndex);
                    entry.HashCode = hash;
                    entry.Key = key;
                    entry.Value = value;
                    entry.Next = dictionary._buckets[bucket];
                    dictionary._buckets[bucket] = destIndex++;
                }
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, RecyclableSortedDictionary<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;

            for (int i = 0; i < count; i++)
            {
                TKey key = items.GetKey(i);
                TValue value = items.GetValue(i);
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, RecyclableSortedList<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;
            TKey[] keys = items._keys;
            TValue[] values = items._values;

            for (int i = 0; i < count; i++)
            {
                TKey key = keys[i];
                TValue value = values[i];
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, RecyclableQueue<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            if (longCount > int.MaxValue - dictionary._count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items exceeds {int.MaxValue}");
            }

            int count = (int)longCount;
            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int destIndex = dictionary._count;

            var chunk = items._head;
            while (chunk != null)
            {
                int sourceIndex = chunk.Bottom;
                int sourceTop = chunk.Top;
                var source = chunk.Value;

                while (sourceIndex < sourceTop)
                {
                    var kvp = source[sourceIndex++];
                    TKey key = kvp.Key;
                    TValue value = kvp.Value;
                    int hash = key.GetHashCode() & int.MaxValue;
                    int bucket = hash & bucketMask;

                    int index = dictionary._buckets[bucket];
                    while (index >= 0)
                    {
                        ref var check = ref GetEntry(dictionary, index);
                        if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                        {
                            throw new ArgumentException("An element with the same key already exists.", nameof(items));
                        }

                        index = check.Next;
                    }

                    ref var entry = ref GetEntry(dictionary, destIndex);
                    entry.HashCode = hash;
                    entry.Key = key;
                    entry.Value = value;
                    entry.Next = dictionary._buckets[bucket];
                    dictionary._buckets[bucket] = destIndex++;
                }

                chunk = chunk.Next;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, RecyclableStack<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            if (longCount > int.MaxValue - dictionary._count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items exceeds {int.MaxValue}");
            }

            int count = (int)longCount;
            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int destIndex = dictionary._count;

            var sourceChunk = items._current;
            while (sourceChunk.Previous != null)
            {
                sourceChunk = sourceChunk.Previous;
            }

            while (sourceChunk != null)
            {
                var source = sourceChunk.Value;
                int sourceCount = sourceChunk.Index;
                int sourceIndex = 0;

                while (sourceIndex < sourceCount)
                {
                    var kvp = source[sourceIndex++];
                    TKey key = kvp.Key;
                    TValue value = kvp.Value;
                    int hash = key.GetHashCode() & int.MaxValue;
                    int bucket = hash & bucketMask;

                    int index = dictionary._buckets[bucket];
                    while (index >= 0)
                    {
                        ref var check = ref GetEntry(dictionary, index);
                        if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                        {
                            throw new ArgumentException("An element with the same key already exists.", nameof(items));
                        }

                        index = check.Next;
                    }

                    ref var entry = ref GetEntry(dictionary, destIndex);
                    entry.HashCode = hash;
                    entry.Key = key;
                    entry.Value = value;
                    entry.Next = dictionary._buckets[bucket];
                    dictionary._buckets[bucket] = destIndex++;
                }

                sourceChunk = sourceChunk.Next;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, RecyclableLinkedList<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            if (longCount > int.MaxValue - dictionary._count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items exceeds {int.MaxValue}");
            }

            int count = (int)longCount;
            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int destIndex = dictionary._count;

            var chunk = items._head;
            while (chunk != null)
            {
                int sourceIndex = chunk.Bottom;
                int sourceTop = chunk.Top;
                var source = chunk.Value;

                while (sourceIndex < sourceTop)
                {
                    var kvp = source[sourceIndex++];
                    TKey key = kvp.Key;
                    TValue value = kvp.Value;
                    int hash = key.GetHashCode() & int.MaxValue;
                    int bucket = hash & bucketMask;

                    int index = dictionary._buckets[bucket];
                    while (index >= 0)
                    {
                        ref var check = ref GetEntry(dictionary, index);
                        if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                        {
                            throw new ArgumentException("An element with the same key already exists.", nameof(items));
                        }

                        index = check.Next;
                    }

                    ref var entry = ref GetEntry(dictionary, destIndex);
                    entry.HashCode = hash;
                    entry.Key = key;
                    entry.Value = value;
                    entry.Next = dictionary._buckets[bucket];
                    dictionary._buckets[bucket] = destIndex++;
                }

                chunk = chunk.Next;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, RecyclablePriorityQueue<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            int count = items._size;
            if (count == 0)
            {
                return;
            }

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;
            var source = items._heap;

            for (int i = 0; i < count; i++)
            {
                var kvp = source[i];
                TKey key = kvp.Key;
                TValue value = kvp.Value;
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, IReadOnlyList<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            int requiredCount = dictionary._count + count;
            EnsureCapacity(dictionary, requiredCount);

            int bucketsLength = dictionary._buckets.Length;
            while (requiredCount >= (bucketsLength * 3) / 4)
            {
                bucketsLength <<= 1;
            }

            if (bucketsLength != dictionary._buckets.Length)
            {
                ResizeBuckets(dictionary, bucketsLength);
            }

            int bucketMask = dictionary._buckets.Length - 1;
            int startIndex = dictionary._count;

            for (int i = 0; i < count; i++)
            {
                var kvp = items[i];
                TKey key = kvp.Key;
                TValue value = kvp.Value;
                int hash = key.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = dictionary._buckets[bucket];
                while (index >= 0)
                {
                    ref var check = ref GetEntry(dictionary, index);
                    if (check.HashCode == hash && EqualityComparer<TKey>.Default.Equals(check.Key, key))
                    {
                        throw new ArgumentException("An element with the same key already exists.", nameof(items));
                    }

                    index = check.Next;
                }

                ref var entry = ref GetEntry(dictionary, startIndex + i);
                entry.HashCode = hash;
                entry.Key = key;
                entry.Value = value;
                entry.Next = dictionary._buckets[bucket];
                dictionary._buckets[bucket] = startIndex + i;
            }

            dictionary._count = requiredCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            if (items is RecyclableDictionary<TKey, TValue> recyclableDictionary)
            {
                AddRange(dictionary, recyclableDictionary);
            }
            else if (items is RecyclableList<KeyValuePair<TKey, TValue>> recyclableList)
            {
                AddRange(dictionary, recyclableList);
            }
            else if (items is RecyclableLongList<KeyValuePair<TKey, TValue>> recyclableLongList)
            {
                AddRange(dictionary, recyclableLongList);
            }
            else if (items is KeyValuePair<TKey, TValue>[] array)
            {
                AddRange(dictionary, array);
            }
            else if (items is List<KeyValuePair<TKey, TValue>> list)
            {
                AddRange(dictionary, list);
            }
            else if (items is ICollection<KeyValuePair<TKey, TValue>> iCollection)
            {
                AddRange(dictionary, iCollection);
            }
            else if (items is ICollection collection)
            {
                AddRange(dictionary, collection);
            }
            else if (items is IReadOnlyList<KeyValuePair<TKey, TValue>> readOnlyList)
            {
                AddRange(dictionary, readOnlyList);
            }
            else
            {
                foreach (var kvp in items)
                {
                    dictionary.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
