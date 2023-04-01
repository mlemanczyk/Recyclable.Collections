using System.Buffers;
using System.Collections;
using System.Numerics;
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

		private int _blockSize;
		private int _blockSizePow2Shift;
		private int _nextItemBlockIndex;
		private int _nextItemIndex;

		protected static readonly bool NeedsClearing = !typeof(T).IsValueType;

		protected ArrayPool<T[]> _memoryBlocksPool;
		protected ArrayPool<T> _blockArrayPool;
		protected T[][] _memoryBlocks;

		private long _capacity;
		public long Capacity
		{
			get => _capacity;
			protected set => _capacity = value;
		}

		public int Count => checked((int)_longCount);
		public bool IsReadOnly { get; }
		public int LastTakenBlockIndex => _nextItemBlockIndex - (_nextItemIndex > 0 ? 0 : 1);

		protected long _longCount;

		public long LongCount
		{
			get => _longCount;
			set => _longCount = value;
		}

		public int ReservedBlockCount => (int)(_capacity >> _blockSizePow2Shift) + ((_capacity & (_blockSize - 1)) > 0 ? 1 : 0);
		public int BlockSize => _blockSize;
		public int NextItemBlockIndex => _nextItemBlockIndex;
		public int NextItemIndex => _nextItemIndex;

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
				list._nextItemBlockIndex--;
				list._nextItemIndex = blockSize;
			}

			if (list._capacity - oldCountMinus1 == blockSize)
			{
				T[][] memoryBlocks = list._memoryBlocks;
				if (blockSize >= RecyclableDefaults.MinPooledArrayLength)
				{
					blockArrayPool.Return(memoryBlocks[list.ReservedBlockCount - 1], NeedsClearing);
				}

				list._capacity -= blockSize;
			}
		}

		private static void ThrowArgumentOutOfRangeException()
		{
			throw new ArgumentOutOfRangeException("index");
		}

		protected void AddRangeEnumerated(IEnumerable<T> source, int growByCount)
		{
			long i, copied = _longCount;

			int blockSize = _blockSize, targetItemIdx = _nextItemIndex, targetBlockIdx = _nextItemBlockIndex;

			using IEnumerator<T> enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return;
			}

			long capacity = _capacity;
			long available = capacity - copied;

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, ReservedBlockCount);
			Span<T> blockArraySpan;
			var memoryBlocksCount = memoryBlocksSpan.Length;
			while (true)
			{
				if (copied + growByCount > capacity)
				{
					capacity = EnsureCapacity(capacity + growByCount);
					memoryBlocksCount = _memoryBlocks.Length;
					if (blockSize != _blockSize)
					{
						blockSize = _blockSize;
					}

					memoryBlocksSpan = new(_memoryBlocks, 0, memoryBlocksCount);
					available = capacity - copied;
				}

				blockArraySpan = new(memoryBlocksSpan[targetBlockIdx], 0, blockSize);
				for (i = 1; i <= available; i++)
				{
					blockArraySpan[targetItemIdx++] = enumerator.Current;

					if (!enumerator.MoveNext())
					{
						if (targetItemIdx < blockSize)
						{
							_nextItemBlockIndex = targetBlockIdx;
							_nextItemIndex = targetItemIdx;
						}
						else
						{
							_nextItemBlockIndex = targetBlockIdx + 1;
							_nextItemIndex = 0;
						}

						_longCount += copied + i - _longCount;
						return;
					}

					if (targetItemIdx == blockSize)
					{
						targetBlockIdx++;
						targetItemIdx = 0;

						if (targetBlockIdx == memoryBlocksCount)
						{
							break;
						}

						blockArraySpan = new(memoryBlocksSpan[targetBlockIdx], 0, blockSize);
					}
				}

				copied += available;
			}
		}

		protected void AddRangeWithKnownCount(IEnumerable<T> source, int requiredAdditionalCapacity)
		{
			long copied = _longCount;
			int blockSize = _blockSize;
			int targetItemIdx = _nextItemIndex;
			int targetBlockIdx = _nextItemBlockIndex;

			Span<T[]> memoryBlocksSpan;
			Span<T> blockArraySpan;
			long requiredCapacity = copied + requiredAdditionalCapacity;
			if (_capacity < requiredCapacity)
			{
				_ = EnsureCapacity(requiredCapacity);
				blockSize = _blockSize;
			}

			var memoryBlocksCount = ReservedBlockCount;
			memoryBlocksSpan = new(_memoryBlocks, 0, memoryBlocksCount);
			blockArraySpan = new Span<T>(memoryBlocksSpan[targetBlockIdx], 0, blockSize);
			foreach (var item in source)
			{
				blockArraySpan[targetItemIdx++] = item;
				if (targetItemIdx == blockSize)
				{
					targetBlockIdx++;
					targetItemIdx = 0;
					if (targetBlockIdx == memoryBlocksCount)
					{
						break;
					}

					blockArraySpan = new Span<T>(memoryBlocksSpan[targetBlockIdx], 0, blockSize);
				}
			}

			_longCount = requiredCapacity;
			_nextItemBlockIndex = targetBlockIdx;
			_nextItemIndex = targetItemIdx;
			return;
		}

		protected static long DoIndexOf(T item, in T[][] memoryBlocks, int lastBlockIndex, int blockSize, int nextItemIndex)
		{
			Span<T[]> memoryBlocksSpan = new(memoryBlocks, 0, lastBlockIndex + 1);
			int itemIndex, blockIndex;
			for (blockIndex = 0; blockIndex < lastBlockIndex; blockIndex++)
			{
				itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex], item, 0, blockSize);
				if (itemIndex >= 0)
				{
					return itemIndex + (blockIndex * blockSize);
				}
			}

			if (blockIndex == lastBlockIndex)
			{
				itemIndex = nextItemIndex != 0
					? Array.IndexOf(memoryBlocksSpan[lastBlockIndex], item, 0, nextItemIndex)
					: Array.IndexOf(memoryBlocksSpan[lastBlockIndex], item, 0, blockSize);
			}
			else
			{
				return ItemNotFoundIndex;
			}

			return itemIndex >= 0 ? itemIndex + (lastBlockIndex * blockSize) : ItemNotFoundIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static long Resize(ref T[][]? source, int minBlockSize, long oldCapacity, long newCapacity, ArrayPool<T[]> memoryBlocksPool, ArrayPool<T> blockArrayPool)
		{
			int minBlockSizePowerToShift = 31 - BitOperations.LeadingZeroCount((uint)minBlockSize);
			int sourceBlockCount = (int)(oldCapacity >> minBlockSizePowerToShift);
			int requiredBlockCount = (int)(newCapacity >> minBlockSizePowerToShift) + ((newCapacity & (minBlockSize - 1)) > 0 ? 1 : 0);
			int blockIndex;
			T[][]? newMemoryBlocks;
			Span<T[]> memoryBlocksSpan;

			// Release excesive blocks if we're downsizing
			if (requiredBlockCount < sourceBlockCount && (minBlockSize >= RecyclableDefaults.MinPooledArrayLength))
			{
				memoryBlocksSpan = new(source, requiredBlockCount, sourceBlockCount - requiredBlockCount);
				blockIndex = 0;
				while (blockIndex < memoryBlocksSpan.Length)
				{
					blockArrayPool.Return(memoryBlocksSpan[blockIndex++]!, NeedsClearing);
				}
			}

			// Allocate new memory block for all arrays
			newMemoryBlocks = requiredBlockCount < RecyclableDefaults.MinPooledArrayLength
				? (new T[requiredBlockCount][])
				: memoryBlocksPool.Rent(requiredBlockCount);

			// Copy arrays from the old memory block for all arrays
			if (sourceBlockCount > 0)
			{
				Array.Copy(source!, newMemoryBlocks, sourceBlockCount);
				// We can now return the old memory block for all arrays itself
				if (sourceBlockCount >= RecyclableDefaults.MinPooledArrayLength)
				{
					memoryBlocksPool.Return(source!, true);
				}
			}

			source = newMemoryBlocks;

			// Allocate arrays for any new blocks
			if (requiredBlockCount > sourceBlockCount)
			{
				memoryBlocksSpan = new(newMemoryBlocks, sourceBlockCount, newMemoryBlocks.Length - sourceBlockCount);
				if (minBlockSize >= RecyclableDefaults.MinPooledArrayLength)
				{
					if (sourceBlockCount == 0)
					{
						memoryBlocksSpan[0] = blockArrayPool.Rent(minBlockSize);
						minBlockSize = memoryBlocksSpan[0].Length;
						blockIndex = 1;
					}
					else
					{
						blockIndex = 0;
					}

					while (blockIndex < memoryBlocksSpan.Length)
					{
						memoryBlocksSpan[blockIndex++] = blockArrayPool.Rent(minBlockSize);
					}
				}
				else
				{
					blockIndex = 0;
					while (blockIndex < memoryBlocksSpan.Length)
					{
						memoryBlocksSpan[blockIndex++] = new T[minBlockSize];
					}
				}
			}

			return newMemoryBlocks.Length << minBlockSizePowerToShift;
		}

		protected long EnsureCapacity(long requestedCapacity)
		{
			long newCapacity = _capacity > 0
				? checked((long)BitOperations.RoundUpToPowerOf2((ulong)requestedCapacity))
				: requestedCapacity;

			newCapacity = Resize(ref _memoryBlocks!, _blockSize, _capacity, newCapacity, _memoryBlocksPool, _blockArrayPool);
			if (newCapacity > 0 && _capacity == 0)
			{
				_blockSize = _memoryBlocks[0].Length;
				_blockSizePow2Shift = MathUtils.GetPow2Shift(_blockSize);
			}

			_capacity = newCapacity;
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
				_capacity = Resize(ref _memoryBlocks!, (int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize), 0, expectedItemsCount.Value, memoryBlocksPool, blockArrayPool);
				if (_memoryBlocks.Length > 0)
				{
					minBlockSize = _memoryBlocks[0].Length;
				}

				_blockSize = minBlockSize;
				_blockSizePow2Shift = MathUtils.GetPow2Shift(minBlockSize);
			}
			else
			{
				_blockSize = (int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize);
				_blockSizePow2Shift = MathUtils.GetPow2Shift(minBlockSize);
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
				_capacity = Resize(ref _memoryBlocks!, (int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize), 0, expectedItemsCount.Value, memoryBlocksPool, blockArrayPool);
				if (_memoryBlocks.Length > 0)
				{
					minBlockSize = _memoryBlocks[0].Length;
				}

				_blockSize = minBlockSize;
				_blockSizePow2Shift = MathUtils.GetPow2Shift(minBlockSize);
			}
			else
			{
				_blockSize = (int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize);
				_blockSizePow2Shift = MathUtils.GetPow2Shift(minBlockSize);
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public T this[long index]
		{
			get => _memoryBlocks[index >> _blockSizePow2Shift][index & (_blockSize - 1)];
			set => new Span<T>(_memoryBlocks[(int)(index >> _blockSizePow2Shift)])[(int)(index & (_blockSize - 1))] = value;
		}

		public T this[int index]
		{
			get => _memoryBlocks[index >> _blockSizePow2Shift][index & (_blockSize - 1)];
			set => new Span<T>(_memoryBlocks[index >> _blockSizePow2Shift])[index & (_blockSize - 1)] = value;
		}

		public T[][] MemoryBlocks { get => _memoryBlocks; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			if (_capacity < _longCount + 1)
			{
				_ = EnsureCapacity(_longCount + 1);
			}

			_memoryBlocks[_nextItemBlockIndex][_nextItemIndex++] = item;
			if (_nextItemIndex == _blockSize)
			{
				_nextItemBlockIndex++;
				_nextItemIndex = 0;
			}

			_longCount++;
		}

		public void AddRange(in T[] items)
		{
			if (items.LongLength == 0)
			{
				return;
			}

			long targetCapacity = _longCount + items.LongLength;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _memoryBlocks.Length;

			Span<T> itemsSpan = new(items);
			Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, memoryBlockCount);
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			while (targetBlockArraySpan.Length <= itemsSpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				targetBlockIndex++;
				if (targetBlockIndex == memoryBlockCount)
				{
					break;
				}

				targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], 0, blockSize);
			}

			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(targetBlockArraySpan);
			}

			_nextItemIndex = itemsSpan.Length;
			_longCount = targetCapacity;
			_nextItemBlockIndex = targetBlockIndex;
		}

		public void AddRange(RecyclableArrayList<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			long targetCapacity = _longCount + items.Count;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _memoryBlocks.Length;

			Span<T> itemsSpan = new(items.AsArray(), 0, items.Count);
			Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, memoryBlockCount);
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			while (targetBlockArraySpan.Length <= itemsSpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				targetBlockIndex++;
				if (targetBlockIndex == memoryBlockCount)
				{
					break;
				}

				targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], 0, blockSize);
			}

			_nextItemIndex = itemsSpan.Length;
			itemsSpan.CopyTo(targetBlockArraySpan);
			_longCount = targetCapacity;
			_nextItemBlockIndex = targetBlockIndex;
		}

		public void AddRange(RecyclableList<T> items)
		{
			if (items.LongCount == 0)
			{
				return;
			}

			long itemsCount = items.LongCount,
				targetCapacity = _longCount + itemsCount;

			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize,
				sourceBlockIndex = 0,
				targetBlockIndex = _nextItemBlockIndex,
				toCopy;

			Span<T[]> sourceMemoryBlocksSpan = new(items._memoryBlocks, 0, items._memoryBlocks.Length);
			Span<T> itemsSpan = new(sourceMemoryBlocksSpan[sourceBlockIndex], 0, items._blockSize);
			Span<T[]> targetMemoryBlocksSpan = new(_memoryBlocks, 0, _memoryBlocks.Length);
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
						targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex], 0, blockSize);
					}
				}
			}

			if ((targetCapacity & (blockSize - 1)) == 0)
			{
				_nextItemIndex = 0;
				_nextItemBlockIndex = targetBlockIndex + 1;
			}
			else
			{
				_nextItemIndex = (int)(targetCapacity & (blockSize - 1));
				_nextItemBlockIndex = targetBlockIndex;

			}

			_longCount = targetCapacity;
		}

		public void AddRange(List<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			long targetCapacity = _longCount + items.Count;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				itemsCount = items.Count,
				copied = Math.Min(blockSize - _nextItemIndex, itemsCount);

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, _memoryBlocks.Length);
			items.CopyTo(0, memoryBlocksSpan[targetBlockIndex], _nextItemIndex, copied);
			if (_nextItemIndex + copied == blockSize)
			{
				targetBlockIndex++;
				_nextItemIndex = 0;
			}
			else
			{
				_nextItemIndex += copied;
			}

			while (blockSize <= itemsCount - copied)
			{
				items.CopyTo(copied, memoryBlocksSpan[targetBlockIndex++], 0, blockSize);
				copied += blockSize;
			}

			if ((itemsCount - copied < blockSize) && (itemsCount - copied != 0))
			{
				_nextItemIndex = itemsCount - copied;
				items.CopyTo(copied, memoryBlocksSpan[targetBlockIndex], 0, _nextItemIndex);
			}

			_nextItemBlockIndex = targetBlockIndex;
			_longCount = targetCapacity;
		}

		public void AddRange(IList<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			long targetCapacity = _longCount + items.Count;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _memoryBlocks.Length;

			T[] itemsBuffer = items.Count >= RecyclableDefaults.MinPooledArrayLength ? _blockArrayPool.Rent(items.Count) : new T[items.Count];
			try
			{
				items.CopyTo(itemsBuffer, 0);

				Span<T> itemsSpan = new(itemsBuffer, 0, items.Count);
				Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, memoryBlockCount);
				Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
				while (targetBlockArraySpan.Length <= itemsSpan.Length)
				{
					itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
					itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
					targetBlockIndex++;
					if (targetBlockIndex == memoryBlockCount)
					{
						break;
					}

					targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], 0, blockSize);
				}

				itemsSpan.CopyTo(targetBlockArraySpan);
				_longCount = targetCapacity;
				_nextItemBlockIndex = targetBlockIndex;
				_nextItemIndex = itemsSpan.Length;
			}
			finally
			{
				if (items.Count >= RecyclableDefaults.MinPooledArrayLength)
				{
					_blockArrayPool.Return(itemsBuffer, NeedsClearing);
				}
			}
		}

		public void AddRange(IEnumerable<T> source, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			if (source is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(sourceRecyclableList);
				return;
			}

			if (source is RecyclableArrayList<T> sourceRecyclableArrayList)
			{
				AddRange(sourceRecyclableArrayList);
				return;
			}

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

			if (source is IList<T> sourceIList)
			{
				AddRange(sourceIList);
				return;
			}

			if (source.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
			{
				AddRangeWithKnownCount(source, requiredAdditionalCapacity);
				return;
			}

			AddRangeEnumerated(source, growByCount);
		}

		public void Clear()
		{
			if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
			{
				Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, ReservedBlockCount);
				int memoryBlocksCount = memoryBlocksSpan.Length;
				ArrayPool<T> blockArrayPool = _blockArrayPool;
				for (int toRemoveIdx = 0; toRemoveIdx < memoryBlocksCount; toRemoveIdx++)
				{
					blockArrayPool.Return(memoryBlocksSpan[toRemoveIdx], NeedsClearing);
				}
			}

			_capacity = 0;
			_nextItemBlockIndex = 0;
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

			if (_nextItemBlockIndex == 0 || (_nextItemBlockIndex == 1 && _nextItemIndex == 0))
			{
				return Array.IndexOf(_memoryBlocks[0], item, 0, (int)_longCount) >= 0;
			}

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			int lastBlockIndex = _nextItemBlockIndex;
			for (int blockIndex = 0; blockIndex < lastBlockIndex; blockIndex++)
			{
				if (Array.IndexOf(memoryBlocksSpan[blockIndex], item, 0, _blockSize) >= 0)
				{
					return true;
				}
			}

			return lastBlockIndex < memoryBlocksSpan.Length && Array.IndexOf(memoryBlocksSpan[lastBlockIndex], item, 0, _nextItemIndex) >= 0;
		}

		public void CopyTo(T[] array, int arrayIndex) => _memoryBlocks.CopyTo(0, _blockSize, _longCount, array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T item) => _longCount == 0
				? ItemNotFoundIndex
				: _nextItemBlockIndex == 0 || (_nextItemBlockIndex == 1 && _nextItemIndex == 0)
				? Array.IndexOf(_memoryBlocks[0], item, 0, (int)_longCount)
				: (int)DoIndexOf(item, _memoryBlocks, LastTakenBlockIndex, _blockSize, _nextItemIndex);

		public void Insert(int index, T item) => throw new NotSupportedException();
		public long LongIndexOf(T item) => _longCount == 0
				? ItemNotFoundIndex
				: _nextItemBlockIndex == 0 || (_nextItemBlockIndex == 1 && _nextItemIndex == 0)
				? Array.IndexOf(_memoryBlocks[0], item, 0, (int)_longCount)
				: DoIndexOf(item, _memoryBlocks, LastTakenBlockIndex, _blockSize, _nextItemIndex);

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
				_blockArrayPool.Return(_memoryBlocks[index], NeedsClearing);
			}

			_capacity -= _blockSize;
			if (_nextItemBlockIndex == index)
			{
				_nextItemIndex = 0;
			}
			else if (_nextItemBlockIndex > index)
			{
				_nextItemBlockIndex--;
			}
		}

		public void RemoveAt(int index) => RemoveAt(this, index, _blockArrayPool);
		public void RemoveAt(long index) => RemoveAt(this, index, _blockArrayPool);

		IEnumerator IEnumerable.GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		~RecyclableList()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose();
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			if (_capacity > 0)
			{
				Clear();
				if (_memoryBlocks.Length >= RecyclableDefaults.MinPooledArrayLength)
				{
					_memoryBlocksPool.Return(_memoryBlocks, NeedsClearing);
				}

				GC.SuppressFinalize(this);
			}
		}
	}
}
