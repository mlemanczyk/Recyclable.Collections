using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal static class zRecyclableHashSetAddRange
    {
        private static class AddRangeHelper<T>
        {
            private static readonly Action<RecyclableHashSet<T>, object>? _dictionaryAdder;
            private static readonly Action<RecyclableHashSet<T>, object>? _sortedListAdder;
            private static readonly Action<RecyclableHashSet<T>, object>? _sortedDictionaryAdder;
            private static readonly Type? _dictionaryType;
            private static readonly Type? _sortedListType;
            private static readonly Type? _sortedDictionaryType;

            static AddRangeHelper()
            {
                Type elementType = typeof(T);
                if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    Type[] args = elementType.GetGenericArguments();
                    _dictionaryType = typeof(RecyclableDictionary<,>).MakeGenericType(args);
                    _sortedDictionaryType = typeof(RecyclableSortedDictionary<,>).MakeGenericType(args);

                    MethodInfo? method = typeof(zRecyclableHashSetAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclableHashSet<>).MakeGenericType(elementType), _dictionaryType },
                        modifiers: null);
                    if (method != null)
                    {
                        var setParam = Expression.Parameter(typeof(RecyclableHashSet<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, setParam, Expression.Convert(objParam, _dictionaryType));
                        _dictionaryAdder = Expression.Lambda<Action<RecyclableHashSet<T>, object>>(call, setParam, objParam).Compile();
                    }

                    method = typeof(zRecyclableHashSetAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclableHashSet<>).MakeGenericType(elementType), _sortedDictionaryType },
                        modifiers: null);
                    if (method != null)
                    {
                        var setParam = Expression.Parameter(typeof(RecyclableHashSet<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, setParam, Expression.Convert(objParam, _sortedDictionaryType));
                        _sortedDictionaryAdder = Expression.Lambda<Action<RecyclableHashSet<T>, object>>(call, setParam, objParam).Compile();
                    }
                }
                else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                {
                    Type[] args = elementType.GetGenericArguments();
                    _sortedListType = typeof(RecyclableSortedList<,>).MakeGenericType(args);
                    MethodInfo? method = typeof(zRecyclableHashSetAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclableHashSet<>).MakeGenericType(elementType), _sortedListType },
                        modifiers: null);
                    if (method != null)
                    {
                        var setParam = Expression.Parameter(typeof(RecyclableHashSet<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, setParam, Expression.Convert(objParam, _sortedListType));
                        _sortedListAdder = Expression.Lambda<Action<RecyclableHashSet<T>, object>>(call, setParam, objParam).Compile();
                    }
                }
            }

            internal static bool TryAddRange(RecyclableHashSet<T> set, IEnumerable<T> items)
            {
                if (_dictionaryAdder != null && _dictionaryType!.IsInstanceOfType(items))
                {
                    _dictionaryAdder(set, items);
                    return true;
                }

                if (_sortedDictionaryAdder != null && _sortedDictionaryType!.IsInstanceOfType(items))
                {
                    _sortedDictionaryAdder(set, items);
                    return true;
                }

                if (_sortedListAdder != null && _sortedListType!.IsInstanceOfType(items))
                {
                    _sortedListAdder(set, items);
                    return true;
                }

                return false;
            }
        }

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
        internal static void AddRange<T>(this RecyclableHashSet<T> set, in Array items)
            where T : notnull
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            T[] buffer;
            int lower = items.GetLowerBound(0);

            if (items is T[] array && lower == 0)
            {
                buffer = array;
            }
            else
            {
                buffer = count >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)count)))
                    : new T[count];

                Array.Copy(items, lower, buffer, 0, count);
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
                T value = buffer[i];
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

            if (!ReferenceEquals(buffer, items) && count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableHashSet<T>._needsClearing);
            }
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
        internal static void AddRange<T>(this RecyclableHashSet<T> set, ICollection items)
            where T : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            T[] buffer = count >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)count)))
                : new T[count];

            items.CopyTo(buffer, 0);

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
                T value = buffer[i];
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

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableHashSet<T>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableHashSet<T> set, ICollection<T> items)
            where T : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            T[] buffer = count >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)count)))
                : new T[count];

            items.CopyTo(buffer, 0);

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
                T value = buffer[i];
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

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableHashSet<T>._needsClearing);
            }
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
        internal static void AddRange<T>(this RecyclableHashSet<T> set, RecyclableSortedSet<T> items)
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
            T[] source = items._items;

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
        internal static void AddRange<T>(this RecyclableHashSet<T> set, RecyclableLinkedList<T> items)
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
                int index = chunk.Bottom;
                int top = chunk.Top;
                var source = chunk.Value;

                while (index < top)
                {
                    T value = source[index++];
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
        internal static void AddRange<T>(this RecyclableHashSet<T> set, RecyclablePriorityQueue<T> items)
            where T : notnull
        {
            int count = items._size;
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
            T[] source = items._heap;

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
        internal static void AddRange<TKey, TValue>(this RecyclableHashSet<KeyValuePair<TKey, TValue>> set, RecyclableDictionary<TKey, TValue> items)
            where TKey : notnull
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
                ref var src = ref entries[i >> shift][i & mask];
                var pair = new KeyValuePair<TKey, TValue>(src.Key, src.Value);
                int hash = pair.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<KeyValuePair<TKey, TValue>>.Default.Equals(check.Value, pair))
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
                entry.Value = pair;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableHashSet<KeyValuePair<TKey, TValue>> set, RecyclableSortedDictionary<TKey, TValue> items)
            where TKey : notnull
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
                var pair = new KeyValuePair<TKey, TValue>(items.GetKey(i), items.GetValue(i));
                int hash = pair.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<KeyValuePair<TKey, TValue>>.Default.Equals(check.Value, pair))
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
                entry.Value = pair;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
            }

            set._count = insertIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableHashSet<(TKey Key, TValue Value)> set, RecyclableSortedList<TKey, TValue> items)
            where TKey : notnull
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
            TKey[] keys = items._keys;
            TValue[] values = items._values;

            for (int i = 0; i < count; i++)
            {
                var pair = (keys[i], values[i]);
                int hash = pair.GetHashCode() & int.MaxValue;
                int bucket = hash & bucketMask;

                int index = set._buckets[bucket];
                bool exists = false;
                while (index >= 0)
                {
                    ref var check = ref GetEntry(set, index);
                    if (check.HashCode == hash && EqualityComparer<(TKey Key, TValue Value)>.Default.Equals(check.Value, pair))
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
                entry.Value = pair;
                entry.Next = set._buckets[bucket];
                set._buckets[bucket] = insertIndex;
                insertIndex++;
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
            if (items is RecyclableList<T> recyclableList)
            {
                AddRange(set, recyclableList);
            }
            else if (items is RecyclableLongList<T> recyclableLongList)
            {
                AddRange(set, recyclableLongList);
            }
            else if (items is T[] array)
            {
                AddRange(set, array);
            }
            else if (items is List<T> list)
            {
                AddRange(set, list);
            }
            else if (items is ICollection<T> iCollection)
            {
                AddRange(set, iCollection);
            }
            else if (items is ICollection collection)
            {
                AddRange(set, collection);
            }
            else if (items is RecyclableHashSet<T> hashSet)
            {
                AddRange(set, hashSet);
            }
            else if (items is RecyclableStack<T> stack)
            {
                AddRange(set, stack);
            }
            else if (items is RecyclableSortedSet<T> sortedSet)
            {
                AddRange(set, sortedSet);
            }
            else if (items is RecyclableLinkedList<T> linkedList)
            {
                AddRange(set, linkedList);
            }
            else if (items is RecyclablePriorityQueue<T> priorityQueue)
            {
                AddRange(set, priorityQueue);
            }
            else if (items is RecyclableQueue<T> queue)
            {
                AddRange(set, queue);
            }
            else if (AddRangeHelper<T>.TryAddRange(set, items))
            {
            }
            else if (items is IReadOnlyList<T> readOnlyList)
            {
                AddRange(set, readOnlyList);
            }
            else
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
}
