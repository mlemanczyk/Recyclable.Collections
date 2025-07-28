using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal static class zRecyclableLinkedListAddRange
    {
        private static class AddRangeHelper<T>
        {
            private static readonly Action<RecyclableLinkedList<T>, object>? _dictionaryAdder;
            private static readonly Action<RecyclableLinkedList<T>, object>? _sortedListAdder;
            private static readonly Type? _dictionaryType;
            private static readonly Type? _sortedListType;

            static AddRangeHelper()
            {
                Type elementType = typeof(T);
                if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    Type[] args = elementType.GetGenericArguments();
                    _dictionaryType = typeof(RecyclableDictionary<,>).MakeGenericType(args);
                    MethodInfo? method = typeof(zRecyclableLinkedListAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclableLinkedList<>).MakeGenericType(elementType), _dictionaryType },
                        modifiers: null);
                    if (method != null)
                    {
                        var listParam = Expression.Parameter(typeof(RecyclableLinkedList<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, listParam, Expression.Convert(objParam, _dictionaryType));
                        _dictionaryAdder = Expression.Lambda<Action<RecyclableLinkedList<T>, object>>(call, listParam, objParam).Compile();
                    }
                }
                else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                {
                    Type[] args = elementType.GetGenericArguments();
                    _sortedListType = typeof(RecyclableSortedList<,>).MakeGenericType(args);
                    MethodInfo? method = typeof(zRecyclableLinkedListAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclableLinkedList<>).MakeGenericType(elementType), _sortedListType },
                        modifiers: null);
                    if (method != null)
                    {
                        var listParam = Expression.Parameter(typeof(RecyclableLinkedList<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, listParam, Expression.Convert(objParam, _sortedListType));
                        _sortedListAdder = Expression.Lambda<Action<RecyclableLinkedList<T>, object>>(call, listParam, objParam).Compile();
                    }
                }
            }

            internal static bool TryAddRange(RecyclableLinkedList<T> list, IEnumerable<T> items)
            {
                if (_dictionaryAdder != null && _dictionaryType!.IsInstanceOfType(items))
                {
                    _dictionaryAdder(list, items);
                    return true;
                }

                if (_sortedListAdder != null && _sortedListType!.IsInstanceOfType(items))
                {
                    _sortedListAdder(list, items);
                    return true;
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, ReadOnlySpan<T> items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(list, list._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                items.Slice(copied, toCopy).CopyTo(new Span<T>(chunk.Value, index, toCopy));
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, Span<T> items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(list, list._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                items.Slice(copied, toCopy).CopyTo(new Span<T>(chunk.Value, index, toCopy));
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, T[] items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(list, list._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(items, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, in Array items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(list, list._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;
            int lower = items.GetLowerBound(0);

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(items, lower + copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, List<T> items)
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(list, list._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;
            Span<T> source = CollectionsMarshal.AsSpan(items);

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                source.Slice(copied, toCopy).CopyTo(new Span<T>(chunk.Value, index, toCopy));
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, ICollection items)
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

            EnsureCapacity(list, list._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(buffer, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableLinkedList<T>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, ICollection<T> items)
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

            EnsureCapacity(list, list._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(buffer, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableLinkedList<T>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, IReadOnlyList<T> items)
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(list, list._count + count);

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;

            for (int i = 0; i < count; i++)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                chunk.Value[index++] = items[i];
            }

            chunk.Top = index;
            list._count += count;
        }

        internal static IEnumerator AddRange<T>(this RecyclableLinkedList<T> list, IEnumerable source)
        {
            IEnumerator enumerator = source.GetEnumerator();
            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;
            int chunkLength = chunk.Value.Length;

            while (enumerator.MoveNext())
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    if (chunk.Next == null)
                    {
                        Grow(list);
                        chunk = list._tail;
                    }
                    else
                    {
                        chunk = chunk.Next;
                    }

                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                chunk.Value[index++] = (T)enumerator.Current!;
                list._count++;
            }

            chunk.Top = index;
            return enumerator;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, IEnumerable<T> items)
        {
            if (items is RecyclableList<T> recyclableList)
            {
                AddRange(list, recyclableList);
            }
            else if (items is RecyclableLongList<T> recyclableLongList)
            {
                AddRange(list, recyclableLongList);
            }
            else if (items is T[] array)
            {
                AddRange(list, array);
            }
            else if (items is List<T> listSource)
            {
                AddRange(list, listSource);
            }
            else if (items is ICollection<T> iCollection)
            {
                AddRange(list, iCollection);
            }
            else if (items is ICollection collection)
            {
                AddRange(list, collection);
            }
            else if (items is RecyclableHashSet<T> hashSet)
            {
                AddRange(list, hashSet);
            }
            else if (items is RecyclableStack<T> stack)
            {
                AddRange(list, stack);
            }
            else if (items is RecyclableSortedSet<T> sortedSet)
            {
                AddRange(list, sortedSet);
            }
            else if (items is RecyclableLinkedList<T> linkedList)
            {
                AddRange(list, linkedList);
            }
            else if (items is RecyclablePriorityQueue<T> priorityQueue)
            {
                AddRange(list, priorityQueue);
            }
            else if (items is RecyclableQueue<T> queue)
            {
                AddRange(list, queue);
            }
            else if (AddRangeHelper<T>.TryAddRange(list, items))
            {
            }
            else if (items is IReadOnlyList<T> readOnlyList)
            {
                AddRange(list, readOnlyList);
            }
            else
            {
                using IEnumerator<T> enumerator = items.GetEnumerator();
                BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
                int index = chunk.Top;
                int chunkLength = chunk.Value.Length;

                while (enumerator.MoveNext())
                {
                    if (index == chunkLength)
                    {
                        chunk.Top = index;
                        if (chunk.Next == null)
                        {
                            Grow(list);
                            chunk = list._tail;
                        }
                        else
                        {
                            chunk = chunk.Next;
                        }
                        index = chunk.Top;
                        chunkLength = chunk.Value.Length;
                    }

                    chunk.Value[index++] = enumerator.Current;
                    list._count++;
                }

                chunk.Top = index;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, RecyclableList<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;

            EnsureCapacity(list, list._count + count);

            int chunkLength = chunk.Value.Length;
            int copied = 0;
            T[] source = items._memoryBlock;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(source, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, RecyclableLongList<T> items)
        {
            long longCount = items._longCount;
            if (longCount == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;

            EnsureCapacity(list, list._count + longCount);

            int blockSize = items._blockSize;
            int fullBlocks = (int)(longCount >> items._blockSizePow2BitShift);
            int lastBlockLength = (int)(longCount & items._blockSizeMinus1);
            int chunkLength = chunk.Value.Length;

            for (int block = 0; block < fullBlocks; block++)
            {
                T[] source = items._memoryBlocks[block];
                int sourceIndex = 0;
                while (sourceIndex < blockSize)
                {
                    if (index == chunkLength)
                    {
                        chunk.Top = index;
                        chunk = chunk.Next!;
                        index = chunk.Top;
                        chunkLength = chunk.Value.Length;
                    }

                    int toCopy = Math.Min(chunkLength - index, blockSize - sourceIndex);
                    Array.Copy(source, sourceIndex, chunk.Value, index, toCopy);
                    sourceIndex += toCopy;
                    index += toCopy;
                }
            }

            if (lastBlockLength > 0)
            {
                T[] source = items._memoryBlocks[fullBlocks];
                int sourceIndex = 0;
                while (sourceIndex < lastBlockLength)
                {
                    if (index == chunkLength)
                    {
                        chunk.Top = index;
                        chunk = chunk.Next!;
                        index = chunk.Top;
                        chunkLength = chunk.Value.Length;
                    }

                    int toCopy = Math.Min(chunkLength - index, lastBlockLength - sourceIndex);
                    Array.Copy(source, sourceIndex, chunk.Value, index, toCopy);
                    sourceIndex += toCopy;
                    index += toCopy;
                }
            }

            chunk.Top = index;
            list._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, RecyclableHashSet<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;

            EnsureCapacity(list, list._count + count);

            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;

            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
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
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, RecyclableStack<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> destChunk = list._tail;
            int destIndex = destChunk.Top;
            int destLength = destChunk.Value.Length;

            EnsureCapacity(list, list._count + longCount);

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
                        destChunk = destChunk.Next!;
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
            list._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, RecyclableSortedSet<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;

            EnsureCapacity(list, list._count + count);

            T[] source = items._items;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(source, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, RecyclableLinkedList<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> destChunk = list._tail;
            int destIndex = destChunk.Top;
            int destLength = destChunk.Value.Length;

            EnsureCapacity(list, list._count + longCount);

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
            list._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, RecyclablePriorityQueue<T> items)
        {
            int count = items._size;
            if (count == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> chunk = list._tail;
            int index = chunk.Top;

            EnsureCapacity(list, list._count + count);

            T[] source = items._heap;
            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
                    index = chunk.Top;
                    chunkLength = chunk.Value.Length;
                }

                int toCopy = Math.Min(chunkLength - index, count - copied);
                Array.Copy(source, copied, chunk.Value, index, toCopy);
                copied += toCopy;
                index += toCopy;
            }

            chunk.Top = index;
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableLinkedList<T> list, RecyclableQueue<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<T> destChunk = list._tail;
            int destIndex = destChunk.Top;
            int destLength = destChunk.Value.Length;

            EnsureCapacity(list, list._count + longCount);

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
            list._count += longCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableLinkedList<KeyValuePair<TKey, TValue>> list, RecyclableDictionary<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            BiDirectionalRecyclableArrayPoolChunk<KeyValuePair<TKey, TValue>> chunk = list._tail;
            int index = chunk.Top;

            EnsureCapacity(list, list._count + count);

            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;

            int chunkLength = chunk.Value.Length;
            int copied = 0;

            while (copied < count)
            {
                if (index == chunkLength)
                {
                    chunk.Top = index;
                    chunk = chunk.Next!;
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
            list._count += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Grow<T>(RecyclableLinkedList<T> list)
        {
            int newSize;
            if (list._capacity >= RecyclableDefaults.MaxPooledBlockSize)
            {
                newSize = RecyclableDefaults.MaxPooledBlockSize;
            }
            else
            {
                long doubled = list._capacity << 1;
                newSize = (int)Math.Min(doubled - list._capacity, RecyclableDefaults.MaxPooledBlockSize);
            }

            var newChunk = BiDirectionalRecyclableArrayPoolChunkPool<T>.Rent();
            if (newChunk.Value.Length < newSize)
            {
                if (newChunk.Value.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<T>.ReturnShared(newChunk.Value, RecyclableLinkedList<T>._needsClearing);
                }

                newChunk.Value = newSize >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(newSize)
                    : new T[newSize];
            }

            newChunk.Top = 0;
            newChunk.Bottom = 0;
            newChunk.Previous = list._tail;
            newChunk.Next = null;
            list._tail.Next = newChunk;
            list._tail = newChunk;
            list._capacity += newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Grow<T>(RecyclableLinkedList<T> list, long additionalCapacity)
        {
            int newSize = additionalCapacity >= RecyclableDefaults.MaxPooledBlockSize
                ? RecyclableDefaults.MaxPooledBlockSize
                : (int)Math.Min(BitOperations.RoundUpToPowerOf2((uint)additionalCapacity), RecyclableDefaults.MaxPooledBlockSize);

            var newChunk = BiDirectionalRecyclableArrayPoolChunkPool<T>.Rent();
            if (newChunk.Value.Length < newSize)
            {
                if (newChunk.Value.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<T>.ReturnShared(newChunk.Value, RecyclableLinkedList<T>._needsClearing);
                }

                newChunk.Value = newSize >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(newSize)
                    : new T[newSize];
            }

            newChunk.Top = 0;
            newChunk.Bottom = 0;
            newChunk.Previous = list._tail;
            newChunk.Next = null;
            list._tail.Next = newChunk;
            list._tail = newChunk;
            list._capacity += newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCapacity<T>(RecyclableLinkedList<T> list, long requiredCapacity)
        {
            if (requiredCapacity <= list._capacity)
            {
                return;
            }

            long needed = requiredCapacity - list._capacity;

            do
            {
                int size = needed >= RecyclableDefaults.MaxPooledBlockSize
                    ? RecyclableDefaults.MaxPooledBlockSize
                    : (int)BitOperations.RoundUpToPowerOf2((uint)needed);

                var newChunk = BiDirectionalRecyclableArrayPoolChunkPool<T>.Rent();
                if (newChunk.Value.Length < size)
                {
                    if (newChunk.Value.Length >= RecyclableDefaults.MinPooledArrayLength)
                    {
                        RecyclableArrayPool<T>.ReturnShared(newChunk.Value, RecyclableLinkedList<T>._needsClearing);
                    }

                    newChunk.Value = size >= RecyclableDefaults.MinPooledArrayLength
                        ? RecyclableArrayPool<T>.RentShared(size)
                        : new T[size];
                }

                newChunk.Top = 0;
                newChunk.Bottom = 0;
                newChunk.Previous = list._tail;
                newChunk.Next = null;
                list._tail.Next = newChunk;
                list._tail = newChunk;
                list._capacity += size;

                needed -= size;
            }
            while (needed > 0);
        }
    }
}
