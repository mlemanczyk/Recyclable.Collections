using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
    internal sealed class RecyclableSortedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
        where TKey : notnull
    {
        private static readonly bool _needsClearing = !typeof(TKey).IsValueType || !typeof(TValue).IsValueType;

        private readonly IComparer<TKey> _comparer;
        private TKey[] _keys;
        private TValue[] _values;
        private int _count;
        private bool _disposed;

        public RecyclableSortedDictionary(int initialCapacity = RecyclableDefaults.InitialCapacity, IComparer<TKey>? comparer = null)
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
                if (index < _count && _comparer.Compare(_keys[index], key) == 0)
                {
                    return _values[index];
                }

                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(key), "Key not found");
                return default!;
            }
            set
            {
                int index = BinarySearch(key);
                if (index < _count && _comparer.Compare(_keys[index], key) == 0)
                {
                    _values[index] = value;
                    return;
                }

                InsertAt(index, key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value)
        {
            int index = BinarySearch(key);
            if (index < _count && _comparer.Compare(_keys[index], key) == 0)
            {
                throw new ArgumentException("An element with the same key already exists.", nameof(key));
            }

            InsertAt(index, key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            int index = BinarySearch(key);
            return index < _count && _comparer.Compare(_keys[index], key) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            int index = BinarySearch(key);
            if (index >= _count || _comparer.Compare(_keys[index], key) != 0)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = BinarySearch(key);
            if (index < _count && _comparer.Compare(_keys[index], key) == 0)
            {
                value = _values[index];
                return true;
            }

            value = default!;
            return false;
        }

        public void Clear()
        {
            if (_needsClearing)
            {
                Array.Clear(_keys, 0, _count);
                Array.Clear(_values, 0, _count);
            }

            _count = 0;
        }

        public TKey GetKey(int index) => _keys[index];
        public TValue GetValue(int index) => _values[index];

        public Enumerator GetEnumerator() => new(this);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        private void InsertAt(int index, TKey key, TValue value)
        {
            if (_count == _keys.Length)
            {
                Grow();
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

        private void Grow()
        {
            int newSize;
            if (_keys.Length >= RecyclableDefaults.MaxPooledBlockSize)
            {
                newSize = RecyclableDefaults.MaxPooledBlockSize;
            }
            else
            {
                int doubled = _keys.Length << 1;
                newSize = (int)Math.Min(doubled, RecyclableDefaults.MaxPooledBlockSize);
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

            return low;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Clear();
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
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly RecyclableSortedDictionary<TKey, TValue> _dictionary;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;

            internal Enumerator(RecyclableSortedDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _index = 0;
                _current = default;
            }

            public KeyValuePair<TKey, TValue> Current => _current;
            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                if (_index >= _dictionary._count)
                {
                    return false;
                }

                _current = new KeyValuePair<TKey, TValue>(_dictionary._keys[_index], _dictionary._values[_index]);
                _index++;
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
