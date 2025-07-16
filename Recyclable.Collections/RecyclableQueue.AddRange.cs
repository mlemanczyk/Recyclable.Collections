using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal static class zRecyclableQueueAddRange
    {
        private static class AddRangeHelper<T>
        {
            private static readonly Action<RecyclableQueue<T>, object>? _dictionaryAdder;
            private static readonly Action<RecyclableQueue<T>, object>? _sortedListAdder;
            private static readonly Type? _dictionaryType;
            private static readonly Type? _sortedListType;

            static AddRangeHelper()
            {
                Type elementType = typeof(T);
                if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    Type[] args = elementType.GetGenericArguments();
                    _dictionaryType = typeof(RecyclableDictionary<,>).MakeGenericType(args);
                    MethodInfo? method = typeof(zRecyclableQueueAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclableQueue<>).MakeGenericType(elementType), _dictionaryType },
                        modifiers: null);
                    if (method != null)
                    {
                        var queueParam = Expression.Parameter(typeof(RecyclableQueue<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, queueParam, Expression.Convert(objParam, _dictionaryType));
                        _dictionaryAdder = Expression.Lambda<Action<RecyclableQueue<T>, object>>(call, queueParam, objParam).Compile();
                    }
                }
                else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                {
                    Type[] args = elementType.GetGenericArguments();
                    _sortedListType = typeof(RecyclableSortedList<,>).MakeGenericType(args);
                    MethodInfo? method = typeof(zRecyclableQueueAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclableQueue<>).MakeGenericType(elementType), _sortedListType },
                        modifiers: null);
                    if (method != null)
                    {
                        var queueParam = Expression.Parameter(typeof(RecyclableQueue<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, queueParam, Expression.Convert(objParam, _sortedListType));
                        _sortedListAdder = Expression.Lambda<Action<RecyclableQueue<T>, object>>(call, queueParam, objParam).Compile();
                    }
                }
            }

            internal static bool TryAddRange(RecyclableQueue<T> queue, IEnumerable<T> items)
            {
                if (_dictionaryAdder != null && _dictionaryType!.IsInstanceOfType(items))
                {
                    _dictionaryAdder(queue, items);
                    return true;
                }

                if (_sortedListAdder != null && _sortedListType!.IsInstanceOfType(items))
                {
                    _sortedListAdder(queue, items);
                    return true;
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, ReadOnlySpan<T> items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(queue, queue._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                items.Slice(copied, toCopy).CopyTo(new Span<T>(chunk.Value, index, toCopy));
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, Span<T> items) => AddRange(queue, (ReadOnlySpan<T>)items);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, T[] items) => AddRange(queue, (ReadOnlySpan<T>)items);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, in Array items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(queue, queue._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;
            int lower = items.GetLowerBound(0);

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(items, lower + copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, List<T> items) => AddRange(queue, CollectionsMarshal.AsSpan(items));

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, ICollection items)
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

            EnsureCapacity(queue, queue._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(buffer, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableQueue<T>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, ICollection<T> items)
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

            EnsureCapacity(queue, queue._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(buffer, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableQueue<T>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, RecyclableList<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(queue, queue._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            T[] source = items._memoryBlock;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(source, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Grow<T>(RecyclableQueue<T> queue)
        {
            int newSize;
            if (queue._capacity >= RecyclableDefaults.MaxPooledBlockSize)
            {
                newSize = RecyclableDefaults.MaxPooledBlockSize;
            }
            else
            {
                long doubled = queue._capacity << 1;
                newSize = (int)Math.Min(doubled - queue._capacity, RecyclableDefaults.MaxPooledBlockSize);
            }

            var newChunk = BiDirectionalRecyclableArrayPoolChunkPool<T>.Rent();
            if (newChunk.Value.Length < newSize)
            {
                if (newChunk.Value.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<T>.ReturnShared(newChunk.Value, RecyclableQueue<T>._needsClearing);
                }

                newChunk.Value = newSize >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(newSize)
                    : new T[newSize];
            }

            newChunk.Top = 0;
            newChunk.Bottom = 0;
            newChunk.Previous = queue._tail;
            newChunk.Next = null;
            queue._tail.Next = newChunk;
            queue._tail = newChunk;
            queue._capacity += newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Grow<T>(RecyclableQueue<T> queue, long additionalCapacity)
        {
            int newSize = additionalCapacity >= RecyclableDefaults.MaxPooledBlockSize
                ? RecyclableDefaults.MaxPooledBlockSize
                : (int)Math.Min(BitOperations.RoundUpToPowerOf2((uint)additionalCapacity), RecyclableDefaults.MaxPooledBlockSize);

            var newChunk = BiDirectionalRecyclableArrayPoolChunkPool<T>.Rent();
            if (newChunk.Value.Length < newSize)
            {
                if (newChunk.Value.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<T>.ReturnShared(newChunk.Value, RecyclableQueue<T>._needsClearing);
                }

                newChunk.Value = newSize >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(newSize)
                    : new T[newSize];
            }

            newChunk.Top = 0;
            newChunk.Bottom = 0;
            newChunk.Previous = queue._tail;
            newChunk.Next = null;
            queue._tail.Next = newChunk;
            queue._tail = newChunk;
            queue._capacity += newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCapacity<T>(RecyclableQueue<T> queue, long requiredCapacity)
        {
            while (queue._capacity < requiredCapacity)
            {
                Grow(queue, requiredCapacity - queue._capacity);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, RecyclableLongList<T> items)
        {
            long longCount = items._longCount;
            if (longCount == 0)
            {
                return;
            }

            int blockSize = items._blockSize;
            int fullBlocks = (int)(longCount >> items._blockSizePow2BitShift);
            int lastBlockLength = (int)(longCount & items._blockSizeMinus1);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;

            for (int blockIndex = 0; blockIndex < fullBlocks; blockIndex++)
            {
                T[] block = items._memoryBlocks[blockIndex];
                int sourceIndex = 0;
                while (sourceIndex < blockSize)
                {
                    if (index == chunkLength)
                    {
                        chunk.Top = index;
                        Grow(queue);
                        chunk = queue._tail;
                        index = chunk.Top;
                        chunkLength = chunk.Value.Length;
                    }

                    int toCopy = Math.Min(chunkLength - index, blockSize - sourceIndex);
                    Array.Copy(block, sourceIndex, chunk.Value, index, toCopy);
                    sourceIndex += toCopy;
                    index += toCopy;
                }
            }

            if (lastBlockLength > 0)
            {
                T[] block = items._memoryBlocks[fullBlocks];
                int sourceIndex = 0;
                while (sourceIndex < lastBlockLength)
                {
                    if (index == chunkLength)
                    {
                        chunk.Top = index;
                        Grow(queue);
                        chunk = queue._tail;
                        index = chunk.Top;
                        chunkLength = chunk.Value.Length;
                    }

                    int toCopy = Math.Min(chunkLength - index, lastBlockLength - sourceIndex);
                    Array.Copy(block, sourceIndex, chunk.Value, index, toCopy);
                    sourceIndex += toCopy;
                    index += toCopy;
                }
            }

            chunk.Top = index;
            queue._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, RecyclableHashSet<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                var destination = chunk.Value;
                for (int i = 0; i < toCopy; i++)
                {
                    destination[index + i] = entries[(copied + i) >> shift][(copied + i) & mask].Value;
                }
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, RecyclableStack<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> destChunk = queue._tail;
            int destIndex = destChunk.Top;
            int destLength = destChunk.Value.Length;

            RecyclableArrayPoolChunk<T> sourceChunk = items._current;
            while (sourceChunk.Previous != null)
            {
                sourceChunk = sourceChunk.Previous;
            }

            while (sourceChunk != null)
            {
                T[] source = sourceChunk.Value;
                int sourceCount = sourceChunk.Index;
                int sourceIndex = 0;

                while (sourceIndex < sourceCount)
                {
                    if (destIndex == destLength)
                    {
                        destChunk.Top = destIndex;
                        Grow(queue);
                        destChunk = queue._tail;
                        destIndex = destChunk.Top;
                        destLength = destChunk.Value.Length;
                    }

                    int toCopy = Math.Min(destLength - destIndex, sourceCount - sourceIndex);
                    Array.Copy(source, sourceIndex, destChunk.Value, destIndex, toCopy);
                    sourceIndex += toCopy;
                    destIndex += toCopy;
                }

                sourceChunk = sourceChunk.Next!;
            }

            destChunk.Top = destIndex;
            queue._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, RecyclableSortedSet<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            T[] source = items._items;
            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(source, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, RecyclableLinkedList<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> destChunk = queue._tail;
            int destIndex = destChunk.Top;
            int destLength = destChunk.Value.Length;

            var sourceChunk = items._head;
            while (sourceChunk != null)
            {
                int sourceIndex = sourceChunk.Bottom;
                int sourceTop = sourceChunk.Top;
                T[] source = sourceChunk.Value;

                while (sourceIndex < sourceTop)
                {
                    if (destIndex == destLength)
                    {
                        destChunk.Top = destIndex;
                        Grow(queue);
                        destChunk = queue._tail;
                        destIndex = destChunk.Top;
                        destLength = destChunk.Value.Length;
                    }

                    int toCopy = Math.Min(destLength - destIndex, sourceTop - sourceIndex);
                    Array.Copy(source, sourceIndex, destChunk.Value, destIndex, toCopy);
                    sourceIndex += toCopy;
                    destIndex += toCopy;
                }

                sourceChunk = sourceChunk.Next;
            }

            destChunk.Top = destIndex;
            queue._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, RecyclablePriorityQueue<T> items)
        {
            int count = items._size;
            if (count == 0)
            {
                return;
            }

            T[] source = items._heap;
            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(source, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, RecyclableQueue<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> destChunk = queue._tail;
            int destIndex = destChunk.Top;
            int destLength = destChunk.Value.Length;

            EnsureCapacity(queue, queue._count + longCount);

            var sourceChunk = items._head;
            while (sourceChunk != null)
            {
                int sourceIndex = sourceChunk.Bottom;
                int sourceTop = sourceChunk.Top;
                T[] source = sourceChunk.Value;

                while (sourceIndex < sourceTop)
                {
                    if (destIndex == destLength)
                    {
                        destChunk.Top = destIndex;
                        destChunk = destChunk.Next!;
                        destIndex = destChunk.Top;
                        destLength = destChunk.Value.Length;
                    }

                    int toCopy = Math.Min(destLength - destIndex, sourceTop - sourceIndex);
                    Array.Copy(source, sourceIndex, destChunk.Value, destIndex, toCopy);
                    sourceIndex += toCopy;
                    destIndex += toCopy;
                }

                sourceChunk = sourceChunk.Next;
            }

            destChunk.Top = destIndex;
            queue._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableQueue<KeyValuePair<TKey, TValue>> queue, RecyclableDictionary<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(queue, queue._count + count);

            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;

            BiDirectionalRecyclableArrayPoolChunk<KeyValuePair<TKey, TValue>> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                var destination = chunk.Value;
                int local = copied;
                for (int i = 0; i < toCopy; i++)
                {
                    ref var entry = ref entries[(local + i) >> shift][(local + i) & mask];
                    destination[index + i] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                }

                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableQueue<(TKey Key, TValue Value)> queue, RecyclableSortedList<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(queue, queue._count + count);

            TKey[] keys = items._keys;
            TValue[] values = items._values;

            BiDirectionalRecyclableArrayPoolChunk<(TKey Key, TValue Value)> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                var destination = chunk.Value;
                for (int i = 0; i < toCopy; i++)
                {
                    destination[index + i] = (keys[copied + i], values[copied + i]);
                }

                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            queue._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, IReadOnlyList<T> items)
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(queue, queue._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;

            for (int i = 0; i < count; i++)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                chunk.Value[index++] = items[i];
            }

            chunk.Top = index;
            queue._count += count;
        }

        internal static IEnumerator AddRange<T>(this RecyclableQueue<T> queue, IEnumerable source)
        {
            IEnumerator enumerator = source.GetEnumerator();
            BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;

            while (enumerator.MoveNext())
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    Grow(queue);
                    chunk = queue._tail;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                chunk.Value[index++] = (T)enumerator.Current!;
                queue._count++;
            }

            chunk.Top = index;
            return enumerator;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableQueue<T> queue, IEnumerable<T> items)
        {
            if (items is RecyclableList<T> recyclableList)
            {
                AddRange(queue, recyclableList);
            }
            else if (items is RecyclableLongList<T> recyclableLongList)
            {
                AddRange(queue, recyclableLongList);
            }
            else if (items is T[] array)
            {
                AddRange(queue, array);
            }
            else if (items is List<T> list)
            {
                AddRange(queue, list);
            }
            else if (items is ICollection<T> iCollection)
            {
                AddRange(queue, iCollection);
            }
            else if (items is ICollection collection)
            {
                AddRange(queue, collection);
            }
            else if (items is RecyclableHashSet<T> hashSet)
            {
                AddRange(queue, hashSet);
            }
            else if (items is RecyclableStack<T> stack)
            {
                AddRange(queue, stack);
            }
            else if (items is RecyclableSortedSet<T> sortedSet)
            {
                AddRange(queue, sortedSet);
            }
            else if (items is RecyclableLinkedList<T> linkedList)
            {
                AddRange(queue, linkedList);
            }
            else if (items is RecyclablePriorityQueue<T> priorityQueue)
            {
                AddRange(queue, priorityQueue);
            }
            else if (items is RecyclableQueue<T> queueSource)
            {
                AddRange(queue, queueSource);
            }
            else if (AddRangeHelper<T>.TryAddRange(queue, items))
            {
            }
            else if (items is IReadOnlyList<T> readOnlyList)
            {
                AddRange(queue, readOnlyList);
            }
            else
            {
                using IEnumerator<T> enumerator = items.GetEnumerator();
                BiDirectionalRecyclableArrayPoolChunk<T> chunk = queue._tail;
                int index = chunk.Top;
                int chunkLength = chunk.Value.Length;

                while (enumerator.MoveNext())
                {
                    if (index == chunkLength)
                    {
                        chunk.Top = index;
                        Grow(queue);
                        chunk = queue._tail;
                        index = chunk.Top;
                        chunkLength = chunk.Value.Length;
                    }

                    chunk.Value[index++] = enumerator.Current;
                    queue._count++;
                }

                chunk.Top = index;
            }
        }
    }
}
