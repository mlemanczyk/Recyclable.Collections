using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableList<T> : IDisposable, IList<T>
	{
		private const int ItemNotFoundIndex = -1;
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
			long sourceItemsCount = items.LongLength;
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
			int memoryBlocksCount = memoryBlocksSpan.Length;
			Span<T> targetBlockSpan = new(memoryBlocksSpan[targetBlockIdx]);
			for (var i = 0L; i < sourceItemsCount; i++)
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
			_lastBlockIndex = targetBlockIdx - 1;
			_nextItemIndex = targetItemIdx;
		}

		public void AddRange(in List<T> items)
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
			_lastBlockIndex = targetBlockIdx - 1;
			_nextItemIndex = targetItemIdx;
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
			_lastBlockIndex = targetBlockIdx - 1;
			_nextItemIndex = targetItemIdx;
		}

		private void AddRange(IEnumerable<T> source, int growByCount = RecyclableDefaults.MinPooledArrayLength)
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
			_lastBlockIndex = targetBlockIdx - 1;
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

			int blockSize = _blockSize;
			int itemIndex = 0;
			int blockIndex = 0;
			int lastBlockIndex = 0;

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			Span<T> blockArraySpan = new(memoryBlocksSpan[blockIndex]);
			if (item != null)
			{
				while (blockIndex < lastBlockIndex)
				{
					if (item.Equals(blockArraySpan[itemIndex]))
					{
						return true;
					}

					if (itemIndex + 1 < blockSize && item.Equals(blockArraySpan[itemIndex + 1]))
					{
						return true;
					}
					
					if (itemIndex + 2 < blockSize && item.Equals(blockArraySpan[itemIndex + 2]))
					{
						return true;
					}
					
					if (itemIndex + 3 < blockSize && item.Equals(blockArraySpan[itemIndex + 3]))
					{
						return true;
					}

					if (itemIndex + 4 < blockSize && item.Equals(blockArraySpan[itemIndex + 4]))
					{
						return true;
					}

					if (itemIndex + 5 < blockSize && item.Equals(blockArraySpan[itemIndex + 5]))
					{
						return true;
					}

					if (itemIndex + 6 < blockSize && item.Equals(blockArraySpan[itemIndex + 6]))
					{
						return true;
					}

					if (itemIndex + 7 < blockSize && item.Equals(blockArraySpan[itemIndex + 7]))
					{
						return true;
					}

					itemIndex += 8;
					if (itemIndex >= blockSize)
					{
						blockIndex++;
						blockArraySpan = new(memoryBlocksSpan[blockIndex]);
						itemIndex = 0;
					}
				}

				blockArraySpan = new(memoryBlocksSpan[lastBlockIndex]);
				var nextItemIndex = _nextItemIndex;
				while (itemIndex < nextItemIndex)
				{
					if (item.Equals(blockArraySpan[itemIndex]))
					{
						return true;
					}

					if (itemIndex + 1 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 1]))
					{
						return true;
					}

					if (itemIndex + 2 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 2]))
					{
						return true;
					}

					if (itemIndex + 3 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 3]))
					{
						return true;
					}

					if (itemIndex + 4 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 4]))
					{
						return true;
					}

					if (itemIndex + 5 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 5]))
					{
						return true;
					}

					if (itemIndex + 6 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 6]))
					{
						return true;
					}

					if (itemIndex + 7 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 7]))
					{
						return true;
					}

					itemIndex += 8;
				}
			}
			else
			{
				while (blockIndex < lastBlockIndex)
				{
					if (blockArraySpan[itemIndex] == null)
					{
						return true;
					}

					if (itemIndex + 1 < blockSize && blockArraySpan[itemIndex + 1] == null)
					{
						return true;
					}

					if (itemIndex + 2 < blockSize && blockArraySpan[itemIndex + 2] == null)
					{
						return true;
					}

					if (itemIndex + 3 < blockSize && blockArraySpan[itemIndex + 3] == null)
					{
						return true;
					}

					if (itemIndex + 4 < blockSize && blockArraySpan[itemIndex + 4] == null)
					{
						return true;
					}

					if (itemIndex + 5 < blockSize && blockArraySpan[itemIndex + 5] == null)
					{
						return true;
					}

					if (itemIndex + 6 < blockSize && blockArraySpan[itemIndex + 6] == null)
					{
						return true;
					}

					if (itemIndex + 7 < blockSize && blockArraySpan[itemIndex + 7] == null)
					{
						return true;
					}

					itemIndex += 8;
					if (itemIndex >= blockSize)
					{
						blockIndex++;
						blockArraySpan = new(memoryBlocksSpan[blockIndex]);
						itemIndex = 0;
					}
				}

				blockArraySpan = new(memoryBlocksSpan[lastBlockIndex]);
				var nextItemIndex = _nextItemIndex;
				while (itemIndex < nextItemIndex)
				{
					if (blockArraySpan[itemIndex] == null)
					{
						return true;
					}

					if (itemIndex + 1 < nextItemIndex && blockArraySpan[itemIndex + 1] == null)
					{
						return true;
					}

					if (itemIndex + 2 < nextItemIndex && blockArraySpan[itemIndex + 2] == null)
					{
						return true;
					}

					if (itemIndex + 3 < nextItemIndex && blockArraySpan[itemIndex + 3] == null)
					{
						return true;
					}

					if (itemIndex + 4 < nextItemIndex && blockArraySpan[itemIndex + 4] == null)
					{
						return true;
					}

					if (itemIndex + 5 < nextItemIndex && blockArraySpan[itemIndex + 5] == null)
					{
						return true;
					}

					if (itemIndex + 6 < nextItemIndex && blockArraySpan[itemIndex + 6] == null)
					{
						return true;
					}

					if (itemIndex + 7 < nextItemIndex && blockArraySpan[itemIndex + 7] == null)
					{
						return true;
					}

					itemIndex += 8;
				}
			}

			return false;
		}

		public void CopyTo(T[] array, int arrayIndex) => _memoryBlocks.CopyTo(0, _blockSize, _longCount, array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		public int IndexOf(T item)
		{
			if (_longCount == 0)
			{
				return ItemNotFoundIndex;
			}

			int blockSize = _blockSize;
			int itemIndex = 0;
			int blockIndex = 0;
			int lastBlockIndex = 0;

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			Span<T> blockArraySpan = new(memoryBlocksSpan[blockIndex]);
			if (item != null)
			{
				while (blockIndex < lastBlockIndex)
				{
					if (item.Equals(blockArraySpan[itemIndex]))
					{
						return itemIndex;
					}

					if (itemIndex + 1 < blockSize && item.Equals(blockArraySpan[itemIndex + 1]))
					{
						return itemIndex + 1;
					}

					if (itemIndex + 2 < blockSize && item.Equals(blockArraySpan[itemIndex + 2]))
					{
						return itemIndex + 2;
					}

					if (itemIndex + 3 < blockSize && item.Equals(blockArraySpan[itemIndex + 3]))
					{
						return itemIndex + 3;
					}

					if (itemIndex + 4 < blockSize && item.Equals(blockArraySpan[itemIndex + 4]))
					{
						return itemIndex + 4;
					}

					if (itemIndex + 5 < blockSize && item.Equals(blockArraySpan[itemIndex + 5]))
					{
						return itemIndex + 5;
					}

					if (itemIndex + 6 < blockSize && item.Equals(blockArraySpan[itemIndex + 6]))
					{
						return itemIndex + 6;
					}

					if (itemIndex + 7 < blockSize && item.Equals(blockArraySpan[itemIndex + 7]))
					{
						return itemIndex + 7;
					}

					itemIndex += 8;
					if (itemIndex >= blockSize)
					{
						blockIndex++;
						blockArraySpan = new(memoryBlocksSpan[blockIndex]);
						itemIndex = 0;
					}
				}

				blockArraySpan = new(memoryBlocksSpan[lastBlockIndex]);
				var nextItemIndex = _nextItemIndex;
				while (itemIndex < nextItemIndex)
				{
					if (item.Equals(blockArraySpan[itemIndex]))
					{
						return itemIndex;
					}

					if (itemIndex + 1 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 1]))
					{
						return itemIndex + 1;
					}

					if (itemIndex + 2 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 2]))
					{
						return itemIndex + 2;
					}

					if (itemIndex + 3 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 3]))
					{
						return itemIndex + 3;
					}

					if (itemIndex + 4 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 4]))
					{
						return itemIndex + 4;
					}

					if (itemIndex + 5 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 5]))
					{
						return itemIndex + 5;
					}

					if (itemIndex + 6 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 6]))
					{
						return itemIndex + 6;
					}

					if (itemIndex + 7 < nextItemIndex && item.Equals(blockArraySpan[itemIndex + 7]))
					{
						return itemIndex + 7;
					}

					itemIndex += 8;
				}
			}
			else
			{
				while (blockIndex < lastBlockIndex)
				{
					if (blockArraySpan[itemIndex] == null)
					{
						return itemIndex;
					}

					if (itemIndex + 1 < blockSize && blockArraySpan[itemIndex + 1] == null)
					{
						return itemIndex + 1;
					}

					if (itemIndex + 2 < blockSize && blockArraySpan[itemIndex + 2] == null)
					{
						return itemIndex + 2;
					}

					if (itemIndex + 3 < blockSize && blockArraySpan[itemIndex + 3] == null)
					{
						return itemIndex + 3;
					}

					if (itemIndex + 4 < blockSize && blockArraySpan[itemIndex + 4] == null)
					{
						return itemIndex + 4;
					}

					if (itemIndex + 5 < blockSize && blockArraySpan[itemIndex + 5] == null)
					{
						return itemIndex + 5;
					}

					if (itemIndex + 6 < blockSize && blockArraySpan[itemIndex + 6] == null)
					{
						return itemIndex + 6;
					}

					if (itemIndex + 7 < blockSize && blockArraySpan[itemIndex + 7] == null)
					{
						return itemIndex + 7;
					}

					itemIndex += 8;
					if (itemIndex >= blockSize)
					{
						blockIndex++;
						blockArraySpan = new(memoryBlocksSpan[blockIndex]);
						itemIndex = 0;
					}
				}

				blockArraySpan = new(memoryBlocksSpan[lastBlockIndex]);
				var nextItemIndex = _nextItemIndex;
				while (itemIndex < nextItemIndex)
				{
					if (blockArraySpan[itemIndex] == null)
					{
						return itemIndex;
					}

					if (itemIndex + 1 < nextItemIndex && blockArraySpan[itemIndex + 1] == null)
					{
						return itemIndex + 1;
					}

					if (itemIndex + 2 < nextItemIndex && blockArraySpan[itemIndex + 2] == null)
					{
						return itemIndex + 2;
					}

					if (itemIndex + 3 < nextItemIndex && blockArraySpan[itemIndex + 3] == null)
					{
						return itemIndex + 3;
					}

					if (itemIndex + 4 < nextItemIndex && blockArraySpan[itemIndex + 4] == null)
					{
						return itemIndex + 4;
					}

					if (itemIndex + 5 < nextItemIndex && blockArraySpan[itemIndex + 5] == null)
					{
						return itemIndex + 5;
					}

					if (itemIndex + 6 < nextItemIndex && blockArraySpan[itemIndex + 6] == null)
					{
						return itemIndex + 6;
					}

					if (itemIndex + 7 < nextItemIndex && blockArraySpan[itemIndex + 7] == null)
					{
						return itemIndex + 7;
					}

					itemIndex += 8;
				}
			}

			return ItemNotFoundIndex;
		}

		public void Insert(int index, T item) => throw new NotSupportedException();
		public long LongIndexOf(T item) => _memoryBlocks.LongIndexOf(_blockSize, item, _equalityComparer);
		public bool Remove(T item) => throw new NotSupportedException();
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
