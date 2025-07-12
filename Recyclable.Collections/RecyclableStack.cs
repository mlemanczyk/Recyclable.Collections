using System.Collections;
using System.Numerics;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal sealed class RecyclableStack<T> : IEnumerable<T>, IDisposable
    {
        private static readonly bool _needsClearing = !typeof(T).IsValueType;

        private sealed class Chunk
        {
            internal T[] Buffer;
            internal int Index;
            internal Chunk? Previous;
            internal Chunk? Next;

            internal Chunk(int size, Chunk? previous)
            {
                Buffer = size >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(size)
                    : new T[size];
                Previous = previous;
                Next = null;
                Index = 0;
            }
        }

        private Chunk _current;
        private Chunk _bottom;
        private bool _disposed;
        private long _capacity;
        private long _count;

        public RecyclableStack(int initialCapacity = RecyclableDefaults.InitialCapacity)
        {
            if (initialCapacity < 1)
            {
                initialCapacity = 1;
            }

            if (!BitOperations.IsPow2((uint)initialCapacity))
            {
                initialCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity);
            }

            _current = new Chunk(initialCapacity, null);
            _bottom = _current;
            _capacity = initialCapacity;
            _count = 0;
        }

        public RecyclableStack(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.InitialCapacity) : this(initialCapacity)
        {
            foreach (var item in source)
            {
                Push(item);
            }
        }

        public int Count => checked((int)_count);
        public long LongCount => _count;

        public void Push(T item)
        {
            if (_current.Index == _current.Buffer.Length)
            {
                Grow();
            }

            _current.Buffer[_current.Index++] = item;
            _count++;
        }

        public void Add(T item) => Push(item);

        public T Pop()
        {
            if (_count == 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(_count), "Stack is empty");
            }

            _count--;
            _current.Index--;
            var value = _current.Buffer[_current.Index];
            if (_needsClearing)
            {
                _current.Buffer[_current.Index] = default!;
            }

            if (_current.Index == 0 && _current.Previous != null)
            {
                ReleaseCurrentChunk();
            }

            return value;
        }

        public void Clear()
        {
            while (_current.Previous != null)
            {
                ReleaseCurrentChunk();
            }

            if (_needsClearing && _current.Index > 0)
            {
                Array.Clear(_current.Buffer, 0, _current.Index);
            }

            _capacity = _current.Buffer.Length;
            _current.Index = 0;
            _count = 0;
            _bottom = _current;
        }

        public Enumerator GetEnumerator() => new(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        private void Grow()
        {
            int newSize;
            if (_capacity >= RecyclableDefaults.MaxPooledBlockSize)
            {
                newSize = RecyclableDefaults.MaxPooledBlockSize;
            }
            else
            {
                var doubled = _capacity * 2;
                newSize = (int)Math.Min(doubled - _capacity, RecyclableDefaults.MaxPooledBlockSize);
            }

            var newChunk = new Chunk(newSize, _current);
            _current.Next = newChunk;
            _current = newChunk;
            _capacity += newSize;
        }

        private void ReleaseCurrentChunk()
        {
            var toReturn = _current.Buffer;
            _current = _current.Previous!;
            _current.Next = null;
            _capacity -= toReturn.Length;
            if (_current.Previous == null)
            {
                _bottom = _current;
            }
            if (toReturn.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(toReturn, _needsClearing);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Clear();
            if (_current.Buffer.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(_current.Buffer, _needsClearing);
            }
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly Chunk? _start;
            private Chunk? _chunk;
            private int _index;
            private T? _current;

            internal Enumerator(RecyclableStack<T> stack)
            {
                _start = stack._current;
                _chunk = stack._current;
                _index = _chunk != null ? _chunk.Index - 1 : -1;
                _current = default;
            }

            public T Current => _current!;
            object IEnumerator.Current => _current!;

            public bool MoveNext()
            {
                if (_chunk == null)
                {
                    return false;
                }

                if (_index < 0)
                {
                    _chunk = _chunk.Previous;
                    if (_chunk == null)
                    {
                        return false;
                    }
                    _index = _chunk.Index - 1;
                }

                _current = _chunk.Buffer[_index--];
                return true;
            }

            public void Reset()
            {
                _chunk = _start;
                _index = _chunk != null ? _chunk.Index - 1 : -1;
            }

            public void Dispose()
            {
                _chunk = null;
                _current = default;
            }
        }
    }
}
