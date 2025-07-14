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
            for (int i = 0; i < items.Length; i++)
            {
                stack.Push(items[i]);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, Span<T> items) => AddRange(stack, (ReadOnlySpan<T>)items);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, T[] items) => AddRange(stack, (ReadOnlySpan<T>)items);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, in Array items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                stack.Push((T)items.GetValue(i)!);
            }
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

            for (int i = 0; i < count; i++)
            {
                stack.Push(buffer[i]);
            }

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

            for (int i = 0; i < count; i++)
            {
                stack.Push(buffer[i]);
            }

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclableStack<T>._needsClearing);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableList<T> items)
        {
            AddRange(stack, new ReadOnlySpan<T>(items._memoryBlock, 0, items._count));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableLongList<T> items)
        {
            long longCount = items._longCount;
            if (longCount == 0)
            {
                return;
            }

            int blockSize = items._blockSize;
            int fullBlocks = (int)(longCount >> items._blockSizePow2BitShift);
            int lastBlockLength = (int)(longCount & items._blockSizeMinus1);

            for (int blockIndex = 0; blockIndex < fullBlocks; blockIndex++)
            {
                T[] block = items._memoryBlocks[blockIndex];
                for (int i = 0; i < blockSize; i++)
                {
                    stack.Push(block[i]);
                }
            }

            if (lastBlockLength > 0)
            {
                T[] block = items._memoryBlocks[fullBlocks];
                for (int i = 0; i < lastBlockLength; i++)
                {
                    stack.Push(block[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableHashSet<T> items)
        {
            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;
            for (int i = 0; i < items._count; i++)
            {
                stack.Push(entries[i >> shift][i & mask].Value);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableStack<T> items)
        {
            if (items._count == 0)
            {
                return;
            }

            RecyclableArrayPoolChunk<T> chunk = items._current;
            while (chunk.Previous != null)
            {
                chunk = chunk.Previous;
            }

            while (chunk != null)
            {
                for (int i = 0; i < chunk.Index; i++)
                {
                    stack.Push(chunk.Value[i]);
                }
                chunk = chunk.Next!;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableSortedSet<T> items)
        {
            for (int i = 0; i < items._count; i++)
            {
                stack.Push(items._items[i]);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableLinkedList<T> items)
        {
            var chunk = items._head;
            while (chunk != null)
            {
                for (int i = chunk.Bottom; i < chunk.Top; i++)
                {
                    stack.Push(chunk.Value[i]);
                }
                chunk = chunk.Next;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclablePriorityQueue<T> items)
        {
            AddRange(stack, new ReadOnlySpan<T>(items._heap, 0, items._size));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclableStack<T> stack, RecyclableQueue<T> items)
        {
            var chunk = items._head;
            while (chunk != null)
            {
                for (int i = chunk.Bottom; i < chunk.Top; i++)
                {
                    stack.Push(chunk.Value[i]);
                }
                chunk = chunk.Next;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclableStack<KeyValuePair<TKey, TValue>> stack, RecyclableDictionary<TKey, TValue> items)
            where TKey : notnull
        {
            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;
            for (int i = 0; i < items._count; i++)
            {
                ref var entry = ref entries[i >> shift][i & mask];
                stack.Push(new KeyValuePair<TKey, TValue>(entry.Key, entry.Value));
            }
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

