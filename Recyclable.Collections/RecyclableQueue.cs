using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal sealed class RecyclableQueue<T> : IEnumerable<T>, IDisposable
    {
        private static readonly bool _needsClearing = !typeof(T).IsValueType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiDirectionalRecyclableArrayPoolChunk<T> RentChunk(int size, BiDirectionalRecyclableArrayPoolChunk<T>? previous)
        {
            var chunk = BiDirectionalRecyclableArrayPoolChunkPool<T>.Rent();
            if (chunk.Buffer.Length < size)
            {
                if (chunk.Buffer.Length >= RecyclableDefaults.MinPooledArrayLength)
                {
                    RecyclableArrayPool<T>.ReturnShared(chunk.Buffer, _needsClearing);
                }

                chunk.Buffer = size >= RecyclableDefaults.MinPooledArrayLength
                    ? RecyclableArrayPool<T>.RentShared(size)
                    : new T[size];
            }

            chunk.Top = 0;
            chunk.Bottom = 0;
            chunk.Previous = previous;
            chunk.Next = null;
            return chunk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReturnChunk(BiDirectionalRecyclableArrayPoolChunk<T> chunk)
        {
            BiDirectionalRecyclableArrayPoolChunkPool<T>.Return(chunk);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DisposeChunk(BiDirectionalRecyclableArrayPoolChunk<T> chunk)
        {
            BiDirectionalRecyclableArrayPoolChunkPool<T>.Dispose(chunk, _needsClearing);
        }

        private BiDirectionalRecyclableArrayPoolChunk<T> _head;
        private BiDirectionalRecyclableArrayPoolChunk<T> _tail;
        private bool _disposed;
        private long _capacity;
        private long _count;

        public RecyclableQueue(int initialCapacity = RecyclableDefaults.InitialCapacity)
        {
            if (initialCapacity < 1)
            {
                initialCapacity = 1;
            }

            if (!BitOperations.IsPow2((uint)initialCapacity))
            {
                initialCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity);
            }

            _head = RentChunk(initialCapacity, null);
            _tail = _head;
            _capacity = initialCapacity;
            _count = 0;
        }

        public RecyclableQueue(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.InitialCapacity) : this(initialCapacity)
        {
            foreach (var item in source)
            {
                Enqueue(item);
            }
        }

        public int Count => checked((int)_count);
        public long LongCount => _count;

        public void Enqueue(T item)
        {
            if (_tail.Top == _tail.Buffer.Length)
            {
                Grow();
            }

            _tail.Buffer[_tail.Top++] = item;
            _count++;
        }

        public void Add(T item) => Enqueue(item);

        public T Dequeue()
        {
            if (_count == 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(_count), "Queue is empty");
            }

            _count--;
            var value = _head.Buffer[_head.Bottom];
            if (_needsClearing)
            {
                _head.Buffer[_head.Bottom] = default!;
            }

            _head.Bottom++;
            if (_head.Bottom == _head.Top && _head.Next != null)
            {
                ReleaseHeadChunk();
            }
            else if (_head.Bottom == _head.Top)
            {
                _head.Bottom = 0;
                _head.Top = 0;
            }

            return value;
        }

        public bool TryDequeue(out T? item)
        {
            if (_count > 0)
            {
                item = Dequeue();
                return true;
            }

            item = default;
            return false;
        }

        public void Clear()
        {
            while (_head.Next != null)
            {
                ReleaseHeadChunk();
            }

            if (_needsClearing && _head.Top > _head.Bottom)
            {
                Array.Clear(_head.Buffer, _head.Bottom, _head.Top - _head.Bottom);
            }

            _capacity = _head.Buffer.Length;
            _head.Top = 0;
            _head.Bottom = 0;
            _tail = _head;
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

            var newChunk = RentChunk(newSize, _tail);
            _tail.Next = newChunk;
            _tail = newChunk;
            _capacity += newSize;
        }

        private void ReleaseHeadChunk()
        {
            var toReturn = _head;
            _head = toReturn.Next!;
            _head.Previous = null;
            _capacity -= toReturn.Buffer.Length;
            ReturnChunk(toReturn);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            var chunk = _head;
            while (chunk != null)
            {
                var next = chunk.Next;
                DisposeChunk(chunk);
                chunk = next;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly BiDirectionalRecyclableArrayPoolChunk<T>? _start;
            private BiDirectionalRecyclableArrayPoolChunk<T>? _chunk;
            private int _index;
            private T? _current;

            internal Enumerator(RecyclableQueue<T> queue)
            {
                _start = queue._head;
                _chunk = queue._head;
                _index = _chunk != null ? _chunk.Bottom : 0;
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

                if (_index >= _chunk.Top)
                {
                    _chunk = _chunk.Next;
                    if (_chunk == null)
                    {
                        return false;
                    }
                    _index = _chunk.Bottom;
                }

                _current = _chunk.Buffer[_index++];
                return true;
            }

            public void Reset()
            {
                _chunk = _start;
                _index = _chunk != null ? _chunk.Bottom : 0;
            }

            public void Dispose()
            {
                _chunk = null;
                _current = default;
            }
        }
    }
}
