using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableList<T> : IDisposable, IList<T>
	{
		private const int ItemNotFoundIndex = -1;
		private const int BatchSize = 8;

		private static readonly ArrayPool<T[]> _defaultMemoryBlocksPool = ArrayPool<T[]>.Create();
		private static readonly ArrayPool<T> _defaultBlockArrayPool = ArrayPool<T>.Create();
		private static readonly T[][] _emptyMemoryBlocksArray = new T[0][];
		private static readonly T[] _emptyBlockArray = new T[0];
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;

		private int _blockSize;
		private int _lastBlockIndex;
		private int _nextItemIndex;
		protected ArrayPool<T[]> _memoryBlocksPool;
		protected ArrayPool<T> _blockArrayPool;
		protected T[][] _memoryBlocks;

		private long _capacity;
		public long Capacity
		{
			get => _capacity;
			protected set => _capacity = value;
		}

		public int Count => (int)_longCount;

		protected long _longCount;
		public long LongCount
		{
			get => _longCount;
			set => _longCount = value;
		}

		public bool IsReadOnly { get; }
		public int BlockCount => _memoryBlocks.Length;

		private static void RemoveAt(RecyclableList<T> list, long index, ArrayPool<T> blockArrayPool)
		{
			long oldCountMinus1 = list._longCount - 1;
			if (index != oldCountMinus1)
			{
				ThrowArgumentOutOfRangeException();
			}

			list._longCount--;
			int blockSize = list._blockSize;
			if (list._nextItemIndex > 0)
			{
				list._nextItemIndex--;
			}
			else
			{
				list._lastBlockIndex--;
				list._nextItemIndex = blockSize;
			}

			if ((list._capacity * blockSize) - oldCountMinus1 == blockSize)
			{
				T[][] memoryBlocks = list._memoryBlocks;
				if (blockSize >= RecyclableDefaults.MinPooledArrayLength)
				{
					blockArrayPool.Return(memoryBlocks[list.BlockCount - 1]);
				}

				list._capacity -= blockSize;
			}
		}

		private static void ThrowArgumentOutOfRangeException()
		{
			throw new ArgumentOutOfRangeException("index");
		}

		protected static long DoIndexOf(T item, in T[][] memoryBlocks, int lastBlockIndex, int blockSize, int nextItemIndex)
		{
			Span<T[]> memoryBlocksSpan = new(memoryBlocks);
			int itemIndex;
			for (var blockIndex = 0; blockIndex < lastBlockIndex; blockIndex++)
			{
				itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex], item, 0, blockSize);
				if (itemIndex >= 0)
				{
					return itemIndex + (blockIndex * blockSize);
				}
			}

			itemIndex = Array.IndexOf(memoryBlocksSpan[lastBlockIndex], item, 0, nextItemIndex);
			return itemIndex >= 0 ? itemIndex + (lastBlockIndex * blockSize) : itemIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static T[][] SetNewLength(in T[][]? source, int minBlockSize, in long newCapacity, ArrayPool<T[]> memoryBlocksPool, ArrayPool<T> blockArrayPool)
		{
			const int sourceBlockCount = 0, requiredBlockCount = 1, uninitializedBlocksCount = 2, i = 3;

			Span<int> localInts = stackalloc int[4]
			{
				source?.Length ?? 0, // sourceBlockCount
				(int)(newCapacity / minBlockSize) + (newCapacity % minBlockSize > 0 ? 1 : 0), // requiredBlockCount
				0, // uninitializedBlocksCount
				0, // i
			};

			T[][] newMemoryBlocks = localInts[requiredBlockCount] < RecyclableDefaults.MinPooledArrayLength
				? new T[localInts[requiredBlockCount]][]
				: memoryBlocksPool.Rent(localInts[requiredBlockCount]);

			Span<T[]> newMemoryBlocksSpan = new(newMemoryBlocks);
			if (localInts[sourceBlockCount] > 0)
			{
				Span<T[]> sourceSpan = new(source);
				sourceSpan.CopyTo(newMemoryBlocksSpan);
				if (localInts[sourceBlockCount] >= RecyclableDefaults.MinPooledArrayLength)
				{
					memoryBlocksPool.Return(source!);
				}

				newMemoryBlocksSpan = newMemoryBlocksSpan[localInts[sourceBlockCount]..];
			}

			localInts[uninitializedBlocksCount] = newMemoryBlocksSpan.Length;
			if (localInts[uninitializedBlocksCount] == localInts[requiredBlockCount])
			{
				if (minBlockSize >= RecyclableDefaults.MinPooledArrayLength)
				{
					newMemoryBlocksSpan[0] = blockArrayPool.Rent(minBlockSize);
					minBlockSize = newMemoryBlocksSpan[0].Length;
				}
				else
				{
					newMemoryBlocksSpan[0] = new T[minBlockSize];
				}

				newMemoryBlocksSpan = newMemoryBlocksSpan[1..];
				localInts[uninitializedBlocksCount]--;
			}

			if (minBlockSize >= RecyclableDefaults.MinPooledArrayLength)
			{
				for (localInts[i] = 0; localInts[i] < localInts[uninitializedBlocksCount]; localInts[i]++)
				{
					newMemoryBlocksSpan[localInts[i]] = blockArrayPool.Rent(minBlockSize);
				}
			}
			else
			{
				for (localInts[i] = 0; localInts[i] < localInts[uninitializedBlocksCount]; localInts[i]++)
				{
					newMemoryBlocksSpan[localInts[i]] = new T[minBlockSize];
				}
			}

			return newMemoryBlocks;
		}

		protected long EnsureCapacity(in long requestedCapacity)
		{
			T[][] memory = _memoryBlocks;
			long newCapacity;

			if (_capacity > 0)
			{
				newCapacity = _capacity;
				while (newCapacity < requestedCapacity)
				{
					newCapacity *= 2;
				}
			}
			else
			{
				newCapacity = requestedCapacity;
			}

			memory = SetNewLength(memory, _blockSize, newCapacity, _memoryBlocksPool, _blockArrayPool);
			if (newCapacity > 0 && _capacity == 0)
			{
				_blockSize = memory[0].Length;
			}

			newCapacity = memory.Length * _blockSize;
			_capacity = newCapacity;
			_memoryBlocks = memory;
			return newCapacity;
		}

		public RecyclableList(int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
		{
			memoryBlocksPool ??= _defaultMemoryBlocksPool;
			blockArrayPool ??= _defaultBlockArrayPool;
			_memoryBlocksPool = memoryBlocksPool;
			_blockArrayPool = blockArrayPool;

			if (expectedItemsCount > 0)
			{
				_memoryBlocks = SetNewLength(_memoryBlocks, minBlockSize, expectedItemsCount.Value, memoryBlocksPool, blockArrayPool);
				if (_memoryBlocks.Length > 0)
				{
					minBlockSize = _memoryBlocks[0].Length;
				}

				_blockSize = minBlockSize;
				_capacity = _memoryBlocks.Length * minBlockSize;
			}
			else
			{
				_blockSize = minBlockSize;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}
		}

		public RecyclableList(IEnumerable<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
		{
			memoryBlocksPool ??= _defaultMemoryBlocksPool;
			blockArrayPool ??= _defaultBlockArrayPool;
			_memoryBlocksPool = memoryBlocksPool;
			_blockArrayPool = blockArrayPool;
			if (expectedItemsCount > 0)
			{
				_memoryBlocks = SetNewLength(_memoryBlocks, minBlockSize, expectedItemsCount.Value, memoryBlocksPool, blockArrayPool);
				if (_memoryBlocks.Length > 0)
				{
					minBlockSize = _memoryBlocks[0].Length;
				}

				_blockSize = minBlockSize;
				_capacity = _memoryBlocks.Length * minBlockSize;
			}
			else
			{
				_blockSize = minBlockSize;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public T this[long index]
		{
			get => _memoryBlocks[(int)(index / _blockSize)][index % _blockSize];
			set => new Span<T>(_memoryBlocks[(int)(index / _blockSize)])[(int)(index % _blockSize)] = value;
		}

		public T this[int index]
		{
			get => _memoryBlocks[index / _blockSize][index % _blockSize];
			set => new Span<T>(_memoryBlocks[index / _blockSize])[index % _blockSize] = value;
		}

		public T[][] MemoryBlocks { get => _memoryBlocks; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			if (_capacity < _longCount + 1)
			{
				_ = EnsureCapacity(_longCount + 1);
			}

			_memoryBlocks[_lastBlockIndex][_nextItemIndex++] = item;
			if (_nextItemIndex == _blockSize)
			{
				_lastBlockIndex++;
				_nextItemIndex = 0;
			}

			_longCount++;
		}

		public void AddRange(in T[] items)
		{
			long targetCapacity = _longCount + items.LongLength;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize, targetBlockIndex = _lastBlockIndex;
			Span<T> itemsSpan = new(items);
			Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, _memoryBlocks.Length);
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			while (targetBlockArraySpan.Length <= itemsSpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				targetBlockIndex++;
				targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], 0, blockSize);
			}

			_nextItemIndex = itemsSpan.Length;
			itemsSpan.CopyTo(targetBlockArraySpan);
			_longCount = targetCapacity;
			_lastBlockIndex = targetBlockIndex;
		}

		public void AddRange(RecyclableArrayList<T> items)
		{
			long targetCapacity = _longCount + items.Count;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize, targetBlockIndex = _lastBlockIndex;
			Span<T> itemsSpan = new(items.AsArray(), 0, items.Count);
			Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, _memoryBlocks.Length);
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			while (targetBlockArraySpan.Length <= itemsSpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				targetBlockIndex++;
				targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], 0, blockSize);
			}

			_nextItemIndex = itemsSpan.Length;
			itemsSpan.CopyTo(targetBlockArraySpan);
			_longCount = targetCapacity;
			_lastBlockIndex = targetBlockIndex;
		}

		public void AddRange(in RecyclableList<T> items)
		{
			long itemsCount = items.LongCount;
			long targetCapacity = _longCount + itemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize,
				sourceBlockIndex = 0,
				targetBlockIndex = _lastBlockIndex,
				toCopy = 0;

			Span<T[]> sourceMemoryBlocksSpan = new(items._memoryBlocks);
			Span<T> itemsSpan = new(sourceMemoryBlocksSpan[sourceBlockIndex], 0, items._blockSize);
			Span<T[]> targetMemoryBlocksSpan = new(_memoryBlocks);
			Span<T> targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			long copiedCount = 0;
			while (copiedCount < itemsCount)
			{
				toCopy = (int)Math.Min(itemsCount - copiedCount, itemsSpan.Length);
				if (targetBlockArraySpan.Length < toCopy)
				{
					toCopy = targetBlockArraySpan.Length;
					itemsSpan[..toCopy].CopyTo(targetBlockArraySpan);
					targetBlockIndex++;
					targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex], 0, blockSize);
					itemsSpan = itemsSpan[toCopy..];
					copiedCount += toCopy;
				}
				else
				{
					itemsSpan[..toCopy].CopyTo(targetBlockArraySpan);
					targetBlockArraySpan = targetBlockArraySpan[toCopy..];
					itemsSpan = itemsSpan[toCopy..];
					if (itemsSpan.IsEmpty && sourceBlockIndex + 1 < sourceMemoryBlocksSpan.Length)
					{
						sourceBlockIndex++;
						itemsSpan = new(sourceMemoryBlocksSpan[sourceBlockIndex], 0, items._blockSize);
					}

					copiedCount += toCopy;
					if (targetBlockArraySpan.IsEmpty && copiedCount < itemsCount)
					{
						targetBlockIndex++;
						targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex]);

					}
				}
			}

			if (targetCapacity % blockSize == 0)
			{
				_nextItemIndex = 0;
				_lastBlockIndex = targetBlockIndex + 1;
			}
			else
			{
				_nextItemIndex = (int)(targetCapacity % blockSize);
				_lastBlockIndex = targetBlockIndex;

			}

			_longCount = targetCapacity;

		}

		public void AddRange(List<T> items)
		{
			long targetCapacity = _longCount + items.Count;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize, targetBlockIndex = _lastBlockIndex, toCopy = 0,
				copiedCount = 0, targetItemIndex = _nextItemIndex, totalItemsCount = items.Count;

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			T[] targetBlockArray = memoryBlocksSpan[targetBlockIndex];
			while (true)
			{
				if (blockSize - targetItemIndex <= totalItemsCount - copiedCount)
				{
					toCopy = blockSize - targetItemIndex;
					items.CopyTo(copiedCount, targetBlockArray, targetItemIndex, toCopy);
					targetItemIndex = 0;
					targetBlockIndex++;
					if (targetBlockIndex > memoryBlocksSpan.Length)
					{
						break;
					}

					targetBlockArray = memoryBlocksSpan[targetBlockIndex];
					copiedCount += toCopy;
				}
				else
				{
					toCopy = totalItemsCount - copiedCount;
					items.CopyTo(copiedCount, targetBlockArray, targetItemIndex, toCopy);
					targetItemIndex = toCopy;
					break;
				}
			}

			_longCount = targetCapacity;
			_lastBlockIndex = targetBlockIndex;
			_nextItemIndex = targetItemIndex;
		}

		public void AddRange(in IList<T> items)
		{
			long sourceItemsCount = items.Count;
			long oldLongCount = _longCount;
			long targetCapacity = oldLongCount + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize;
			int targetItemIdx = (int)(oldLongCount % blockSize);
			int targetBlockIdx = (int)(oldLongCount / blockSize) + (targetItemIdx > 0 ? 1 : 0);
			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			var memoryBlocksCount = memoryBlocksSpan.Length;
			Span<T> targetBlockSpan = new(memoryBlocksSpan[targetBlockIdx]);
			for (int i = 0; i < sourceItemsCount; i++)
			{
				targetBlockSpan[targetItemIdx++] = items[i];
				if (targetItemIdx == blockSize)
				{
					targetBlockIdx++;
					if (targetBlockIdx == memoryBlocksCount)
					{
						break;
					}

					targetBlockSpan = new Span<T>(memoryBlocksSpan[targetBlockIdx]);
					targetItemIdx = 0;
				}
			}

			_longCount = targetCapacity;
			_lastBlockIndex = targetBlockIdx;
			_nextItemIndex = targetItemIdx;
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

			long capacity = _capacity;
			long oldLongCount = _longCount;
			int blockSize = _blockSize;
			int targetItemIdx = (int)(oldLongCount % blockSize);
			int targetBlockIdx = (targetItemIdx / blockSize) + (targetItemIdx > 0 ? 1 : 0);

			Span<T[]> memoryBlocksSpan;
			Span<T> blockArraySpan;
			if (source.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
			{

				int requiredCapacity = targetItemIdx + requiredAdditionalCapacity;
				if (capacity < requiredCapacity)
				{
					_ = EnsureCapacity(requiredCapacity);
					blockSize = _blockSize;
					targetBlockIdx = (targetItemIdx / blockSize) + (targetItemIdx > 0 ? 1 : 0);
					targetItemIdx = (int)(oldLongCount % blockSize);
				}

				memoryBlocksSpan = new(_memoryBlocks);
				var memoryBlocksCount = memoryBlocksSpan.Length;
				blockArraySpan = new Span<T>(memoryBlocksSpan[targetBlockIdx]);
				foreach (var item in source)
				{
					blockArraySpan[targetItemIdx++] = item;
					if (targetItemIdx == blockSize)
					{
						targetBlockIdx++;
						if (targetBlockIdx == memoryBlocksCount)
						{
							break;
						}

						targetItemIdx = 0;
						blockArraySpan = new Span<T>(memoryBlocksSpan[targetBlockIdx]);
					}
				}

				_longCount = (targetBlockIdx * blockSize) + targetItemIdx;
				_lastBlockIndex = targetBlockIdx;
				_nextItemIndex = targetItemIdx;
				return;
			}

			long i;
			using var enumerator = source.GetEnumerator();

			if (enumerator.MoveNext())
			{
				long available = capacity - targetItemIdx;
				memoryBlocksSpan = new(_memoryBlocks);
				var memoryBlocksCount = memoryBlocksSpan.Length;
				while (true)
				{
					if (targetItemIdx + growByCount > capacity)
					{
						capacity = EnsureCapacity(capacity + growByCount);
						memoryBlocksSpan = new(_memoryBlocks);
						memoryBlocksCount = memoryBlocksSpan.Length;
						available = capacity - targetItemIdx;
					}

					blockArraySpan = memoryBlocksSpan[targetBlockIdx];
					for (i = 0; i < available; i++)
					{
						blockArraySpan[targetItemIdx++] = enumerator.Current;
						if (!enumerator.MoveNext())
						{
							break;
						}

						if (targetItemIdx == blockSize)
						{
							targetBlockIdx++;
							if (targetBlockIdx == memoryBlocksCount)
							{
								break;
							}

							targetItemIdx = 0;
							blockArraySpan = new(memoryBlocksSpan[targetBlockIdx]);
						}
					}

					if (i < available)
					{
						break;
					}
				}
			}

			_longCount = targetItemIdx;
			_lastBlockIndex = targetBlockIdx;
			_nextItemIndex = targetItemIdx;
		}

		public void Clear()
		{
			if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
			{
				Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
				int memoryBlocksCount = memoryBlocksSpan.Length;
				ArrayPool<T> blockArrayPool = _blockArrayPool;
				for (int toRemoveIdx = 0; toRemoveIdx < memoryBlocksCount; toRemoveIdx++)
				{
					blockArrayPool.Return(memoryBlocksSpan[toRemoveIdx]);
				}
			}

			_capacity = 0;
			_lastBlockIndex = 0;
			_nextItemIndex = 0;
			_longCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item)
		{
			if (_longCount == 0)
			{
				return false;
			}

			if (_lastBlockIndex == 0 || (_lastBlockIndex == 1 && _nextItemIndex == 0))
			{
				return Array.IndexOf(_memoryBlocks[0], item, 0, (int)_longCount) >= 0;
			}

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			int lastBlockIndex = _lastBlockIndex;
			for (int blockIndex = 0; blockIndex < lastBlockIndex; blockIndex++)
			{
				if (Array.IndexOf(memoryBlocksSpan[blockIndex], item, 0, _blockSize) >= 0)
				{
					return true;
				}
			}

			return Array.IndexOf(memoryBlocksSpan[lastBlockIndex], item, 0, _nextItemIndex) >= 0;
		}

		public void CopyTo(T[] array, int arrayIndex) => _memoryBlocks.CopyTo(0, _blockSize, _longCount, array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T item) => _longCount == 0
				? ItemNotFoundIndex
				: _lastBlockIndex == 0 || (_lastBlockIndex == 1 && _nextItemIndex == 0)
				? Array.IndexOf(_memoryBlocks[0], item, 0, (int)_longCount)
				: (int)DoIndexOf(item, _memoryBlocks, _lastBlockIndex, _blockSize, _nextItemIndex);

		public void Insert(int index, T item) => throw new NotSupportedException();
		public long LongIndexOf(T item) => _longCount == 0
				? ItemNotFoundIndex
				: _lastBlockIndex == 0 || (_lastBlockIndex == 1 && _nextItemIndex == 0)
				? Array.IndexOf(_memoryBlocks[0], item, 0, (int)_longCount)
				: DoIndexOf(item, _memoryBlocks, _lastBlockIndex, _blockSize, _nextItemIndex);

		public bool Remove(T item)
		{
			var itemIndex = LongIndexOf(item);
			if (itemIndex >= 0)
			{
				RemoveAt(this, itemIndex, _defaultBlockArrayPool);
				return true;
			}

			return false;
		}

		public void RemoveBlock(int index)
		{
			if (_blockSize < RecyclableDefaults.MinPooledArrayLength)
			{
				_memoryBlocks[index] = _emptyBlockArray;
			}
			else
			{
				_blockArrayPool.Return(_memoryBlocks[index]);
			}

			_capacity -= _blockSize;
			if (_lastBlockIndex == index)
			{
				_nextItemIndex = 0;
			}
			else if (_lastBlockIndex > index)
			{
				_lastBlockIndex--;
			}
		}

		public void RemoveAt(int index) => RemoveAt(this, index, _blockArrayPool);
		public void RemoveAt(long index) => RemoveAt(this, index, _blockArrayPool);

		IEnumerator IEnumerable.GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		public void Dispose()
		{
			if (_capacity > 0)
			{
				Clear();
				if (_memoryBlocks.Length >= RecyclableDefaults.MinPooledArrayLength)
				{
					_memoryBlocksPool.Return(_memoryBlocks);
				}

				GC.SuppressFinalize(this);
			}
		}
	}
}
