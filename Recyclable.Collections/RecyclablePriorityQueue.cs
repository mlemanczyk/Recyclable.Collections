using System.Collections;
using System.Runtime.CompilerServices;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal sealed class RecyclablePriorityQueue<T> : IEnumerable<T>, IDisposable
    {
        internal static readonly bool _needsClearing = !typeof(T).IsValueType;

        internal readonly IComparer<T> _comparer;

#nullable disable
        internal T[] _heap;
#nullable restore
        internal int _size;
        internal bool _disposed;

        public RecyclablePriorityQueue(int initialCapacity = RecyclableDefaults.InitialCapacity, IComparer<T>? comparer = null)
        {
            if (initialCapacity < 1)
            {
                initialCapacity = 1;
            }

            if (!BitOperations.IsPow2((uint)initialCapacity))
            {
                initialCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity);
            }

            _heap = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<T>.RentShared(initialCapacity)
                : new T[initialCapacity];
            _size = 0;
            _comparer = comparer ?? Comparer<T>.Default;
        }

        public int Count => _size;
        public long LongCount => _size;

        public void Enqueue(T item)
        {
            if (_size == _heap.Length)
            {
                Grow();
            }

            int index = _size++;
            _heap[index] = item;
            MoveUp(index);
        }

        public T Dequeue()
        {
            if (_size == 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(_size), "Queue is empty");
            }

            var root = _heap[0];
            if (_needsClearing)
            {
                _heap[0] = default!;
            }

            _size--;
            if (_size > 0)
            {
                _heap[0] = _heap[_size];
                if (_needsClearing)
                {
                    _heap[_size] = default!;
                }

                MoveDown(0);
            }

            return root;
        }

        public bool TryDequeue(out T? item)
        {
            if (_size > 0)
            {
                item = Dequeue();
                return true;
            }

            item = default;
            return false;
        }

        public T Peek()
        {
            if (_size == 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(_size), "Queue is empty");
            }

            return _heap[0];
        }

        public void Clear()
        {
            if (_needsClearing)
            {
                Array.Clear(_heap, 0, _size);
            }

            _size = 0;
        }

        public Enumerator GetEnumerator() => new(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        private void Grow()
        {
            int newSize;
            if (_heap.Length >= RecyclableDefaults.MaxPooledBlockSize)
            {
                newSize = RecyclableDefaults.MaxPooledBlockSize;
            }
            else
            {
                var doubled = _heap.Length << 1;
                newSize = (int)Math.Min(doubled, RecyclableDefaults.MaxPooledBlockSize);
            }

            var newHeap = newSize >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<T>.RentShared(newSize)
                : new T[newSize];
            Array.Copy(_heap, newHeap, _size);

            if (_heap.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(_heap, _needsClearing);
            }

            _heap = newHeap;
        }

        private void MoveUp(int index)
        {
            var item = _heap[index];
            while (index > 0)
            {
                int parent = (index - 1) >> 1;
                if (_comparer.Compare(item, _heap[parent]) >= 0)
                {
                    break;
                }

                _heap[index] = _heap[parent];
                index = parent;
            }

            _heap[index] = item;
        }

        private void MoveDown(int index)
        {
            var item = _heap[index];
            int half = _size >> 1;
            while (index < half)
            {
                int child = (index << 1) + 1;
                int right = child + 1;
                if (right < _size && _comparer.Compare(_heap[right], _heap[child]) < 0)
                {
                    child = right;
                }

                if (_comparer.Compare(_heap[child], item) >= 0)
                {
                    break;
                }

                _heap[index] = _heap[child];
                index = child;
            }

            _heap[index] = item;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_heap.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(_heap, _needsClearing);
            }

            _heap = Array.Empty<T>();
            _size = 0;
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly RecyclablePriorityQueue<T> _queue;
            private int _index;
            private T? _current;

            internal Enumerator(RecyclablePriorityQueue<T> queue)
            {
                _queue = queue;
                _index = 0;
                _current = default;
            }

            public T Current => _current!;
            object IEnumerator.Current => _current!;

            public bool MoveNext()
            {
                if (_index >= _queue._size)
                {
                    return false;
                }

                _current = _queue._heap[_index++];
                return true;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public void Dispose()
            {
                _current = default;
            }
        }
    }
}
