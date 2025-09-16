using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal static class zRecyclablePriorityQueueAddRange
    {
        private static class AddRangeHelper<T>
        {
            private static readonly Action<RecyclablePriorityQueue<T>, object>? _dictionaryAdder;
            private static readonly Action<RecyclablePriorityQueue<T>, object>? _sortedListAdder;
            private static readonly Action<RecyclablePriorityQueue<T>, object>? _sortedDictionaryAdder;
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

                    MethodInfo? method = typeof(zRecyclablePriorityQueueAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclablePriorityQueue<>).MakeGenericType(elementType), _dictionaryType },
                        modifiers: null);
                    if (method != null)
                    {
                        var queueParam = Expression.Parameter(typeof(RecyclablePriorityQueue<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, queueParam, Expression.Convert(objParam, _dictionaryType));
                        _dictionaryAdder = Expression.Lambda<Action<RecyclablePriorityQueue<T>, object>>(call, queueParam, objParam).Compile();
                    }

                    method = typeof(zRecyclablePriorityQueueAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclablePriorityQueue<>).MakeGenericType(elementType), _sortedDictionaryType },
                        modifiers: null);
                    if (method != null)
                    {
                        var queueParam = Expression.Parameter(typeof(RecyclablePriorityQueue<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, queueParam, Expression.Convert(objParam, _sortedDictionaryType));
                        _sortedDictionaryAdder = Expression.Lambda<Action<RecyclablePriorityQueue<T>, object>>(call, queueParam, objParam).Compile();
                    }
                }
                else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                {
                    Type[] args = elementType.GetGenericArguments();
                    _sortedListType = typeof(RecyclableSortedList<,>).MakeGenericType(args);
                    MethodInfo? method = typeof(zRecyclablePriorityQueueAddRange).GetMethod(
                        nameof(AddRange),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(RecyclablePriorityQueue<>).MakeGenericType(elementType), _sortedListType },
                        modifiers: null);
                    if (method != null)
                    {
                        var queueParam = Expression.Parameter(typeof(RecyclablePriorityQueue<T>));
                        var objParam = Expression.Parameter(typeof(object));
                        var call = Expression.Call(method, queueParam, Expression.Convert(objParam, _sortedListType));
                        _sortedListAdder = Expression.Lambda<Action<RecyclablePriorityQueue<T>, object>>(call, queueParam, objParam).Compile();
                    }
                }
            }

            internal static bool TryAddRange(RecyclablePriorityQueue<T> queue, IEnumerable<T> items)
            {
                if (_dictionaryAdder != null && _dictionaryType!.IsInstanceOfType(items))
                {
                    _dictionaryAdder(queue, items);
                    return true;
                }

                if (_sortedDictionaryAdder != null && _sortedDictionaryType!.IsInstanceOfType(items))
                {
                    _sortedDictionaryAdder(queue, items);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureCapacity<T>(RecyclablePriorityQueue<T> queue, int min)
        {
            if (queue._heap.Length >= min)
            {
                return;
            }

            int newSize = queue._heap.Length;
            do
            {
                if (newSize >= RecyclableDefaults.MaxPooledBlockSize)
                {
                    newSize = RecyclableDefaults.MaxPooledBlockSize;
                    break;
                }

                newSize <<= 1;
            }
            while (newSize < min);

            if (newSize < min)
            {
                newSize = min;
            }

            T[] newHeap = newSize >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<T>.RentShared(newSize)
                : new T[newSize];
            System.Array.Copy(queue._heap, newHeap, queue._size);

            if (queue._heap.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(queue._heap, RecyclablePriorityQueue<T>._needsClearing);
            }

            queue._heap = newHeap;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, ReadOnlySpan<T> items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = items[i];
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, Span<T> items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = items[i];
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, T[] items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = items[i];
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, in Array items)
        {
            int count = items.Length;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            Array.Copy(items, items.GetLowerBound(0), queue._heap, startIndex, count);
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, List<T> items)
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = items[i];
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, ICollection items)
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

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);
            Array.Copy(buffer, 0, queue._heap, startIndex, count);
            queue._size = newCount;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclablePriorityQueue<T>._needsClearing);
            }

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, ICollection<T> items)
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

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);
            Array.Copy(buffer, 0, queue._heap, startIndex, count);
            queue._size = newCount;

            if (count >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, RecyclablePriorityQueue<T>._needsClearing);
            }

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, RecyclableList<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            T[] source = items._memoryBlock;
            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = source[i];
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, RecyclablePriorityQueue<T> items)
        {
            int count = items._size;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            T[] source = items._heap;
            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = source[i];
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

            queue._heap[index] = item;
        }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, RecyclableQueue<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            int count = checked((int)longCount);
            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            var sourceChunk = items._head;
            int destIndex = startIndex;
            while (sourceChunk != null)
            {
                int sourceIndex = sourceChunk.Bottom;
                int sourceTop = sourceChunk.Top;
                T[] source = sourceChunk.Value;

                while (sourceIndex < sourceTop)
                {
                    queue._heap[destIndex++] = source[sourceIndex++];
                }

                sourceChunk = sourceChunk.Next;
            }

            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, RecyclableStack<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            int count = checked((int)longCount);
            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            var sourceChunk = items._current;
            while (sourceChunk.Previous != null)
            {
                sourceChunk = sourceChunk.Previous;
            }

            int destIndex = startIndex;
            while (sourceChunk != null)
            {
                T[] source = sourceChunk.Value;
                int sourceCount = sourceChunk.Index;
                int sourceIndex = 0;

                while (sourceIndex < sourceCount)
                {
                    queue._heap[destIndex++] = source[sourceIndex++];
                }

                sourceChunk = sourceChunk.Next;
            }

            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, RecyclableLinkedList<T> items)
        {
            long longCount = items._count;
            if (longCount == 0)
            {
                return;
            }

            int count = checked((int)longCount);
            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            var sourceChunk = items._head;
            int destIndex = startIndex;
            while (sourceChunk != null)
            {
                int sourceIndex = sourceChunk.Bottom;
                int sourceTop = sourceChunk.Top;
                T[] source = sourceChunk.Value;

                while (sourceIndex < sourceTop)
                {
                    queue._heap[destIndex++] = source[sourceIndex++];
                }

                sourceChunk = sourceChunk.Next;
            }

            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, RecyclableSortedSet<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            T[] source = items._items;
            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = source[i];
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, RecyclableHashSet<T> items)
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;

            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = entries[i >> shift][i & mask].Value;
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, RecyclableLongList<T> items)
        {
            long longCount = items._longCount;
            if (longCount == 0)
            {
                return;
            }

            int blockSize = items._blockSize;
            int fullBlocks = (int)(longCount >> items._blockSizePow2BitShift);
            int lastBlockLength = (int)(longCount & items._blockSizeMinus1);

            int startIndex = queue._size;
            int newCount = startIndex + checked((int)longCount);
            EnsureCapacity(queue, newCount);

            int destIndex = startIndex;
            for (int blockIndex = 0; blockIndex < fullBlocks; blockIndex++)
            {
                T[] block = items._memoryBlocks[blockIndex];
                int sourceIndex = 0;
                while (sourceIndex < blockSize)
                {
                    queue._heap[destIndex++] = block[sourceIndex++];
                }
            }

            if (lastBlockLength > 0)
            {
                T[] block = items._memoryBlocks[fullBlocks];
                int sourceIndex = 0;
                while (sourceIndex < lastBlockLength)
                {
                    queue._heap[destIndex++] = block[sourceIndex++];
                }
            }

            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclablePriorityQueue<KeyValuePair<TKey, TValue>> queue, RecyclableDictionary<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            var entries = items._entries;
            int shift = items._blockShift,
                mask = items._blockSizeMinus1;

            for (int i = 0; i < count; i++)
            {
                ref var src = ref entries[i >> shift][i & mask];
                queue._heap[startIndex + i] = new KeyValuePair<TKey, TValue>(src.Key, src.Value);
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                var item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclablePriorityQueue<KeyValuePair<TKey, TValue>> queue, RecyclableSortedDictionary<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = new KeyValuePair<TKey, TValue>(items.GetKey(i), items.GetValue(i));
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                var item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<TKey, TValue>(this RecyclablePriorityQueue<(TKey Key, TValue Value)> queue, RecyclableSortedList<TKey, TValue> items)
            where TKey : notnull
        {
            int count = items._count;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            TKey[] keys = items._keys;
            TValue[] values = items._values;

            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = (keys[i], values[i]);
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                var item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, IReadOnlyList<T> items)
        {
            int count = items.Count;
            if (count == 0)
            {
                return;
            }

            int startIndex = queue._size;
            int newCount = startIndex + count;
            EnsureCapacity(queue, newCount);

            for (int i = 0; i < count; i++)
            {
                queue._heap[startIndex + i] = items[i];
            }
            queue._size = newCount;

            int half = newCount >> 1;
            for (int i = half - 1; i >= 0; i--)
            {
                int index = i;
                T item = queue._heap[index];
                while (index < half)
                {
                    int child = (index << 1) + 1;
                    int right = child + 1;
                    if (right < newCount && queue._comparer.Compare(queue._heap[right], queue._heap[child]) < 0)
                    {
                        child = right;
                    }

                    if (queue._comparer.Compare(queue._heap[child], item) >= 0)
                    {
                        break;
                    }

                    queue._heap[index] = queue._heap[child];
                    index = child;
                }

                queue._heap[index] = item;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void AddRange<T>(this RecyclablePriorityQueue<T> queue, IEnumerable<T> items)
        {
            if (items is RecyclableList<T> recyclableList)
            {
                AddRange(queue, recyclableList);
            }
            else if (items is RecyclablePriorityQueue<T> priorityQueue)
            {
                AddRange(queue, priorityQueue);
            }
            else if (items is RecyclableQueue<T> recyclableQueue)
            {
                AddRange(queue, recyclableQueue);
            }
            else if (items is RecyclableStack<T> recyclableStack)
            {
                AddRange(queue, recyclableStack);
            }
            else if (items is RecyclableLinkedList<T> linkedList)
            {
                AddRange(queue, linkedList);
            }
            else if (items is RecyclableSortedSet<T> sortedSet)
            {
                AddRange(queue, sortedSet);
            }
            else if (items is RecyclableHashSet<T> hashSet)
            {
                AddRange(queue, hashSet);
            }
            else if (items is RecyclableLongList<T> longList)
            {
                AddRange(queue, longList);
            }
            else if (items is T[] array)
            {
                AddRange(queue, array);
            }
            else if (items is List<T> list)
            {
                AddRange(queue, list);
            }
            else if (items is ICollection<T> genericCollection)
            {
                AddRange(queue, genericCollection);
            }
            else if (items is ICollection collection)
            {
                AddRange(queue, collection);
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
                foreach (T item in items)
                {
                    queue.Enqueue(item);
                }
            }
        }
    }
}
