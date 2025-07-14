using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal sealed class RecyclableSortedSet<T> : IEnumerable<T>, IDisposable where T : notnull
    {
        internal static readonly bool _needsClearing = !typeof(T).IsValueType;

        internal readonly IComparer<T> _comparer;

        internal T[] _items;
        internal int _count;
        internal bool _disposed;

        public RecyclableSortedSet(int initialCapacity = RecyclableDefaults.InitialCapacity, IComparer<T>? comparer = null)
        {
            if (initialCapacity < 1)
            {
                initialCapacity = 1;
            }

            if (!BitOperations.IsPow2((uint)initialCapacity))
            {
                initialCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity);
            }

            _items = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<T>.RentShared(initialCapacity)
                : new T[initialCapacity];
            _count = 0;
            _comparer = comparer ?? Comparer<T>.Default;
        }

        public int Count => _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item)
        {
            int index = BinarySearch(item);
            if (index < _count && _comparer.Compare(_items[index], item) == 0)
            {
                return false;
            }

            if (_count == _items.Length)
            {
                Grow();
            }

            if (index < _count)
            {
                Array.Copy(_items, index, _items, index + 1, _count - index);
            }

            _items[index] = item;
            _count++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            int index = BinarySearch(item);
            return index < _count && _comparer.Compare(_items[index], item) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            int index = BinarySearch(item);
            if (index >= _count || _comparer.Compare(_items[index], item) != 0)
            {
                return false;
            }

            if (index < _count - 1)
            {
                Array.Copy(_items, index + 1, _items, index, _count - index - 1);
            }

            _count--;
            if (_needsClearing)
            {
                _items[_count] = default!;
            }

            return true;
        }

        public void Clear()
        {
            if (_needsClearing)
            {
                Array.Clear(_items, 0, _count);
            }

            _count = 0;
        }

        public Enumerator GetEnumerator() => new(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        private void Grow()
        {
            int newSize;
            if (_items.Length >= RecyclableDefaults.MaxPooledBlockSize)
            {
                newSize = RecyclableDefaults.MaxPooledBlockSize;
            }
            else
            {
                var doubled = _items.Length << 1;
                newSize = (int)Math.Min(doubled, RecyclableDefaults.MaxPooledBlockSize);
            }

            var newItems = newSize >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<T>.RentShared(newSize)
                : new T[newSize];
            Array.Copy(_items, newItems, _count);

            if (_items.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(_items, _needsClearing);
            }

            _items = newItems;
        }

        private int BinarySearch(T item)
        {
            int low = 0;
            int high = _count - 1;
            while (low <= high)
            {
                int mid = (low + high) >> 1;
                int cmp = _comparer.Compare(_items[mid], item);
                if (cmp == 0)
                {
                    return mid;
                }

                if (cmp < 0)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return low;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Clear();
            if (_items.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(_items, _needsClearing);
            }

            _items = Array.Empty<T>();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly RecyclableSortedSet<T> _set;
            private int _index;
            private T? _current;

            internal Enumerator(RecyclableSortedSet<T> set)
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

                _current = _set._items[_index++];
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
