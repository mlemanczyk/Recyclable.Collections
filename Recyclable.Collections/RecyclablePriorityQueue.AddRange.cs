using System.Runtime.CompilerServices;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal static class zRecyclablePriorityQueueAddRange
    {
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
    }
}
