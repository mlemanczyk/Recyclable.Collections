using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal sealed class RecyclableLinkedList<T> : IEnumerable<T>, IDisposable
    {
        internal static readonly bool _needsClearing = !typeof(T).IsValueType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiDirectionalRecyclableArrayPoolChunk<T> RentChunk(int size, BiDirectionalRecyclableArrayPoolChunk<T>? previous, bool forHead)
        {
            var chunk = BiDirectionalRecyclableArrayPoolChunkPool<T>.Rent();
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

            if (forHead)
            {
                chunk.Top = size;
                chunk.Bottom = size;
            }
            else
            {
                chunk.Top = 0;
                chunk.Bottom = 0;
            }

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

        internal BiDirectionalRecyclableArrayPoolChunk<T> _head;
        internal BiDirectionalRecyclableArrayPoolChunk<T> _tail;
        internal bool _disposed;
        internal long _capacity;
        internal long _count;

        public RecyclableLinkedList(int initialCapacity = RecyclableDefaults.InitialCapacity)
        {
            if (initialCapacity < 1)
            {
                initialCapacity = 1;
            }

            if (!BitOperations.IsPow2((uint)initialCapacity))
            {
                initialCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity);
            }

            _head = RentChunk(initialCapacity, null, false);
            _tail = _head;
            _capacity = initialCapacity;
            _count = 0;
        }

        public RecyclableLinkedList(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.InitialCapacity) : this(initialCapacity)
        {
            foreach (var item in source)
            {
                AddLast(item);
            }
        }

        public int Count => checked((int)_count);
        public long LongCount => _count;

        public void AddLast(T item)
        {
            if (_tail.Top == _tail.Value.Length)
            {
                GrowTail();
            }

            _tail.Value[_tail.Top++] = item;
            _count++;
        }

        public void AddFirst(T item)
        {
            if (_head.Bottom == 0)
            {
                GrowHead();
            }

            _head.Value[--_head.Bottom] = item;
            _count++;
        }

        public T RemoveFirst()
        {
            if (_count == 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(_count), "List is empty");
            }

            _count--;
            var value = _head.Value[_head.Bottom];
            if (_needsClearing)
            {
                _head.Value[_head.Bottom] = default!;
            }

            _head.Bottom++;
            if (_head.Bottom == _head.Top && _head.Next != null)
            {
                ReleaseHeadChunk();
            }
            else if (_head.Bottom == _head.Top)
            {
                _head.Bottom = _head.Top;
            }

            return value;
        }

        public T RemoveLast()
        {
            if (_count == 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(_count), "List is empty");
            }

            _count--;
            _tail.Top--;
            var value = _tail.Value[_tail.Top];
            if (_needsClearing)
            {
                _tail.Value[_tail.Top] = default!;
            }

            if (_tail.Top == _tail.Bottom && _tail.Previous != null)
            {
                ReleaseTailChunk();
            }
            else if (_tail.Top == _tail.Bottom)
            {
                _tail.Top = _tail.Bottom;
            }

            return value;
        }

        public void Clear()
        {
            while (_head.Next != null)
            {
                ReleaseHeadChunk();
            }

            while (_tail.Previous != null)
            {
                ReleaseTailChunk();
            }

            if (_needsClearing && _head.Top > _head.Bottom)
            {
                Array.Clear(_head.Value, _head.Bottom, _head.Top - _head.Bottom);
            }

            _capacity = _head.Value.Length;
            _head.Bottom = _head.Top;
            _tail = _head;
            _count = 0;
        }

        public Enumerator GetEnumerator() => new(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        private void GrowTail()
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

            var newChunk = RentChunk(newSize, _tail, false);
            _tail.Next = newChunk;
            _tail = newChunk;
            _capacity += newSize;
        }

        private void GrowHead()
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

            var newChunk = RentChunk(newSize, null, true);
            newChunk.Next = _head;
            _head.Previous = newChunk;
            _head = newChunk;
            _capacity += newSize;
        }

        private void ReleaseHeadChunk()
        {
            var toReturn = _head;
            _head = toReturn.Next!;
            _head.Previous = null;
            _capacity -= toReturn.Value.Length;
            ReturnChunk(toReturn);
        }

        private void ReleaseTailChunk()
        {
            var toReturn = _tail;
            _tail = toReturn.Previous!;
            _tail.Next = null;
            _capacity -= toReturn.Value.Length;
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

            internal Enumerator(RecyclableLinkedList<T> list)
            {
                _start = list._head;
                _chunk = list._head;
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

                while (_index >= _chunk.Top)
                {
                    _chunk = _chunk.Next;
                    if (_chunk == null)
                    {
                        return false;
                    }
                    _index = _chunk.Bottom;
                }

                _current = _chunk.Value[_index++];
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

