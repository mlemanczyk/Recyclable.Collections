using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableList<T> : IList<T>, IDisposable
	{
		private static readonly ArrayPool<T> _arrayPool = ArrayPool<T>.Create();

		protected static readonly bool NeedsClearing = !typeof(T).IsValueType;

		protected T[] _memoryBlock;

		private static void ThrowArgumentOutOfRangeException(in string message)
		{
			throw new ArgumentOutOfRangeException(message, (Exception?)null);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		protected static T[] Resize(in T[]? source, int newSize)
		{
			ArrayPool<T> arrayPool = _arrayPool;
			T[] newMemoryBlock = newSize >= RecyclableDefaults.MinPooledArrayLength
				? arrayPool.Rent(newSize)
				: new T[newSize];

			if (source?.Length > 0)
			{
				Array.Copy(source!, 0, newMemoryBlock, 0, source!.Length);
				//var sourceSpan = new Memory<T>(source);
				//var newArraySpan = new Memory<T>(newMemoryBlock);

				//sourceSpan.CopyTo(newArraySpan);

				if (source!.Length >= RecyclableDefaults.MinPooledArrayLength)
				{
					arrayPool.Return(source!, NeedsClearing);
				}
			}

			return newMemoryBlock;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		protected static int EnsureCapacity(RecyclableList<T> list, int requestedCapacity)
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
			set => _memoryBlock[index] = value;
		}

		protected int _capacity;
		public int Capacity
		{
			get => _capacity;
			set
			{
				if (_capacity != value)
				{
					_memoryBlock = Resize(_memoryBlock, value);
					_capacity = _memoryBlock.Length;
				}
			}
		}

		protected int _count;
		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _count = value;
		}

		public bool IsReadOnly { get; } = false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			int requiredCapacity = _count + 1;
			if (_capacity < requiredCapacity)
			{
				_ = EnsureCapacity(this, requiredCapacity);
			}

			_memoryBlock[_count++] = item;
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
				ThrowArgumentOutOfRangeException($"The number of items exceeds the maximum capacity of {nameof(RecyclableList<T>)}, equal {int.MaxValue}, equal {int.MaxValue}. Please consider using {nameof(RecyclableLongList<T>)}, instead");
			}

			if (_count + sourceItemsCount > int.MaxValue)
			{
				ThrowArgumentOutOfRangeException($"The total number of items in source and target table exceeds the maximum capacity of {nameof(RecyclableList<T>)}, equal {int.MaxValue}. Please consider using {nameof(RecyclableLongList<T>)}, instead");
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
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
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

			if (source is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(sourceRecyclableList);
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
				_ = EnsureCapacity(this, targetItemIdx + requiredAdditionalCapacity);
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
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item) => _count > 0 && Array.IndexOf(_memoryBlock, item, 0, _count) >= 0;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void CopyTo(T[] array, int arrayIndex) => Array.Copy(_memoryBlock, 0, array, arrayIndex, _count);

		protected static IEnumerable<T> Enumerate(RecyclableList<T> list)
		{
			int count = list._count;
			var memory = list._memoryBlock;
			for (var i = 0; i < count; i++)
			{
				yield return memory[i];
			}
		}

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
#pragma warning disable CS8601 // In real use cases we'll never access it
					_memoryBlock[_count] = default;
#pragma warning restore CS8601
				}

				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void RemoveAt(int index)
		{
			if (index >= _count)
			{
				ThrowArgumentOutOfRangeException($"Argument \"{nameof(index)}\" = {index} is out of range");
			}

			_count--;
			if (index < _count)
			{
				Array.Copy(_memoryBlock, index + 1, _memoryBlock, index, _count - index);
			}

			if (NeedsClearing)
			{
#pragma warning disable CS8601 // In real use cases we'll never access it
				_memoryBlock[_count] = default;
#pragma warning restore CS8601
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator() => Enumerate(this).GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator() => Enumerate(this).GetEnumerator();

		public void Dispose()
		{
			if (_capacity > 0)
			{
				Clear();
				_capacity = 0;
				_memoryBlock.ReturnToPool(_arrayPool, NeedsClearing);
				GC.SuppressFinalize(this);
			}
		}
	}
}
