using System.Buffers;
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Parallel;
using Recyclable.Collections.Pools;

[assembly: InternalsVisibleTo("Recyclable.Collections.Benchmarks")]

namespace Recyclable.Collections
{
	public partial class RecyclableList<T> : IDisposable, IList<T>
	{
		private static readonly ArrayPool<T[]> _defaultMemoryBlocksPool = ArrayPool<T[]>.Create();
		private static readonly ArrayPool<T> _defaultBlockArrayPool = ArrayPool<T>.Create();
		private static readonly T[][] _emptyMemoryBlocksArray = new T[0][];
		private static readonly T[] _emptyBlockArray = new T[0];

		internal int _blockSize;
		internal byte _blockSizePow2BitShift;
		internal int _blockSizeMinus1;
		private int _nextItemBlockIndex;
		internal int _nextItemIndex;
		private int _reservedBlockCount;

		protected static readonly bool NeedsClearing = !typeof(T).IsValueType;

		protected ArrayPool<T[]> _memoryBlocksPool;
		protected ArrayPool<T> _blockArrayPool;
		internal T[][] _memoryBlocks;

		private long _capacity;
		public long Capacity
		{
			get => _capacity;
			protected set => _capacity = value;
		}

		public int Count => checked((int)_longCount);
		public bool IsReadOnly { get; }
		private int _lastBlockWithData;
		public int LastBlockWithData => _lastBlockWithData;

		internal long _longCount;

		public long LongCount
		{
			get => _longCount;
			set => _longCount = value;
		}

		public int ReservedBlockCount => _reservedBlockCount;
		public int BlockSize => _blockSize;
		public byte BlockSizePow2BitShift => _blockSizePow2BitShift;
		public int NextItemBlockIndex => _nextItemBlockIndex;
		public int NextItemIndex => _nextItemIndex;

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		private static void CopyItems(RecyclableList<T> list, long index)
		{
			T[][] memoryBlocks = list._memoryBlocks;
			int nextItemIndex = list._nextItemIndex;
			int blockSize = list._blockSize;
			int sourceBlockIndex = (int)((index + 1) >> list._blockSizePow2BitShift);
			int sourceItemIndex = (int)((index + 1) & list._blockSizeMinus1);

			int targetBlockIndex = (int)(index >> list._blockSizePow2BitShift);
			int targetItemIndex = (int)(index & list._blockSizeMinus1);

			int lastTakenBlockIndex = list._lastBlockWithData;
			while (sourceBlockIndex < lastTakenBlockIndex || (sourceBlockIndex == lastTakenBlockIndex && (sourceItemIndex < nextItemIndex || sourceBlockIndex != list._nextItemBlockIndex)))
			{
				int toCopy = sourceBlockIndex < lastTakenBlockIndex || nextItemIndex == 0
					? blockSize - (sourceItemIndex >= targetItemIndex ? sourceItemIndex : targetItemIndex)
					: Math.Min(nextItemIndex, blockSize - targetItemIndex);

				Array.Copy(memoryBlocks[sourceBlockIndex], sourceItemIndex, memoryBlocks[targetBlockIndex], targetItemIndex, toCopy);

				// We didn't have enough room in the target array block. There are still items in the source array block to copy.
				if (sourceItemIndex + toCopy < blockSize)
				{
					sourceItemIndex += toCopy;
					targetBlockIndex++;
					targetItemIndex = 0;
				}
				// We copied all the source items in the current array block. But have we filled the target?
				else
				{
					sourceItemIndex = 0;
					sourceBlockIndex++;
					if (targetItemIndex + toCopy < blockSize)
					{
						targetItemIndex += toCopy;
					}
					else
					{
						targetBlockIndex++;
						targetItemIndex = 0;
					}
				}
			}
		}

