using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableArrayList<T> : IEnumerable<T>, IList<T>, IDisposable
	{
		private static readonly ArrayPool<T> _arrayPool = ArrayPool<T>.Create();
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;

		protected T[] _memoryBlock;

		private static void ThrowArgumentOutOfRangeException(in string message)
		{
			throw new ArgumentOutOfRangeException(message, (Exception?)null);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static T[] SetNewLength(in T[]? source, in int newSize)
		{
			ArrayPool<T> arrayPool = _arrayPool;
			var sourceLength = source?.Length ?? 0;
			T[] newMemoryBlock = newSize >= RecyclableDefaults.MinPooledArrayLength
				? arrayPool.Rent(newSize)
				: new T[newSize];

			if (sourceLength > 0)
			{
				var sourceSpan = new Memory<T>(source);
				var newArraySpan = new Memory<T>(newMemoryBlock);
				sourceSpan.CopyTo(newArraySpan);
				if (sourceLength >= RecyclableDefaults.MinPooledArrayLength)
				{
					arrayPool.Return(source!);
				}
			}

			return newMemoryBlock;
		}

		protected int EnsureCapacity(in int requestedCapacity)
		{
			int oldCapacity = _capacity;
			ref T[] memory = ref _memoryBlock;

			int newCapacity;
			switch (oldCapacity > 0)
			{
				case true:
					newCapacity = oldCapacity;
					while (newCapacity < requestedCapacity)
					{
						newCapacity *= 2;
					}

					break;

				case false:
					newCapacity = requestedCapacity;
					break;
			}

			memory = SetNewLength(memory, newCapacity);
			newCapacity = memory.Length;
			_capacity = newCapacity;
			return newCapacity;
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList()
#pragma warning restore CS8618
		{
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(int initialCapacity)
#pragma warning restore CS8618
		{
			if (initialCapacity > 0)
			{
				_memoryBlock = SetNewLength(_memoryBlock, initialCapacity);
				_capacity = _memoryBlock.Length;
			}
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(RecyclableArrayList<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(in T[] source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(List<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(IList<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

		public RecyclableArrayList(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.Capacity)
		{
			_memoryBlock = SetNewLength(_memoryBlock, initialCapacity);
			_capacity = _memoryBlock.Length;
			AddRange(source);
		}

		public T this[int index]
		{
			get => _memoryBlock[index];
			set => new Span<T>(_memoryBlock)[index] = value;
		}

		protected int _capacity;
		public int Capacity 
		{
			get => _capacity;
			set
			{
				if (_capacity != value)
				{
					_memoryBlock = SetNewLength(_memoryBlock, value);
					_capacity = _memoryBlock.Length;
				}
			}
		}

		protected int _count;
		public int Count
		{
			get => _count;
			set => _count = value;
		}

		public bool IsReadOnly { get; } = false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			int requiredCapacity = _count + 1;
			if (_capacity < requiredCapacity)
			{
				_ = EnsureCapacity(requiredCapacity);
			}

			_memoryBlock[_count++] = item;
		}

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
				_ = EnsureCapacity(targetCapacity);
			}

			var targetSpan = new Span<T>(_memoryBlock)[_count..];
			Span<T> itemsSpan = new(items);
			itemsSpan.CopyTo(targetSpan);
			_count = targetCapacity;
		}

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
				_ = EnsureCapacity(targetCapacity);
			}

			items.CopyTo(_memoryBlock, _count);
			_count = targetCapacity;
		}

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
				_ = EnsureCapacity(targetCapacity);
			}

			items.CopyTo(_memoryBlock, _count);
			_count = targetCapacity;
		}

		public void AddRange(RecyclableArrayList<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			var sourceItemsCount = items._count;
			var targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			var targetSpan = new Span<T>(_memoryBlock)[_count..];
			Span<T> itemsSpan = new(items._memoryBlock);
			itemsSpan.CopyTo(targetSpan);
			_count = targetCapacity;
		}

		public void AddRange(RecyclableList<T> items)
		{
			if (items.LongCount == 0)
			{
				return;
			}

			var sourceItemsCount = items.LongCount;
			if (sourceItemsCount > int.MaxValue)
			{
				ThrowArgumentOutOfRangeException($"The number of items exceeds the maximum capacity of {nameof(RecyclableArrayList<T>)}, equal {int.MaxValue}, equal {int.MaxValue}. Please consider using {nameof(RecyclableList<T>)}, instead");
			}

			if (_count + sourceItemsCount > int.MaxValue)
			{
				ThrowArgumentOutOfRangeException($"The total number of items in source and target table exceeds the maximum capacity of {nameof(RecyclableArrayList<T>)}, equal {int.MaxValue}. Please consider using {nameof(RecyclableList<T>)}, instead");
			}

			int targetCapacity = _count + (int)sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			Span<T> targetSpan = new Span<T>(_memoryBlock)[_count..],
				itemsSpan;

			int blockIndex,
				sourceBlockSize = items.BlockSize,
				lastBlockIndex = (int)(sourceItemsCount / sourceBlockSize) - 1;

			for (blockIndex = 0; blockIndex <= lastBlockIndex; blockIndex++)
			{
				itemsSpan = new(items.MemoryBlocks[blockIndex], 0, sourceBlockSize);
				itemsSpan.CopyTo(targetSpan);
				targetSpan = targetSpan[sourceBlockSize..];
			}

			if (blockIndex == 0)
			{
				itemsSpan = new(items.MemoryBlocks[blockIndex], 0, (int)sourceItemsCount);
				itemsSpan.CopyTo(targetSpan);
			}
			else if (blockIndex < items.BlockCount)
			{
				itemsSpan = new(items.MemoryBlocks[blockIndex], 0, items.NextItemIndex);
				itemsSpan.CopyTo(targetSpan);
			}

			_count = targetCapacity;
		}

		public void AddRange(IEnumerable<T> source, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			if (source is T[] sourceArray)
			{
				AddRange(sourceArray);
				return;
			}

			if (source is List<T> sourceList)
			{
				AddRange(sourceList);
				return;
			}

			if (source is RecyclableArrayList<T> sourceRecyclableArrayList)
			{
				AddRange(sourceRecyclableArrayList);
				return;
			}

			if (source is IList<T> sourceIList)
			{
				AddRange(sourceIList);
				return;
			}

			int targetItemIdx = _count;
			Span<T> memorySpan;
			if (source.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
			{
				_ = EnsureCapacity(targetItemIdx + requiredAdditionalCapacity);
				memorySpan = new(_memoryBlock);
				foreach (var item in source)
				{
					memorySpan[targetItemIdx++] = item;
				}

				_count = targetItemIdx;
				return;
			}

			int i;
			using var enumerator = source.GetEnumerator();

			int capacity = _capacity;
			memorySpan = new(_memoryBlock);
			if (enumerator.MoveNext())
			{
				int available = capacity - targetItemIdx;
				while (true)
				{
					if (targetItemIdx + growByCount > capacity)
					{
						capacity = EnsureCapacity(capacity + growByCount);
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

					if (i < available)
					{
						break;
					}
				}
			}

			_count = targetItemIdx;
		}

		public T[] AsArray() => _memoryBlock;

		public void Clear()
		{
			_count = 0;
		}

		public bool Contains(T item) => Array.IndexOf(_memoryBlock, 0, _count) >= 0;

		public void CopyTo(T[] array, int arrayIndex) => Array.Copy(_memoryBlock, 0, array, arrayIndex, _count);

		protected static IEnumerable<T> Enumerate(RecyclableArrayList<T> list)
		{
			int count = list._count;
			var memory = list._memoryBlock;
			for (var i = 0; i < count; i++)
			{
				yield return memory[i];
			}
		}

		public int IndexOf(T itemToFind)
		{
			Span<T> memorySpan = new(_memoryBlock);
			int itemCount = _count;
			var equalityComparer = _equalityComparer;
			for (var itemIdx = 0; itemIdx < itemCount; itemIdx++)
			{
				var item = memorySpan[itemIdx];
				if (equalityComparer.Equals(item, itemToFind))
				{
					return itemIdx;
				}
			}

			return -1;
		}

		public void Insert(int index, T item)
		{
			int oldCount = _count;
			int requestedCapacity = oldCount + 1;
			if (_capacity < requestedCapacity)
			{
				_ = EnsureCapacity(requestedCapacity);
			}

			if (oldCount > 0)
			{
				var sourceSpan = new Span<T>(_memoryBlock)[index..oldCount];
				var targetSpan = new Span<T>(_memoryBlock)[(index + 1)..];
				sourceSpan.CopyTo(targetSpan);
			}

			_memoryBlock[index] = item;
			_count++;
		}

		public bool Remove(T itemToRemove)
		{
			var itemIdx = IndexOf(itemToRemove);
			if (itemIdx >= 0)
			{
				RemoveAt(itemIdx);
				return true;
			}

			return false;
		}

		public void RemoveAt(int index)
		{
			int oldCount = _count;
			int oldCountMinus1 = oldCount - 1;
			if (index == oldCountMinus1)
			{
				_count--;
				return;
			}

			if (index >= oldCount)
			{
				ThrowArgumentOutOfRangeException($"Argument \"{index}\" is out of range. The current version supports removing the last element, only ");
				return;
			}

			_memoryBlock.CopyItems(index, oldCountMinus1, ref _memoryBlock);
			_count--;
		}

		public IEnumerator<T> GetEnumerator() => Enumerate(this).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Enumerate(this).GetEnumerator();

		public void Dispose()
		{
			if (_capacity > 0)
			{
				Clear();
				_capacity = 0;
				_memoryBlock.ReturnToPool(_arrayPool);
				GC.SuppressFinalize(this);
			}
		}
	}
}
