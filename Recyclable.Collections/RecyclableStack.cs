using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal sealed class RecyclableStack<T> : IEnumerable<T>, IDisposable
    {
        internal static readonly bool _needsClearing = !typeof(T).IsValueType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RecyclableArrayPoolChunk<T> RentChunk(int size, RecyclableArrayPoolChunk<T>? previous)
        {
            var chunk = RecyclableArrayPoolChunkPool<T>.Rent();
            if (chunk.Value.Length < size)
            {
                if (chunk.Value.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<T>.ReturnShared(chunk.Value, _needsClearing);
                }

                chunk.Value = size >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(size)
                    : new T[size];
            }
            chunk.Index = 0;
            chunk.Previous = previous;
            chunk.Next = null;
            return chunk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReturnChunk(RecyclableArrayPoolChunk<T> chunk)
        {
            RecyclableArrayPoolChunkPool<T>.Return(chunk);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DisposeChunk(RecyclableArrayPoolChunk<T> chunk)
        {
            RecyclableArrayPoolChunkPool<T>.Dispose(chunk, _needsClearing);
        }

        internal RecyclableArrayPoolChunk<T> _current;
        internal bool _disposed;
        internal long _capacity;
        internal long _count;

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

            _current = RentChunk(initialCapacity, null);
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
            if (_current.Index == _current.Value.Length)
            {
                Grow();
            }

            _current.Value[_current.Index++] = item;
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
            var value = _current.Value[_current.Index];
            if (_needsClearing)
            {
                _current.Value[_current.Index] = default!;
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
                Array.Clear(_current.Value, 0, _current.Index);
            }

            _capacity = _current.Value.Length;
            _current.Index = 0;
            _count = 0;
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
                var doubled = _capacity << 1;
                newSize = (int)Math.Min(doubled - _capacity, RecyclableDefaults.MaxPooledBlockSize);
            }

            var newChunk = RentChunk(newSize, _current);
            _current.Next = newChunk;
            _current = newChunk;
            _capacity += newSize;
        }

        private void ReleaseCurrentChunk()
        {
            var toReturn = _current;
            _current = toReturn.Previous!;
            _current.Next = null;
            _capacity -= toReturn.Value.Length;
            ReturnChunk(toReturn);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            var chunk = _current;
            while (chunk != null)
            {
                var previous = chunk.Previous;
                DisposeChunk(chunk);
                chunk = previous;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly RecyclableArrayPoolChunk<T>? _start;
            private RecyclableArrayPoolChunk<T>? _chunk;
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
                while (true)
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
                        continue;
                    }

                    _current = _chunk.Value[_index--];
                    return true;
                }
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
