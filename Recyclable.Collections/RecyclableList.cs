using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
	[Serializable]
	public sealed partial class RecyclableList<T> : IList<T>, IReadOnlyList<T>, IDisposable
	{
		internal static readonly bool _needsClearing = !typeof(T).IsValueType;

		public static explicit operator ReadOnlySpan<T>(RecyclableList<T> source) => new(source._memoryBlock, 0, source._count);

#nullable disable
		internal T[] _memoryBlock;
#nullable restore

		internal ulong _version;
		public ulong Version => _version;

		public RecyclableList()
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
		}

		public RecyclableList(int initialCapacity)
		{
			if (initialCapacity >= RecyclableDefaults.InitialCapacity)
			{
				_memoryBlock = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
					? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity)))
					: new T[initialCapacity];
				_capacity = _memoryBlock.Length;
			}
			else
			{
				_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
				_capacity = RecyclableDefaults.InitialCapacity;
			}
		}

		public RecyclableList(int initialCapacity, int lowerBound)
		{
			if (initialCapacity >= RecyclableDefaults.InitialCapacity)
			{
				_memoryBlock = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
					? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity)))
					: new T[initialCapacity];
				_capacity = _memoryBlock.Length;
			}
			else
			{
				_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
				_capacity = RecyclableDefaults.InitialCapacity;
			}
		}

		public RecyclableList(in Array source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(RecyclableList<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(RecyclableLongList<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(ReadOnlySpan<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(in T[] source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(IReadOnlyList<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(List<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(ICollection source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(ICollection<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(IEnumerable source, int initialCapacity = RecyclableDefaults.InitialCapacity)
		{
			if (initialCapacity >= RecyclableDefaults.InitialCapacity)
			{
				_memoryBlock = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
					? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity)))
					: new T[initialCapacity];
				_capacity = _memoryBlock.Length;
			}
			else
			{
				_capacity = RecyclableDefaults.InitialCapacity;
				_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			}

			zRecyclableListAddRange.AddRange(this, source, RecyclableDefaults.MinPooledArrayLength);
		}

		public RecyclableList(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.InitialCapacity)
		{
			if (initialCapacity >= RecyclableDefaults.InitialCapacity)
			{
				_memoryBlock = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
					? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity)))
					: new T[initialCapacity];
				_capacity = _memoryBlock.Length;
			}
			else
			{
				_capacity = RecyclableDefaults.InitialCapacity;
				_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			}

			zRecyclableListAddRange.AddRange(this, source);
		}

		public RecyclableList(Span<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			zRecyclableListAddRange.AddRange(this, source);
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _memoryBlock[index];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				_memoryBlock[index] = value;
				_version++;
			}
		}

		internal int _capacity;
		public int Capacity
		{
			get => _capacity;
			set
			{
				if (_capacity != value)
				{
					_capacity = RecyclableListHelpers<T>.EnsureCapacity(this, _count, checked((int)BitOperations.RoundUpToPowerOf2((uint)value)));
					_version++;
				}
			}
		}

                internal int _count;
                public int Count
                {
                        get => _count;
                        set
                        {
				if (_count != value)
				{
					if (value > _capacity)
					{
						_capacity = RecyclableListHelpers<T>.EnsureCapacity(this, _capacity, checked((int)BitOperations.RoundUpToPowerOf2((uint)value)));
					}

					_count = value;
					_version++;
				}
                        }
                }

                /// <summary>
                /// Ensures that the capacity of this list is at least the specified value.
                /// </summary>
                /// <param name="capacity">The minimum capacity to ensure.</param>
                /// <returns>The new capacity of the list.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public int EnsureCapacity(int capacity)
                {
                        if (capacity > _capacity)
                        {
                                _capacity = RecyclableListHelpers<T>.EnsureCapacity(this, _count, checked((int)BitOperations.RoundUpToPowerOf2((uint)capacity)));
                        }

                        return _capacity;
                }

                public bool IsReadOnly => false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			if (_count >= _capacity)
			{
				if (BitOperations.IsPow2(_capacity))
				{
					RecyclableListHelpers<T>.EnsureCapacity(this);
				}
				else
				{
					_capacity = RecyclableListHelpers<T>.EnsureCapacity(this, checked((int)BitOperations.RoundUpToPowerOf2((uint)_capacity)));
				}
			}

			_memoryBlock[_count++] = item;
			_version++;
		}

		public T[] AsArray => _memoryBlock;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Clear()
		{
			if (_count == 0)
			{
				return;
			}

			if (_needsClearing)
			{
				Array.Clear(_memoryBlock, 0, _count);
			}

			_count = 0;
			_version++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item) => _count != 0 && Array.IndexOf(_memoryBlock, item, 0, _count) >= 0;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void CopyTo(T[] array, int index) => Array.Copy(_memoryBlock, 0, array, index, _count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T itemToFind) => _count != 0
				? Array.IndexOf(_memoryBlock, itemToFind, 0, _count)
				: RecyclableDefaults.ItemNotFoundIndex;

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void Insert(int index, T item)
		{
			int oldCount = _count;
			if (_capacity < oldCount + 1)
			{
				_capacity = RecyclableListHelpers<T>.EnsureCapacity(this, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)oldCount + 1)));
			}

			if (index < oldCount)
			{
				// TODO: Compare performance
				// Array.Copy(_memoryBlock, index, _memoryBlock, index + 1, oldCount);
				new ReadOnlySpan<T>(_memoryBlock, index, oldCount -= index)
					.CopyTo(new Span<T>(_memoryBlock, index + 1, oldCount));
			}

			_memoryBlock[index] = item;
			_count++;
			_version++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public bool Remove(T itemToRemove)
		{
			var index = IndexOf(itemToRemove);
			if (index >= 0)
			{
				_count--;
				if (index < _count)
				{
					Array.Copy(_memoryBlock, index + 1, _memoryBlock, index, _count - index);
				}

				if (_needsClearing)
				{
					_memoryBlock[_count] = default;
				}

				_version++;

				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void RemoveAt(int index)
		{
			if (index >= _count)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index), $"Provided value {index} exceeds the no. of items on the list {_count}");
			}

			_count--;
			if (index < _count)
			{
				Array.Copy(_memoryBlock, index + 1, _memoryBlock, index, _count - index);
			}

			if (_needsClearing)
			{
				_memoryBlock[_count] = default;
			}

			_version++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator GetEnumerator() => new(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

		public void Dispose()
		{
			_version++;
			if (_count != 0)
			{
				if (_needsClearing)
				{
					Array.Clear(_memoryBlock, 0, _count);
				}

				_count = 0;
			}

			if (_capacity >= RecyclableDefaults.MinPooledArrayLength)
			{
				// If anything, it has been already cleared by .Clear(), Remove() or RemoveAt() methods, as the list was modified / disposed.
				RecyclableArrayPool<T>.ReturnShared(_memoryBlock, false);
			}

			_capacity = 0;
			GC.SuppressFinalize(this);
		}
	}
}