		private static void ThrowArgumentOutOfRangeException(in string message) => throw new ArgumentOutOfRangeException("index", message);

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
					capacity = EnsureCapacity(this, capacity + growByCount);
					memoryBlocksCount = _reservedBlockCount;
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
						_lastBlockWithData = targetBlockIdx;
						_longCount += copied + i - _longCount;
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
				_ = EnsureCapacity(this, requiredCapacity);
				blockSize = _blockSize;
			}

			var memoryBlocksCount = ReservedBlockCount;
			memoryBlocksSpan = new(_memoryBlocks, 0, memoryBlocksCount);
			blockArraySpan = new(memoryBlocksSpan[targetBlockIdx], 0, blockSize);
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
			_lastBlockWithData = targetBlockIdx - (targetItemIdx > 0 ? 0 : 1);
		}

		/// <summary>
		/// Creates a set of new memory buffers, if needed, to allow storing at minimum <paramref name="newCapacity"/> no. of items.
		/// </summary>
		/// <param name="list"><see cref="RecyclableList{T}"/> that needs to be resized.</param>
		/// <param name="minBlockSize">Minimal requested block size. It MUST be rounded to the power of 2, see remarks.</param>
		/// <param name="minBlockSizePow2Shift">Pre-calculated bit shifting value for left & right shift operations against<paramref name="minBlockSize"/>.</param>
		/// <param name="newCapacity">The minimum no. of items <paramref name="list"/> MUST be able to store after <see cref="RecyclableList{T}.Resize(RecyclableList{T}, int, byte, long)"/>.</param>
		/// <remarks><para>
		/// For performance reasons, <paramref name="minBlockSize"/> MUST a power of 2. This simplifies a lot block & item
		/// index calculations, i.e. makes them logical operations on bits.
		/// </para>
		/// <para>This method checks for integral overflow.</para>
		/// </remarks>
		/// <remarks>
		/// This method doesn't support downsizing the memory block. As such it doesn't release excessive blocks. This is to for additional performance
		/// gain - 1+ operation less for each call. If you need this functionality, you need to implement it on a higher level.
		/// </remarks>
		/// <returns>The maximum no. of items <paramref name="list"/> can store.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		protected static long Resize(RecyclableList<T> list, int minBlockSize, byte minBlockSizePow2Shift, long newCapacity)
		{
			ArrayPool<T> blockArrayPool = list._blockArrayPool;
			int sourceBlockCount = list._reservedBlockCount;
			int requiredBlockCount = checked((int)(newCapacity >> minBlockSizePow2Shift) + ((newCapacity & (minBlockSize - 1)) > 0 ? 1 : 0));
			int blockIndex;

			T[][] newMemoryBlocks;
			if (requiredBlockCount > (list._memoryBlocks?.Length ?? 0))
			{
				// Allocate new memory block for all arrays
				newMemoryBlocks = requiredBlockCount >= RecyclableDefaults.MinPooledArrayLength ? list._memoryBlocksPool.Rent(requiredBlockCount) : new T[requiredBlockCount][];

				// Copy arrays from the old memory block for all arrays
				if (sourceBlockCount > 0)
				{
					Array.Copy(list._memoryBlocks!, newMemoryBlocks, sourceBlockCount);
					// We can now return the old memory block for all arrays itself
					if (sourceBlockCount >= RecyclableDefaults.MinPooledArrayLength)
					{
						list._memoryBlocksPool.Return(list._memoryBlocks!, true);
					}
				}

				list._memoryBlocks = newMemoryBlocks;
			}
			else
			{
				newMemoryBlocks = list._memoryBlocks!;
			}

			// Allocate arrays for any new blocks
			if (requiredBlockCount > sourceBlockCount)
			{
				if (minBlockSize >= RecyclableDefaults.MinPooledArrayLength)
				{
					if (sourceBlockCount == 0)
					{
						minBlockSize = GetEstimatedBlockSize(minBlockSize, blockArrayPool);
					}

					blockIndex = sourceBlockCount;
					while (true)
					{
						newMemoryBlocks[blockIndex] = blockArrayPool.Rent(minBlockSize);
						if (blockIndex + 1 < requiredBlockCount)
						{
							blockIndex++;
						}
						else
						{
							break;
						}
					}
				}
				else
				{
					blockIndex = sourceBlockCount;
					while (true)
					{
						newMemoryBlocks[blockIndex] = new T[minBlockSize];
						if (blockIndex + 1 < requiredBlockCount)
						{
							blockIndex++;
						}
						else
						{
							break;
						}
					}
				}
			}

			list._reservedBlockCount = requiredBlockCount;
			return (long)requiredBlockCount << minBlockSizePow2Shift;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		private static int GetEstimatedBlockSize(int minBlockSize, ArrayPool<T> blockArrayPool)
		{
			var array = blockArrayPool.Rent(minBlockSize);
			minBlockSize = array.Length;
			blockArrayPool.Return(array);
			return minBlockSize;
		}

		// & WAS SLOWER
		// [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		// & WAS SLOWER
		// [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		// & WAS SLOWER
		// [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
		// & WAS SLOWER
		// [MethodImpl(MethodImplOptions.NoInlining)]
		//private static void Iterate(RecyclableList<T> list, in ParallelSynchronizationContext context, in T itemToFind)
		//{
		//	// TODO: Switch to use _blockSize instead of list._longCountIndeexOfStep for simplifications & better performance.

		//	// & WAS SLOWER without
		//	int	blockIndex = 0;

		//	long itemsCount = list._longCount;
		//	// & WAS SLOWER
		//	// int blockSize = _blockSize;
		//	// & WAS SLOWER
		//	// while ((blockIndex << list._blockSizePow2BitShift) + itemIndex > step)
		//	while (itemsCount > list._blockSize)
		//	{
		//		// At this point itemIndex is limited to blockSize range - int.
		//		if (!ScheduleIndexOfTask(context, list, blockIndex, list._blockSize, itemToFind))
		//		{
		//			break;
		//		}

		//		itemsCount -= list._blockSize;
		//		blockIndex++;
		//	}

		//	_ = ScheduleIndexOfTask(context, list, blockIndex, (int)itemsCount, itemToFind);
		//}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		protected static void SetupBlockArrayPooling(RecyclableList<T> list, int blockSize, ArrayPool<T>? blockArrayPool = null)
		{
			list._blockSize = blockSize;
			list._blockSizePow2BitShift = (byte)(31 - BitOperations.LeadingZeroCount((uint)blockSize));
			list._blockArrayPool = blockSize >= RecyclableDefaults.MinPooledArrayLength
				? blockArrayPool ?? RecyclableArrayPool<T>.Shared(blockSize)
				: RecyclableArrayPool<T>.Null;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		protected static long EnsureCapacity(RecyclableList<T> list, long requestedCapacity)
		{
			long newCapacity = list._capacity > 0
				? checked((long)BitOperations.RoundUpToPowerOf2((ulong)requestedCapacity))
				: requestedCapacity;

			int blockSize = list._blockSize;
			newCapacity = Resize(list, blockSize, list._blockSizePow2BitShift, newCapacity);
			if (blockSize != list._memoryBlocks[0].Length && newCapacity > 0)
			{
				SetupBlockArrayPooling(list, list._memoryBlocks[0].Length);
				list._blockSizeMinus1 = list._blockSize - 1;
			}

			list._capacity = newCapacity;
			return newCapacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#pragma warning disable CS8618 // It's set by SetupBlockArrayPooling
		public RecyclableList(int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
#pragma warning restore CS8618
		{
			_memoryBlocksPool = memoryBlocksPool ?? _defaultMemoryBlocksPool;

			if (expectedItemsCount > 0)
			{
				minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));
				SetupBlockArrayPooling(this, minBlockSize, blockArrayPool);
				_capacity = Resize(this, minBlockSize, _blockSizePow2BitShift, expectedItemsCount.Value);
				if (_blockSize != _memoryBlocks![0].Length && _capacity > 0)
				{
					SetupBlockArrayPooling(this, _memoryBlocks[0].Length, blockArrayPool);
					_blockSizeMinus1 = _blockSize - 1;
				}
				else
				{
					_blockSizeMinus1 = minBlockSize - 1;
				}
			}
			else
			{
				SetupBlockArrayPooling(this, checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize)), blockArrayPool);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}
		}

