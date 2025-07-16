using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal static class zRecyclableStackAddRange
    {
        private static class AddRangeHelper<T>
        {
            private static readonly Action<RecyclableStack<T>, object>? _dictionaryAdder;
            private static readonly Action<RecyclableStack<T>, object>? _sortedListAdder;
            private static readonly Type? _dictionaryType;
            private static readonly Type? _sortedListType;

            static AddRangeHelper()
            {
                Type elementType = typeof(T);
                if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    Type[] args = elementType.GetGenericArguments();
                    _dictionaryType = typeof(RecyclableDictionary<,>).MakeGenericType(args);
                    MethodInfo? method = typeof(zRecyclableStackAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclableStack<>).MakeGenericType(elementType), _dictionaryType },
                        modifiers: null);
                    if (method != null)
                    {
                        var stackParam = Expression.Parameter(typeof(RecyclableStack<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, stackParam, Expression.Convert(objParam, _dictionaryType));
                        _dictionaryAdder = Expression.Lambda<Action<RecyclableStack<T>, object>>(call, stackParam, objParam).Compile();
                    }
                }
                else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                {
                    Type[] args = elementType.GetGenericArguments();
                    _sortedListType = typeof(RecyclableSortedList<,>).MakeGenericType(args);
                    MethodInfo? method = typeof(zRecyclableStackAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclableStack<>).MakeGenericType(elementType), _sortedListType },
                        modifiers: null);
                    if (method != null)
                    {
                        var stackParam = Expression.Parameter(typeof(RecyclableStack<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, stackParam, Expression.Convert(objParam, _sortedListType));
                        _sortedListAdder = Expression.Lambda<Action<RecyclableStack<T>, object>>(call, stackParam, objParam).Compile();
                    }
                }
            }

            internal static bool TryAddRange(RecyclableStack<T> stack, IEnumerable<T> items)
            {
                if (_dictionaryAdder != null && _dictionaryType!.IsInstanceOfType(items))
                {
                    _dictionaryAdder(stack, items);
                    return true;
                }

                if (_sortedListAdder != null && _sortedListType!.IsInstanceOfType(items))
                {
                    _sortedListAdder(stack, items);
                    return true;
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, ReadOnlySpan<T> items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + count);

            RecyclableArrayPoolChunk<T> chunk = stack._current;
            while (chunk.Previous != null && chunk.Previous.Index < chunk.Previous.Value.Length)
            {
                chunk = chunk.Previous;
            }

            int index = chunk.Index;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                int toCopy = Math.Min(chunkLength - index, count - copied);
                items.Slice(copied, toCopy).CopyTo(new Span<T>(chunk.Value, index, toCopy));
                copied += toCopy;
                index += toCopy;

                if (index == chunkLength && copied < count)
                {
                    chunk.Index = index;
                    chunk = chunk.Next!;
                    index = 0;
                    chunkLength = chunk.Value.Length;
                }
            }

            chunk.Index = index;
            stack._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, Span<T> items) => AddRange(stack, (ReadOnlySpan<T>)items);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, T[] items) => AddRange(stack, (ReadOnlySpan<T>)items);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, in Array items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + count);

            RecyclableArrayPoolChunk<T> chunk = stack._current;
            while (chunk.Previous != null && chunk.Previous.Index < chunk.Previous.Value.Length)
            {
                chunk = chunk.Previous;
            }

            int index = chunk.Index;
            int chunkLength = chunk.Value.Length;
            int copied = 0;
            int lower = items.GetLowerBound(0);

            while (copied < count)
            {
                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(items, lower + copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;

                if (index == chunkLength && copied < count)
                {
                    chunk.Index = index;
                    chunk = chunk.Next!;
                    index = 0;
                    chunkLength = chunk.Value.Length;
                }
            }

            chunk.Index = index;
            stack._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, List<T> items) => AddRange(stack, CollectionsMarshal.AsSpan(items));

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, ICollection items)
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

            EnsureCapacity(stack, stack._count + count);

            RecyclableArrayPoolChunk<T> chunk = stack._current;
            while (chunk.Previous != null && chunk.Previous.Index < chunk.Previous.Value.Length)
            {
                chunk = chunk.Previous;
            }

            int index = chunk.Index;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(buffer, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;

                if (index == chunkLength && copied < count)
                {
                    chunk.Index = index;
                    chunk = chunk.Next!;
                    index = 0;
                    chunkLength = chunk.Value.Length;
                }
            }

            chunk.Index = index;
            stack._count += count;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableStack<T>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, ICollection<T> items)
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

            EnsureCapacity(stack, stack._count + count);

            RecyclableArrayPoolChunk<T> chunk = stack._current;
            while (chunk.Previous != null && chunk.Previous.Index < chunk.Previous.Value.Length)
            {
                chunk = chunk.Previous;
            }

            int index = chunk.Index;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(buffer, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;

                if (index == chunkLength && copied < count)
                {
                    chunk.Index = index;
                    chunk = chunk.Next!;
                    index = 0;
                    chunkLength = chunk.Value.Length;
                }
            }

            chunk.Index = index;
            stack._count += count;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableStack<T>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableList<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + count);

            RecyclableArrayPoolChunk<T> chunk = stack._current;
            while (chunk.Previous != null && chunk.Previous.Index < chunk.Previous.Value.Length)
            {
                chunk = chunk.Previous;
            }

            int index = chunk.Index;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(items._memoryBlock, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;

                if (index == chunkLength && copied < count)
                {
                    chunk.Index = index;
                    chunk = chunk.Next!;
                    index = 0;
                    chunkLength = chunk.Value.Length;
                }
            }

            chunk.Index = index;
            stack._count += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Grow<T>(RecyclableStack<T> stack)
        {
            int newSize;
            if (stack._capacity >= RecyclableDefaults.MaxPooledBlockSize)
            {
                newSize = RecyclableDefaults.MaxPooledBlockSize;
            }
            else
            {
                long doubled = stack._capacity << 1;
                newSize = (int)Math.Min(doubled - stack._capacity, RecyclableDefaults.MaxPooledBlockSize);
            }

            var newChunk = RecyclableArrayPoolChunkPool<T>.Rent();
            if (newChunk.Value.Length < newSize)
            {
                if (newChunk.Value.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<T>.ReturnShared(newChunk.Value, RecyclableStack<T>._needsClearing);
                }

                newChunk.Value = newSize >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(newSize)
                    : new T[newSize];
            }

            newChunk.Index = 0;
            newChunk.Previous = stack._current;
            newChunk.Next = null;
            stack._current.Next = newChunk;
            stack._current = newChunk;
            stack._capacity += newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Grow<T>(RecyclableStack<T> stack, long additionalCapacity)
        {
            int newSize = additionalCapacity >= RecyclableDefaults.MaxPooledBlockSize
                ? RecyclableDefaults.MaxPooledBlockSize
                : (int)Math.Min(BitOperations.RoundUpToPowerOf2((uint)additionalCapacity), RecyclableDefaults.MaxPooledBlockSize);

            var newChunk = RecyclableArrayPoolChunkPool<T>.Rent();
            if (newChunk.Value.Length < newSize)
            {
                if (newChunk.Value.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<T>.ReturnShared(newChunk.Value, RecyclableStack<T>._needsClearing);
                }

                newChunk.Value = newSize >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(newSize)
                    : new T[newSize];
            }

            newChunk.Index = 0;
            newChunk.Previous = stack._current;
            newChunk.Next = null;
            stack._current.Next = newChunk;
            stack._current = newChunk;
            stack._capacity += newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCapacity<T>(RecyclableStack<T> stack, long requiredCapacity)
        {
            if (requiredCapacity <= stack._capacity)
            {
                return;
            }

            long needed = requiredCapacity - stack._capacity;

            do
            {
                int size = needed >= RecyclableDefaults.MaxPooledBlockSize
                    ? RecyclableDefaults.MaxPooledBlockSize
                    : (int)BitOperations.RoundUpToPowerOf2((uint)needed);

                var newChunk = RecyclableArrayPoolChunkPool<T>.Rent();
                if (newChunk.Value.Length < size)
                {
                    if (newChunk.Value.Length >= RecyclableDefaults.MinPooledArrayLength)
                    {
                        RecyclableArrayPool<T>.ReturnShared(newChunk.Value, RecyclableStack<T>._needsClearing);
                    }

                    newChunk.Value = size >= RecyclableDefaults.MinPooledArrayLength
                        ? RecyclableArrayPool<T>.RentShared(size)
                        : new T[size];
                }

                newChunk.Index = 0;
                newChunk.Previous = stack._current;
                newChunk.Next = null;
                stack._current.Next = newChunk;
                stack._current = newChunk;
                stack._capacity += size;

                needed -= size;
            }
            while (needed > 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableLongList<T> items)
        {
            long longCount = items._longCount;
            if (longCount == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + longCount);

            int blockSize = items._blockSize;
            int fullBlocks = (int)(longCount >> items._blockSizePow2BitShift);
            int lastBlockLength = (int)(longCount & items._blockSizeMinus1);

            RecyclableArrayPoolChunk<T> chunk = stack._current;
            while (chunk.Previous != null && chunk.Previous.Index < chunk.Previous.Value.Length)
            {
                chunk = chunk.Previous;
            }

            int index = chunk.Index;
            int chunkLength = chunk.Value.Length;

            for (int blockIndex = 0; blockIndex < fullBlocks; blockIndex++)
            {
                T[] block = items._memoryBlocks[blockIndex];
                int srcIndex = 0;

                while (srcIndex < blockSize)
                {
                    int toCopy = Math.Min(chunkLength - index, blockSize - srcIndex);
                    Array.Copy(block, srcIndex, chunk.Value, index, toCopy);
                    srcIndex += toCopy;
                    index += toCopy;

                    if (index == chunkLength && (srcIndex < blockSize || blockIndex < fullBlocks - 1 || lastBlockLength > 0))
                    {
                        chunk.Index = index;
                        chunk = chunk.Next!;
                        index = 0;
                        chunkLength = chunk.Value.Length;
                    }
                }
            }

            if (lastBlockLength > 0)
            {
                T[] block = items._memoryBlocks[fullBlocks];
                int srcIndex = 0;

                while (srcIndex < lastBlockLength)
                {
                    int toCopy = Math.Min(chunkLength - index, lastBlockLength - srcIndex);
                    Array.Copy(block, srcIndex, chunk.Value, index, toCopy);
                    srcIndex += toCopy;
                    index += toCopy;

                    if (index == chunkLength && srcIndex < lastBlockLength)
                    {
                        chunk.Index = index;
                        chunk = chunk.Next!;
                        index = 0;
                        chunkLength = chunk.Value.Length;
                    }
                }
            }

            chunk.Index = index;
            stack._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableHashSet<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + count);

            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;

            RecyclableArrayPoolChunk<T> chunk = stack._current;
            while (chunk.Previous != null && chunk.Previous.Index < chunk.Previous.Value.Length)
            {
                chunk = chunk.Previous;
            }

            int index = chunk.Index;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                int toCopy = Math.Min(chunkLength - index, count - copied);
                var destination = chunk.Value;
                for (int i = 0; i < toCopy; i++)
                {
                    destination[index + i] = entries[(copied + i) >> shift][(copied + i) & mask].Value;
                }

                copied += toCopy;
                index += toCopy;

                if (index == chunkLength && copied < count)
                {
                    chunk.Index = index;
                    chunk = chunk.Next!;
                    index = 0;
                    chunkLength = chunk.Value.Length;
                }
            }

            chunk.Index = index;
            stack._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableStack<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + longCount);

            RecyclableArrayPoolChunk<T> destChunk = stack._current;
            while (destChunk.Previous != null && destChunk.Previous.Index < destChunk.Previous.Value.Length)
            {
                destChunk = destChunk.Previous;
            }

            int destIndex = destChunk.Index;
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
                    int toCopy = Math.Min(destLength - destIndex, sourceCount - sourceIndex);
                    Array.Copy(source, sourceIndex, destChunk.Value, destIndex, toCopy);
                    sourceIndex += toCopy;
                    destIndex += toCopy;

                    if (destIndex == destLength && (sourceIndex < sourceCount || sourceChunk.Next != null))
                    {
                        destChunk.Index = destIndex;
                        destChunk = destChunk.Next!;
                        destIndex = 0;
                        destLength = destChunk.Value.Length;
                    }
                }

                sourceChunk = sourceChunk.Next!;
            }

            destChunk.Index = destIndex;
            stack._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableSortedSet<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + count);

            RecyclableArrayPoolChunk<T> chunk = stack._current;
            while (chunk.Previous != null && chunk.Previous.Index < chunk.Previous.Value.Length)
            {
                chunk = chunk.Previous;
            }

            int index = chunk.Index;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(items._items, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;

                if (index == chunkLength && copied < count)
                {
                    chunk.Index = index;
                    chunk = chunk.Next!;
                    index = 0;
                    chunkLength = chunk.Value.Length;
                }
            }

            chunk.Index = index;
            stack._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableLinkedList<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + longCount);

            RecyclableArrayPoolChunk<T> destChunk = stack._current;
            while (destChunk.Previous != null && destChunk.Previous.Index < destChunk.Previous.Value.Length)
            {
                destChunk = destChunk.Previous;
            }

            int destIndex = destChunk.Index;
            int destLength = destChunk.Value.Length;

            var sourceChunk = items._head;
            while (sourceChunk != null)
            {
                int sourceCount = sourceChunk.Top - sourceChunk.Bottom;
                int sourceIndex = sourceChunk.Bottom;

                while (sourceCount > 0)
                {
                    int toCopy = Math.Min(destLength - destIndex, sourceCount);
                    Array.Copy(sourceChunk.Value, sourceIndex, destChunk.Value, destIndex, toCopy);
                    destIndex += toCopy;
                    sourceIndex += toCopy;
                    sourceCount -= toCopy;

                    if (destIndex == destLength && (sourceCount > 0 || sourceChunk.Next != null))
                    {
                        destChunk.Index = destIndex;
                        destChunk = destChunk.Next!;
                        destIndex = 0;
                        destLength = destChunk.Value.Length;
                    }
                }

                sourceChunk = sourceChunk.Next;
            }

            destChunk.Index = destIndex;
            stack._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclablePriorityQueue<T> items)
        {
            AddRange(stack, new ReadOnlySpan<T>(items._heap, 0, items._size));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableQueue<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + longCount);

            RecyclableArrayPoolChunk<T> destChunk = stack._current;
            while (destChunk.Previous != null && destChunk.Previous.Index < destChunk.Previous.Value.Length)
            {
                destChunk = destChunk.Previous;
            }

            int destIndex = destChunk.Index;
            int destLength = destChunk.Value.Length;

            var sourceChunk = items._head;
            while (sourceChunk != null)
            {
                int sourceCount = sourceChunk.Top - sourceChunk.Bottom;
                int sourceIndex = sourceChunk.Bottom;

                while (sourceCount > 0)
                {
                    int toCopy = Math.Min(destLength - destIndex, sourceCount);
                    Array.Copy(sourceChunk.Value, sourceIndex, destChunk.Value, destIndex, toCopy);
                    destIndex += toCopy;
                    sourceIndex += toCopy;
                    sourceCount -= toCopy;

                    if (destIndex == destLength && (sourceCount > 0 || sourceChunk.Next != null))
                    {
                        destChunk.Index = destIndex;
                        destChunk = destChunk.Next!;
                        destIndex = 0;
                        destLength = destChunk.Value.Length;
                    }
                }

                sourceChunk = sourceChunk.Next;
            }

            destChunk.Index = destIndex;
            stack._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableStack<KeyValuePair<TKey, TValue>> stack, RecyclableDictionary<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(stack, stack._count + count);

            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;

            RecyclableArrayPoolChunk<KeyValuePair<TKey, TValue>> chunk = stack._current;
            while (chunk.Previous != null && chunk.Previous.Index < chunk.Previous.Value.Length)
            {
                chunk = chunk.Previous;
            }

            int index = chunk.Index;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
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

                if (index == chunkLength && copied < count)
                {
                    chunk.Index = index;
                    chunk = chunk.Next!;
                    index = 0;
                    chunkLength = chunk.Value.Length;
                }
            }

            chunk.Index = index;
            stack._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableStack<(TKey Key, TValue Value)> stack, RecyclableSortedList<TKey, TValue> items)
            where TKey : notnull
        {
            for (int i = 0; i < items._count; i++)
            {
                stack.Push((items._keys[i], items._values[i]));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, IReadOnlyList<T> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                stack.Push(items[i]);
            }
        }

        internal static IEnumerator AddRange<T>(this RecyclableStack<T> stack, IEnumerable source)
        {
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                stack.Push((T)enumerator.Current!);
            }
            return enumerator;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, IEnumerable<T> items)
        {
            if (items is RecyclableList<T> recyclableList)
            {
                AddRange(stack, recyclableList);
            }
            else if (items is RecyclableLongList<T> recyclableLongList)
            {
                AddRange(stack, recyclableLongList);
            }
            else if (items is T[] array)
            {
                AddRange(stack, array);
            }
            else if (items is List<T> list)
            {
                AddRange(stack, list);
            }
            else if (items is ICollection<T> iCollection)
            {
                AddRange(stack, iCollection);
            }
            else if (items is ICollection collection)
            {
                AddRange(stack, collection);
            }
            else if (items is RecyclableHashSet<T> hashSet)
            {
                AddRange(stack, hashSet);
            }
            else if (items is RecyclableStack<T> stackSource)
            {
                AddRange(stack, stackSource);
            }
            else if (items is RecyclableSortedSet<T> sortedSet)
            {
                AddRange(stack, sortedSet);
            }
            else if (items is RecyclableLinkedList<T> linkedList)
            {
                AddRange(stack, linkedList);
            }
            else if (items is RecyclablePriorityQueue<T> priorityQueue)
            {
                AddRange(stack, priorityQueue);
            }
            else if (items is RecyclableQueue<T> queue)
            {
                AddRange(stack, queue);
            }
            else if (AddRangeHelper<T>.TryAddRange(stack, items))
            {
            }
            else if (items is IReadOnlyList<T> readOnlyList)
            {
                AddRange(stack, readOnlyList);
            }
            else
            {
                using IEnumerator<T> enumerator = items.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    stack.Push(enumerator.Current);
                }
            }
        }
    }
}

