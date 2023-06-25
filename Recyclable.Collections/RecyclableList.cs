using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	[Serializable]
	public sealed partial class RecyclableList<T> : IList<T>, IReadOnlyList<T>, IList, IDisposable
	{
		private static readonly ArrayPool<T> _arrayPool = ArrayPool<T>.Create();
		private static readonly bool _defaultIsNull = default(T) == null;
		private static readonly bool NeedsClearing = !typeof(T).IsValueType;

		public static explicit operator ReadOnlySpan<T>(RecyclableList<T> source) => new(source._memoryBlock, 0, source.Count);

#nullable disable
		private T[] _memoryBlock;
#nullable restore

		private ulong _version;
		public ulong Version => _version;

		private IEnumerator<T> AddRangeEnumerated(IEnumerable<T> source, int growByCount)
		{
			int targetItemIdx = _count;
			Span<T> memorySpan;

			int i;
			var enumerator = source.GetEnumerator();

			int capacity = _capacity;
			memorySpan = new(_memoryBlock);
			if (enumerator.MoveNext())
			{
				int available = capacity - targetItemIdx;
				do
				{
					if (targetItemIdx + growByCount > capacity)
					{
						capacity = EnsureCapacity(this, capacity + growByCount);
						memorySpan = new(_memoryBlock);
						available = capacity - targetItemIdx;
					}

					for (i = 0; i < available; i++)
					{
						memorySpan[targetItemIdx++] = enumerator.Current;
						if (!enumerator.MoveNext())
						{
							break;
						}
					}
				}
				while (i >= available);
			}

			_count = targetItemIdx;
			return enumerator;
		}

		private void AddRangeWithKnownCount(IEnumerable<T> source, int targetItemIdx, int requiredAdditionalCapacity)
		{
			_ = EnsureCapacity(this, targetItemIdx + requiredAdditionalCapacity);

			Span<T> memorySpan = new(_memoryBlock);
			foreach (var item in source)
			{
				memorySpan[targetItemIdx++] = item;
			}

			_count = targetItemIdx;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static T[] Resize(in T[]? source, int newSize)
		{
			ArrayPool<T> arrayPool = _arrayPool;
			T[] newMemoryBlock = newSize >= RecyclableDefaults.MinPooledArrayLength
				? arrayPool.Rent(newSize)
				: new T[newSize];

			if (source?.Length > 0)
			{
				Array.Copy(source!, 0, newMemoryBlock, 0, source!.Length);
				if (source!.Length >= RecyclableDefaults.MinPooledArrayLength)
				{
					arrayPool.Return(source!, NeedsClearing);
				}
			}

			return newMemoryBlock;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static int EnsureCapacity(RecyclableList<T> list, int requestedCapacity)
		{
			int newCapacity;
			switch (list._capacity > 0)
			{
				case true:
					newCapacity = list._capacity;
					while (newCapacity < requestedCapacity)
					{
						newCapacity <<= 1;
					}

					break;

				case false:
					newCapacity = requestedCapacity;
					break;
			}

			list._memoryBlock = Resize(list._memoryBlock, newCapacity);
			newCapacity = list._memoryBlock.Length;
			list._capacity = newCapacity;
			return newCapacity;
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableList()
#pragma warning restore CS8618
		{
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableList(int initialCapacity)
#pragma warning restore CS8618
		{
			if (initialCapacity > 0)
			{
				_memoryBlock = Resize(_memoryBlock, initialCapacity);
				_capacity = _memoryBlock.Length;
			}
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableList(RecyclableList<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableList(RecyclableLongList<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableList(ReadOnlySpan<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableList(in T[] source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableList(List<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableList(IList<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

		public RecyclableList(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.Capacity)
		{
			_memoryBlock = Resize(_memoryBlock, initialCapacity);
			_capacity = _memoryBlock.Length;
			AddRange(source);
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

		private int _capacity;
		public int Capacity
		{
			get => _capacity;
			set
			{
				if (_capacity != value)
				{
					_memoryBlock = Resize(_memoryBlock, value);
					_capacity = _memoryBlock.Length;
					_version++;
				}
			}
		}

		private int _count;
		public int Count => _count;

		public bool IsReadOnly => false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			int requiredCapacity = _count + 1;
			if (_capacity < requiredCapacity)
			{
				_ = EnsureCapacity(this, requiredCapacity);
			}

			_memoryBlock[_count++] = item;
			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(in T[] items)
		{
			if (items.LongLength == 0)
			{
				return;
			}

			int sourceItemsCount = items.Length;
			int targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(this, targetCapacity);
			}

			var targetSpan = new Span<T>(_memoryBlock, _count, sourceItemsCount);
			Span<T> itemsSpan = new(items);
			itemsSpan.CopyTo(targetSpan);
			_count = targetCapacity;

			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(ReadOnlySpan<T> itemsSpan)
		{
			if (itemsSpan.Length == 0)
			{
				return;
			}

			int sourceItemsCount = itemsSpan.Length;
			int targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(this, targetCapacity);
			}

			var targetSpan = new Span<T>(_memoryBlock, _count, sourceItemsCount);
			itemsSpan.CopyTo(targetSpan);
			_count = targetCapacity;

			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(List<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			var sourceItemsCount = items.Count;
			var targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(this, targetCapacity);
			}

			items.CopyTo(_memoryBlock, _count);
			_count = targetCapacity;

			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(IList<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			var sourceItemsCount = items.Count;
			var targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(this, targetCapacity);
			}

			items.CopyTo(_memoryBlock, _count);
			_count = targetCapacity;

			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(RecyclableList<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			var sourceItemsCount = items._count;
			var targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(this, targetCapacity);
			}

			var targetSpan = new Span<T>(_memoryBlock, _count, sourceItemsCount);
			Span<T> itemsSpan = new(items._memoryBlock, 0, sourceItemsCount);
			itemsSpan.CopyTo(targetSpan);
			_count = targetCapacity;

			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(RecyclableLongList<T> items)
		{
			if (items.LongCount == 0)
			{
				return;
			}

			var sourceItemsCount = items.LongCount;
			if (sourceItemsCount > int.MaxValue)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The number of items exceeds the maximum capacity of {nameof(RecyclableList<T>)}, equal {int.MaxValue}, equal {int.MaxValue}. Please consider using {nameof(RecyclableLongList<T>)}, instead");
			}

			if (_count + sourceItemsCount > int.MaxValue)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items in source and target table exceeds the maximum capacity of {nameof(RecyclableList<T>)}, equal {int.MaxValue}. Please consider using {nameof(RecyclableLongList<T>)}, instead");
			}

			int targetCapacity = _count + (int)sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(this, targetCapacity);
			}

			Span<T> targetSpan = new Span<T>(_memoryBlock)[_count..],
				itemsSpan;

			int blockIndex,
				sourceBlockSize = items.BlockSize,
				lastBlockIndex = (int)(sourceItemsCount / sourceBlockSize) - 1;

			for (blockIndex = 0; blockIndex <= lastBlockIndex; blockIndex++)
			{
				itemsSpan = new(items.AsArray[blockIndex], 0, sourceBlockSize);
				itemsSpan.CopyTo(targetSpan);
				targetSpan = targetSpan[sourceBlockSize..];
			}

			if (blockIndex == 0)
			{
				itemsSpan = new(items.AsArray[blockIndex], 0, (int)sourceItemsCount);
				itemsSpan.CopyTo(targetSpan);
			}
			else if (blockIndex <= items.LastBlockWithData)
			{
				itemsSpan = new(items.AsArray[blockIndex], 0, items.NextItemIndex);
				itemsSpan.CopyTo(targetSpan);
			}

			_count = targetCapacity;

			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(IEnumerable<T> source, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			if (source is T[] sourceArray)
			{
				AddRange(sourceArray);
			}
			else if (source is List<T> sourceList)
			{
				AddRange(sourceList);
			}
			else if (source is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(sourceRecyclableList);
			}
			else if (source is IList<T> sourceIList)
			{
				AddRange(sourceIList);
			}
			else if (source.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
			{
				AddRangeWithKnownCount(source, _count, requiredAdditionalCapacity);

				_version++;
			}
			else
			{
				AddRangeEnumerated(source, growByCount);

				_version++;
			}
		}

		public T[] AsArray => _memoryBlock;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Clear()
		{
			if (NeedsClearing)
			{
				Array.Clear(_memoryBlock, 0, _count);
			}

			_count = 0;
			_version++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item) => _count > 0 && Array.IndexOf(_memoryBlock, item, 0, _count) >= 0;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void CopyTo(T[] array, int index) => Array.Copy(_memoryBlock, 0, array, index, _count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T itemToFind) => _count > 0 ? Array.IndexOf(_memoryBlock, itemToFind, 0, _count) : RecyclableDefaults.ItemNotFoundIndex;

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void Insert(int index, T item)
		{
			int oldCount = _count;
			int requestedCapacity = oldCount + 1;
			if (_capacity < requestedCapacity)
			{
				_ = EnsureCapacity(this, requestedCapacity);
			}

			if (oldCount > 0)
			{
				var sourceSpan = new Span<T>(_memoryBlock, index, oldCount);
				var targetSpan = new Span<T>(_memoryBlock, index + 1, oldCount);
				sourceSpan.CopyTo(targetSpan);
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

				if (NeedsClearing)
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

			if (NeedsClearing)
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
			if (_capacity > 0)
			{
				Clear();
				if (_count is >= RecyclableDefaults.MinPooledArrayLength)
				{
					_arrayPool.Return(_memoryBlock, NeedsClearing);
				}
			}

			GC.SuppressFinalize(this);
		}
	}
}