#pragma warning disable CS8618 // It's set by SetupBlockArrayPooling
		public RecyclableList(IEnumerable<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
#pragma warning restore CS8618
		{
			_memoryBlocksPool = memoryBlocksPool ?? _defaultMemoryBlocksPool;
			if (expectedItemsCount > 0)
			{
				minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));
				SetupBlockArrayPooling(this, minBlockSize, blockArrayPool);
				_capacity = Resize(this, minBlockSize, _blockSizePow2BitShift, expectedItemsCount.Value);
				if (minBlockSize != _memoryBlocks![0].Length && _capacity > 0)
				{
					SetupBlockArrayPooling(this, _memoryBlocks[0].Length, blockArrayPool);
					_blockSizeMinus1 = _blockSize - 1;
				}
				else
				{
					_blockSizeMinus1 = minBlockSize - 1;
				}
			}
			else
			{
				SetupBlockArrayPooling(this, checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize)), blockArrayPool);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public T this[long index]
		{
			get => _memoryBlocks[index >> _blockSizePow2BitShift][index & _blockSizeMinus1];
			set => new Span<T>(_memoryBlocks[checked((int)(index >> _blockSizePow2BitShift))])[checked((int)(index & _blockSizeMinus1))] = value;
		}

		public T this[int index]
		{
			get => _memoryBlocks[index >> _blockSizePow2BitShift][index & _blockSizeMinus1];
			set => new Span<T>(_memoryBlocks[index >> _blockSizePow2BitShift])[index & _blockSizeMinus1] = value;
		}

		public T[][] AsArray { get => _memoryBlocks; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			if (_capacity < _longCount + 1)
			{
				_ = EnsureCapacity(this, _longCount + 1);
			}

			_memoryBlocks[_nextItemBlockIndex][_nextItemIndex++] = item;
			if (_nextItemIndex == _blockSize)
			{
				_nextItemBlockIndex++;
				_nextItemIndex = 0;
			}
			else if (_nextItemIndex == 1)
			{
				_lastBlockWithData++;
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
				_ = EnsureCapacity(this, targetCapacity);
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _reservedBlockCount;

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
			_lastBlockWithData = targetBlockIndex - (itemsSpan.Length > 0 ? 0 : 1);
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
				_ = EnsureCapacity(this, targetCapacity);
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _reservedBlockCount;

			Span<T> itemsSpan = new(items.AsArray, 0, items.Count);
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
			_lastBlockWithData = targetBlockIndex - (itemsSpan.Length > 0 ? 0 : 1);
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
				_ = EnsureCapacity(this, targetCapacity);
			}

			int blockSize = _blockSize,
				sourceBlockIndex = 0,
				targetBlockIndex = _nextItemBlockIndex,
				toCopy;

			Span<T[]> sourceMemoryBlocksSpan = new(items._memoryBlocks, 0, items._reservedBlockCount);
			Span<T> itemsSpan = new(sourceMemoryBlocksSpan[sourceBlockIndex], 0, items._blockSize);
			Span<T[]> targetMemoryBlocksSpan = new(_memoryBlocks, 0, _reservedBlockCount);
			Span<T> targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			long copiedCount = 0;
			while (copiedCount < itemsCount)
			{
				toCopy = checked((int)Math.Min(itemsCount - copiedCount, itemsSpan.Length));
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

			_lastBlockWithData = targetBlockIndex;
			if ((targetCapacity & _blockSizeMinus1) == 0)
			{
				_nextItemIndex = 0;
				_nextItemBlockIndex = targetBlockIndex + 1;
			}
			else
			{
				_nextItemIndex = checked((int)(targetCapacity & _blockSizeMinus1));
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
				_ = EnsureCapacity(this, targetCapacity);
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				itemsCount = items.Count,
				copied = Math.Min(blockSize - _nextItemIndex, itemsCount);

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, _reservedBlockCount);
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
			_lastBlockWithData = targetBlockIndex - (_nextItemIndex > 0 ? 0 : 1);
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
				_ = EnsureCapacity(this, targetCapacity);
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _reservedBlockCount;

			// We're better off to temporarily copy it to a fixed array,
			// than copying them item by item. We may run into OOMs, though.
			T[] itemsBuffer = items.Count >= RecyclableDefaults.MinPooledArrayLength ? _defaultBlockArrayPool.Rent(items.Count) : new T[items.Count];
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
				_lastBlockWithData = targetBlockIndex - (itemsSpan.Length > 0 ? 0 : 1);
			}
			finally
			{
				if (items.Count >= RecyclableDefaults.MinPooledArrayLength)
				{
					_defaultBlockArrayPool.Return(itemsBuffer, NeedsClearing);
				}
			}
		}

		public void AddRange(IEnumerable<T> source, int growByCount = RecyclableDefaults.BlockSize)
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
			if (_longCount == 0)
			{
				return;
			}

			if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
			{
				T[][] memoryBlocks = _memoryBlocks;
				int memoryBlocksCount = _reservedBlockCount;
				ArrayPool<T> blockArrayPool = _blockArrayPool;
				int toRemoveIdx = 0;
				while (toRemoveIdx < memoryBlocksCount)
				{
					blockArrayPool.Return(memoryBlocks[toRemoveIdx++], NeedsClearing);
				}
			}

			Array.Fill(_memoryBlocks, _emptyBlockArray, 0, _reservedBlockCount);
			_capacity = 0;
			_reservedBlockCount = 0;
			_nextItemBlockIndex = 0;
			_nextItemIndex = 0;
			_lastBlockWithData = RecyclableDefaults.ItemNotFoundIndex;
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
				return Array.IndexOf(_memoryBlocks[0], item, 0, checked((int)_longCount)) >= 0;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public int IndexOf(T item)
		{
			if (_lastBlockWithData == 0)
			{
				return Array.IndexOf(_memoryBlocks[0], item, 0, _longCount < _blockSize ? _nextItemIndex : _blockSize);
			}
			else if (_longCount == 0)
			{
				return RecyclableDefaults.ItemNotFoundIndex;
			}

			int itemIndex = Array.IndexOf(_memoryBlocks[0], item, 0, _longCount < _blockSize ? _nextItemIndex : _blockSize);
			if (itemIndex >= 0)
			{
				return itemIndex;
			}
			else if (_longCount < RecyclableDefaults.MinItemsCountForParallelization)
			{
				return (int)RecyclableListIndexOfHelpers.IndexOfSequential(this, item);
			}
			else
			{
				// & WAS SLOWER
				// long toSearch = _longCount - _blockSize;
				if (_longCount > _blockSize)
				{
					itemIndex = Array.IndexOf(_memoryBlocks[1], item, 0, (int)Math.Min(_blockSize, _longCount - _blockSize));
					return itemIndex >= 0 ? _blockSize + itemIndex : (int)RecyclableListIndexOfHelpers.IndexOfParallel(this, item);
				}
				else
				{
					return RecyclableDefaults.ItemNotFoundIndex;
				}
			}
		}

		public void Insert(int index, T item) => throw new NotSupportedException();

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public long LongIndexOf(T item)
		{
			if (_lastBlockWithData == 0)
			{
				return Array.IndexOf(_memoryBlocks[0], item, 0, _longCount < _blockSize ? _nextItemIndex : _blockSize);
			}
			else if (_longCount == 0)
			{
				return RecyclableDefaults.ItemNotFoundIndex;
			}

			int itemIndex = Array.IndexOf(_memoryBlocks[0], item, 0, _longCount < _blockSize ? _nextItemIndex : _blockSize);
			if (itemIndex >= 0)
			{
				return itemIndex;
			}
			else if (_longCount < RecyclableDefaults.MinItemsCountForParallelization)
			{
				return RecyclableListIndexOfHelpers.IndexOfSequential(this, item);
			}
			else
			{
				// & WAS SLOWER
				// long toSearch = _longCount - _blockSize;
				if (_longCount > _blockSize)
				{
					itemIndex = Array.IndexOf(_memoryBlocks[1], item, 0, (int)Math.Min(_blockSize, _longCount - _blockSize));
					return itemIndex >= 0 ? _blockSize + itemIndex : RecyclableListIndexOfHelpers.IndexOfParallel(this, item);
				}
				else
				{
					return RecyclableDefaults.ItemNotFoundIndex;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public bool Remove(T item)
		{
			var index = LongIndexOf(item);
			if (index >= 0)
			{
				_longCount--;

				if (index < _longCount)
				{
					CopyItems(this, index);
				}

				if (_nextItemIndex > 0)
				{
					_nextItemIndex--;
					if (_nextItemIndex == 0)
					{
						_lastBlockWithData--;
					}
				}
				else
				{
					_nextItemIndex = _blockSizeMinus1;
					_nextItemBlockIndex--;
				}

				if (NeedsClearing)
				{
#pragma warning disable CS8601 // In real use cases we'll never access it
					_memoryBlocks[_nextItemBlockIndex][_nextItemIndex] = default;
#pragma warning restore CS8601
				}

				return true;
			}

			return false;
		}

		public void RemoveBlock(int index)
		{
			if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
			{
				_blockArrayPool.Return(_memoryBlocks[index], NeedsClearing);
				_memoryBlocks[index] = _emptyBlockArray;
			}
			else
			{
				_memoryBlocks[index] = _emptyBlockArray;
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

			_lastBlockWithData--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void RemoveAt(int index)
		{
			if (index >= _longCount || index < 0)
			{
				ThrowArgumentOutOfRangeException($"Argument \"{nameof(index)}\" = {index} is out of range. Expected value between 0 and {_longCount - 1}");
			}

			_longCount--;

			if (index < _longCount)
			{
				CopyItems(this, index);
			}

			if (_nextItemIndex > 0)
			{
				_nextItemIndex--;
				if (_nextItemIndex == 0)
				{
					_lastBlockWithData--;
				}
			}
			else
			{
				_nextItemIndex = _blockSizeMinus1;
				_nextItemBlockIndex--;
			}

			if (NeedsClearing)
			{
#pragma warning disable CS8601 // In real use cases we'll never access it
				_memoryBlocks[_nextItemBlockIndex][_nextItemIndex] = default;
#pragma warning restore CS8601
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void RemoveAt(long index)
		{
			if (index >= _longCount || index < 0)
			{
				ThrowArgumentOutOfRangeException($"Argument \"{nameof(index)}\" = {index} is out of range. Expected value between 0 and {_longCount - 1}");
			}

			_longCount--;

			if (index < _longCount)
			{
				CopyItems(this, index);
			}

			if (_nextItemIndex > 0)
			{
				_nextItemIndex--;
				if (_nextItemIndex == 0)
				{
					_lastBlockWithData--;
				}
			}
			else
			{
				_nextItemIndex = _blockSizeMinus1;
				_nextItemBlockIndex--;
			}

			if (NeedsClearing)
			{
#pragma warning disable CS8601 // In real use cases we'll never access it
				_memoryBlocks[_nextItemBlockIndex][_nextItemIndex] = default;
#pragma warning restore CS8601
			}
		}

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
