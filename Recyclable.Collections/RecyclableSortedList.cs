using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal sealed class RecyclableSortedList<TKey, TValue> : IEnumerable<(TKey Key, TValue Value)>, IDisposable
        where TKey : notnull
    {
        internal static readonly bool _needsClearing = !typeof(TKey).IsValueType || !typeof(TValue).IsValueType;

        internal TKey[] _keys;
        internal TValue[] _values;
        internal int _count;
        internal readonly IComparer<TKey> _comparer;
        internal bool _disposed;

        public RecyclableSortedList(int initialCapacity = RecyclableDefaults.InitialCapacity, IComparer<TKey>? comparer = null)
        {
            if (initialCapacity < 1)
            {
                initialCapacity = 1;
            }

            if (!BitOperations.IsPow2((uint)initialCapacity))
            {
                initialCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity);
            }

            _keys = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<TKey>.RentShared(initialCapacity)
                : new TKey[initialCapacity];
            _values = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<TValue>.RentShared(initialCapacity)
                : new TValue[initialCapacity];
            _count = 0;
            _comparer = comparer ?? Comparer<TKey>.Default;
        }

        public int Count => _count;

        public TValue this[TKey key]
        {
            get
            {
                int index = BinarySearch(key);
                if (index >= 0)
                {
                    return _values[index];
                }

                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(key), "Key not found");
                return default!;
            }
            set
            {
                int index = BinarySearch(key);
                if (index >= 0)
                {
                    _values[index] = value;
                    return;
                }

                Insert(~index, key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value)
        {
            int index = BinarySearch(key);
            if (index >= 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(key), "An element with the same key already exists.");
            }

            Insert(~index, key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => BinarySearch(key) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = BinarySearch(key);
            if (index >= 0)
            {
                value = _values[index];
                return true;
            }

            value = default!;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            int index = BinarySearch(key);
            if (index < 0)
            {
                return false;
            }

            if (index < _count - 1)
            {
                Array.Copy(_keys, index + 1, _keys, index, _count - index - 1);
                Array.Copy(_values, index + 1, _values, index, _count - index - 1);
            }

            _count--;
            if (_needsClearing)
            {
                _keys[_count] = default!;
                _values[_count] = default!;
            }

            return true;
        }

        public void Clear()
        {
            if (_needsClearing && _count > 0)
            {
                Array.Clear(_keys, 0, _count);
                Array.Clear(_values, 0, _count);
            }

            _count = 0;
        }

        public TKey GetKey(int index) => _keys[index];
        public TValue GetValue(int index) => _values[index];

        public Enumerator GetEnumerator() => new(this);
        IEnumerator<(TKey Key, TValue Value)> IEnumerable<(TKey Key, TValue Value)>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_keys.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<TKey>.ReturnShared(_keys, _needsClearing);
            }

            if (_values.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<TValue>.ReturnShared(_values, _needsClearing);
            }

            _keys = Array.Empty<TKey>();
            _values = Array.Empty<TValue>();
            _count = 0;
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void Insert(int index, TKey key, TValue value)
        {
            if (_count == _keys.Length)
            {
                Grow(_count + 1);
            }

            if (index < _count)
            {
                Array.Copy(_keys, index, _keys, index + 1, _count - index);
                Array.Copy(_values, index, _values, index + 1, _count - index);
            }

            _keys[index] = key;
            _values[index] = value;
            _count++;
        }

        private void Grow(int min)
        {
            int newSize = _keys.Length << 1;
            if (newSize < min)
            {
                newSize = min;
            }

            var newKeys = newSize >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<TKey>.RentShared(newSize)
                : new TKey[newSize];
            var newValues = newSize >= RecyclableDefaults.MinPooledArrayLength
                ? RecyclableArrayPool<TValue>.RentShared(newSize)
                : new TValue[newSize];

            Array.Copy(_keys, newKeys, _count);
            Array.Copy(_values, newValues, _count);

            if (_keys.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<TKey>.ReturnShared(_keys, _needsClearing);
            }

            if (_values.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<TValue>.ReturnShared(_values, _needsClearing);
            }

            _keys = newKeys;
            _values = newValues;
        }

        private int BinarySearch(TKey key)
        {
            int low = 0;
            int high = _count - 1;
            while (low <= high)
            {
                int mid = (low + high) >> 1;
                int cmp = _comparer.Compare(_keys[mid], key);
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

            return ~low;
        }

        public struct Enumerator : IEnumerator<(TKey Key, TValue Value)>
        {
            private readonly RecyclableSortedList<TKey, TValue> _list;
            private int _index;
            private (TKey, TValue) _current;

            internal Enumerator(RecyclableSortedList<TKey, TValue> list)
            {
                _list = list;
                _index = 0;
                _current = default!;
            }

            public (TKey Key, TValue Value) Current => _current;
            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                if (_index >= _list._count)
                {
                    return false;
                }

                _current = (_list._keys[_index], _list._values[_index]);
                _index++;
                return true;
            }

            public void Reset()
            {
                _index = 0;
                _current = default!;
            }

            public void Dispose()
            {
            }
        }
    }
}
