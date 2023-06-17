﻿using Recyclable.Collections.Pools;
using System.Buffers;
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections.Benchmarks.POC
{
	#region RecyclableLongList v1

	internal class RecyclableLongListV1<T> : IDisposable, IList<T>
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

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private void DoRemoveAt(long index)
		{
			int blockSize = _blockSize;
			int nextItemIndex = _nextItemIndex;
			T[][] memoryBlocks = _memoryBlocks;
			_longCount--;
			if (index < _longCount)
			{
				int sourceBlockIndex = (int)((index + 1) >> _blockSizePow2Shift);
				int sourceItemIndex = (int)((index + 1) & (blockSize - 1));

				int targetBlockIndex = (int)(index >> _blockSizePow2Shift);
				int targetItemIndex = (int)(index & (blockSize - 1));

				int lastTakenBlockIndex = LastTakenBlockIndex;
				while (sourceBlockIndex < lastTakenBlockIndex || (sourceBlockIndex == lastTakenBlockIndex && (sourceItemIndex < nextItemIndex || sourceBlockIndex != _nextItemBlockIndex)))
				{
					int toCopy = sourceBlockIndex < lastTakenBlockIndex || nextItemIndex == 0
						? Math.Min(blockSize - sourceItemIndex, blockSize - targetItemIndex)
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

			if (nextItemIndex > 0)
			{
				_nextItemIndex--;
			}
			else
			{
				_nextItemIndex = blockSize - 1;
				_nextItemBlockIndex--;
			}

			if (NeedsClearing)
			{
#pragma warning disable CS8601 // In real use cases we'll never access it
				new Span<T>(memoryBlocks[_nextItemBlockIndex], _nextItemIndex, 1)[0] = default;
#pragma warning restore CS8601
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
					return itemIndex + (blockIndex << (31 - BitOperations.LeadingZeroCount((uint)blockSize)));
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

			return itemIndex >= 0 ? itemIndex + (lastBlockIndex << (31 - BitOperations.LeadingZeroCount((uint)blockSize))) : ItemNotFoundIndex;
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

			// Release excessive blocks if we're downsizing
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

		internal long EnsureCapacity(long requestedCapacity)
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

		public RecyclableLongListV1(int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
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

		public RecyclableLongListV1(IEnumerable<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
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

		public void AddRange(RecyclableList<T> items)
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
		}

		public void AddRange(RecyclableLongListV1<T> items)
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
			if (source is RecyclableLongListV1<T> sourceRecyclableLongList)
			{
				AddRange(sourceRecyclableLongList);
				return;
			}

			if (source is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(sourceRecyclableList);
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

		public void CopyTo(T[] array, int arrayIndex) => RecyclableLongList<T>.RecyclableLongListHelpers.CopyTo(_memoryBlocks, 0, _blockSize, _longCount, array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => RecyclableLongList<T>.RecyclableLongListHelpers.Enumerate(_memoryBlocks, _blockSize, LongCount).GetEnumerator();

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
				DoRemoveAt(itemIndex);
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

		public void RemoveAt(int index)
		{
			if (index > _longCount - 1 || index < 0)
			{
				ThrowArgumentOutOfRangeException($"Argument \"{nameof(index)}\" = {index} is out of range. Expected value between 0 and {_longCount - 1}");
			}

			DoRemoveAt(index);
		}

		public void RemoveAt(long index)
		{
			if (index > _longCount - 1 || index < 0)
			{
				ThrowArgumentOutOfRangeException($"Argument \"{nameof(index)}\" = {index} is out of range. Expected value between 0 and {_longCount - 1}");
			}

			DoRemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator() => RecyclableLongList<T>.RecyclableLongListHelpers.Enumerate(_memoryBlocks, _blockSize, LongCount).GetEnumerator();

		~RecyclableLongListV1()
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

	#endregion

	#region RecyclableLongList v2

	internal class RecyclableLongListV2<T> : IDisposable, IList<T>
	{
		private const int ItemNotFoundIndex = -1;

		private static readonly ArrayPool<T[]> _defaultMemoryBlocksPool = ArrayPool<T[]>.Create();
		private static readonly ArrayPool<T> _defaultBlockArrayPool = ArrayPool<T>.Create();
		private static readonly T[][] _emptyMemoryBlocksArray = new T[0][];
		private static readonly T[] _emptyBlockArray = new T[0];

		private int _blockSize;
		private byte _blockSizePow2Shift;
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

		public int ReservedBlockCount => checked((int)(_capacity >> _blockSizePow2Shift) + ((_capacity & (_blockSize - 1)) > 0 ? 1 : 0));
		public int BlockSize => _blockSize;
		public int NextItemBlockIndex => _nextItemBlockIndex;
		public int NextItemIndex => _nextItemIndex;

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		private static void CopyItems(RecyclableLongListV2<T> list, long index)
		{
			T[][] memoryBlocks = list._memoryBlocks;
			int nextItemIndex = list._nextItemIndex;
			int blockSize = list._blockSize;
			int sourceBlockIndex = checked((int)((index + 1) >> list._blockSizePow2Shift));
			int sourceItemIndex = checked((int)((index + 1) & (blockSize - 1)));

			int targetBlockIndex = checked((int)(index >> list._blockSizePow2Shift));
			int targetItemIndex = checked((int)(index & (blockSize - 1)));

			int lastTakenBlockIndex = list.LastTakenBlockIndex;
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
				_ = EnsureCapacity(this, requiredCapacity);
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

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		protected static long DoIndexOf(T item, in T[][] memoryBlocks, int lastBlockIndex, int blockSize, int nextItemIndex)
		{
			Span<T[]> memoryBlocksSpan = new(memoryBlocks, 0, lastBlockIndex + 1);
			int itemIndex, blockIndex;
			for (blockIndex = 0; blockIndex < lastBlockIndex; blockIndex++)
			{
				itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex], item, 0, blockSize);
				if (itemIndex >= 0)
				{
					return itemIndex + ((long)blockIndex << (31 - BitOperations.LeadingZeroCount((uint)blockSize)));
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

			return itemIndex >= 0 ? itemIndex + ((long)lastBlockIndex << (31 - BitOperations.LeadingZeroCount((uint)blockSize))) : ItemNotFoundIndex;
		}

		/// <summary>
		/// Creates a set of new memory buffers, if needed, to allow storing at minimum <paramref name="newCapacity"/> no. of items.
		/// </summary>
		/// <param name="list"><see cref="RecyclableLongListV2{T}"/> that needs to be resized.</param>
		/// <param name="minBlockSize">Minimal requested block size. It MUST be rounded to the power of 2, see remarks.</param>
		/// <param name="minBlockSizeBitShift">Pre-calculated bit shifting value for left & right shift operations against<paramref name="minBlockSize"/>.</param>
		/// <param name="newCapacity">The minimum no. of items <paramref name="list"/> MUST be able to store after <see cref="RecyclableLongListV2{T}.Resize(RecyclableLongListV2{T}, int, byte, long)"/>.</param>
		/// <remarks><para>
		/// For performance reasons, <paramref name="minBlockSize"/> MUST a power of 2. This simplifies a lot block & item
		/// index calculations, i.e. makes them logical operations on bits.
		/// </para>
		/// <para>This method checks for integral overflow.</para>
		/// </remarks>
		/// <returns>The maximum no. of items <paramref name="list"/> can store.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		protected static long Resize(RecyclableLongListV2<T> list, int minBlockSize, byte minBlockSizeBitShift, long newCapacity)
		{
			ArrayPool<T> blockArrayPool = list._blockArrayPool;
			int sourceBlockCount = checked((int)(list._capacity >> minBlockSizeBitShift));
			int requiredBlockCount = checked((int)(newCapacity >> minBlockSizeBitShift) + ((newCapacity & (minBlockSize - 1)) > 0 ? 1 : 0));
			int blockIndex;
			Span<T[]> memoryBlocksSpan;

			// Release excessive blocks if we're downsizing
			if (requiredBlockCount < sourceBlockCount && (minBlockSize >= RecyclableDefaults.MinPooledArrayLength))
			{
				memoryBlocksSpan = new(list._memoryBlocks, requiredBlockCount, sourceBlockCount - requiredBlockCount);
				blockIndex = 0;
				while (blockIndex < memoryBlocksSpan.Length)
				{
					blockArrayPool.Return(memoryBlocksSpan[blockIndex++]!, NeedsClearing);
				}
			}

			ArrayPool<T[]> memoryBlocksPool = list._memoryBlocksPool;

			// Allocate new memory block for all arrays
			T[][]? newMemoryBlocks = requiredBlockCount < RecyclableDefaults.MinPooledArrayLength
				? (new T[requiredBlockCount][])
				: memoryBlocksPool.Rent(requiredBlockCount);

			// Copy arrays from the old memory block for all arrays
			if (sourceBlockCount > 0)
			{
				Array.Copy(list._memoryBlocks!, newMemoryBlocks, sourceBlockCount);
				// We can now return the old memory block for all arrays itself
				if (sourceBlockCount >= RecyclableDefaults.MinPooledArrayLength)
				{
					memoryBlocksPool.Return(list._memoryBlocks!, true);
				}
			}

			list._memoryBlocks = newMemoryBlocks;

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

			return (long)newMemoryBlocks.Length << minBlockSizeBitShift;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static long EnsureCapacity(RecyclableLongListV2<T> list, long requestedCapacity)
		{
			long newCapacity = list._capacity > 0
				? checked((long)BitOperations.RoundUpToPowerOf2((ulong)requestedCapacity))
				: requestedCapacity;

			int blockSize = list._blockSize;
			newCapacity = RecyclableLongListV2<T>.Resize(list, blockSize, list._blockSizePow2Shift, newCapacity);
			if (newCapacity > 0 && blockSize != list._memoryBlocks[0].Length)
			{
				blockSize = list._memoryBlocks[0].Length;
				if (blockSize >= RecyclableDefaults.MinPooledArrayLength)
				{
					list._blockArrayPool = RecyclableArrayPool<T>.Shared(blockSize);
				}

				list._blockSizePow2Shift = (byte)MathUtils.GetPow2Shift(blockSize);
				list._blockSize = blockSize;
			}

			list._capacity = newCapacity;
			return newCapacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public RecyclableLongListV2(int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
		{
			_memoryBlocksPool = memoryBlocksPool ?? _defaultMemoryBlocksPool;

			if (expectedItemsCount > 0)
			{
				minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));
				_blockArrayPool = minBlockSize >= RecyclableDefaults.MinPooledArrayLength
					? blockArrayPool ?? RecyclableArrayPool<T>.Shared(minBlockSize)
					: RecyclableArrayPool<T>.Null;

				_blockSizePow2Shift = (byte)(31 - BitOperations.LeadingZeroCount((uint)minBlockSize));
				_capacity = Resize(this, minBlockSize, _blockSizePow2Shift, expectedItemsCount.Value);
				if (_memoryBlocks!.Length > 0 && _blockSize != _memoryBlocks[0].Length)
				{
					_blockSize = _memoryBlocks[0].Length;
					_blockSizePow2Shift = (byte)(31 - BitOperations.LeadingZeroCount((uint)_blockSize));
					if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
					{
						_blockArrayPool = RecyclableArrayPool<T>.Shared(_blockSize);
					}
				}
				else
				{
					_blockSize = minBlockSize;
				}
			}
			else
			{
				_blockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));
				_blockSizePow2Shift = (byte)(31 - BitOperations.LeadingZeroCount((uint)_blockSize));
				_blockArrayPool = _blockSize >= RecyclableDefaults.MinPooledArrayLength
					? blockArrayPool ?? RecyclableArrayPool<T>.Shared(_blockSize)
					: RecyclableArrayPool<T>.Null;

				_memoryBlocks = _emptyMemoryBlocksArray;
			}
		}

		public RecyclableLongListV2(IEnumerable<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
		{
			memoryBlocksPool ??= _defaultMemoryBlocksPool;
			_memoryBlocksPool = memoryBlocksPool;

			if (expectedItemsCount > 0)
			{
				minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));
				_blockArrayPool = minBlockSize >= RecyclableDefaults.MinPooledArrayLength
					? blockArrayPool ?? RecyclableArrayPool<T>.Shared(minBlockSize)
					: RecyclableArrayPool<T>.Null;

				_blockSizePow2Shift = (byte)(31 - BitOperations.LeadingZeroCount((uint)minBlockSize));
				_capacity = Resize(this, minBlockSize, _blockSizePow2Shift, expectedItemsCount.Value);
				if (_memoryBlocks!.Length > 0 && minBlockSize != _memoryBlocks[0].Length)
				{
					_blockSize = _memoryBlocks[0].Length;
					_blockSizePow2Shift = (byte)(31 - BitOperations.LeadingZeroCount((uint)minBlockSize));
					if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
					{
						_blockArrayPool = RecyclableArrayPool<T>.Shared(_blockSize);
					}
				}
				else
				{
					_blockSize = minBlockSize;
				}
			}
			else
			{
				_blockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));
				_blockArrayPool = _blockSize >= RecyclableDefaults.MinPooledArrayLength
					? blockArrayPool ?? RecyclableArrayPool<T>.Shared(_blockSize)
					: RecyclableArrayPool<T>.Null;

				_blockSizePow2Shift = (byte)(31 - BitOperations.LeadingZeroCount((uint)_blockSize));
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public T this[long index]
		{
			get => _memoryBlocks[index >> _blockSizePow2Shift][index & (_blockSize - 1)];
			set => new Span<T>(_memoryBlocks[checked((int)(index >> _blockSizePow2Shift))])[checked((int)(index & (_blockSize - 1)))] = value;
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
				_ = EnsureCapacity(this, _longCount + 1);
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
				_ = EnsureCapacity(this, targetCapacity);
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

		public void AddRange(RecyclableList<T> items)
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
				memoryBlockCount = _memoryBlocks.Length;

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
		}

		public void AddRange(RecyclableLongListV2<T> items)
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

			Span<T[]> sourceMemoryBlocksSpan = new(items._memoryBlocks, 0, items._memoryBlocks.Length);
			Span<T> itemsSpan = new(sourceMemoryBlocksSpan[sourceBlockIndex], 0, items._blockSize);
			Span<T[]> targetMemoryBlocksSpan = new(_memoryBlocks, 0, _memoryBlocks.Length);
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

			if ((targetCapacity & (blockSize - 1)) == 0)
			{
				_nextItemIndex = 0;
				_nextItemBlockIndex = targetBlockIndex + 1;
			}
			else
			{
				_nextItemIndex = checked((int)(targetCapacity & (blockSize - 1)));
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
				_ = EnsureCapacity(this, targetCapacity);
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _memoryBlocks.Length;

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
			}
			finally
			{
				if (items.Count >= RecyclableDefaults.MinPooledArrayLength)
				{
					_defaultBlockArrayPool.Return(itemsBuffer, NeedsClearing);
				}
			}
		}

		public void AddRange(IEnumerable<T> source, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			if (source is RecyclableLongListV2<T> sourceRecyclableLongList)
			{
				AddRange(sourceRecyclableLongList);
				return;
			}

			if (source is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(sourceRecyclableList);
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

		public void CopyTo(T[] array, int arrayIndex) => RecyclableLongList<T>.RecyclableLongListHelpers.CopyTo(_memoryBlocks, 0, _blockSize, _longCount, array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => RecyclableLongList<T>.RecyclableLongListHelpers.Enumerate(_memoryBlocks, _blockSize, LongCount).GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T item) => _longCount == 0
				? ItemNotFoundIndex
				: _nextItemBlockIndex == 0 || (_nextItemBlockIndex == 1 && _nextItemIndex == 0)
				? Array.IndexOf(_memoryBlocks[0], item, 0, checked((int)_longCount))
				: checked((int)DoIndexOf(item, _memoryBlocks, LastTakenBlockIndex, _blockSize, _nextItemIndex));

		public void Insert(int index, T item) => throw new NotSupportedException();

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public long LongIndexOf(T item) => _longCount == 0
				? ItemNotFoundIndex
				: _nextItemBlockIndex == 0 || (_nextItemBlockIndex == 1 && _nextItemIndex == 0)
				? Array.IndexOf(_memoryBlocks[0], item, 0, checked((int)_longCount))
				: DoIndexOf(item, _memoryBlocks, LastTakenBlockIndex, _blockSize, _nextItemIndex);

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
				}
				else
				{
					_nextItemIndex = _blockSize - 1;
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
			}
			else
			{
				_nextItemIndex = _blockSize - 1;
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
			}
			else
			{
				_nextItemIndex = _blockSize - 1;
				_nextItemBlockIndex--;
			}

			if (NeedsClearing)
			{
#pragma warning disable CS8601 // In real use cases we'll never access it
				_memoryBlocks[_nextItemBlockIndex][_nextItemIndex] = default;
#pragma warning restore CS8601
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => RecyclableLongList<T>.RecyclableLongListHelpers.Enumerate(_memoryBlocks, _blockSize, LongCount).GetEnumerator();

		~RecyclableLongListV2()
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

	#endregion

	#region RecyclableLongList v3

	internal class RecyclableLongListV3<T> : IDisposable, IList<T>
	{
		private const int ItemNotFoundIndex = -1;

		private static readonly ArrayPool<T[]> _defaultMemoryBlocksPool = ArrayPool<T[]>.Create();
		private static readonly ArrayPool<T> _defaultBlockArrayPool = ArrayPool<T>.Create();
		private static readonly T[][] _emptyMemoryBlocksArray = new T[0][];
		private static readonly T[] _emptyBlockArray = new T[0];

		private int _blockSize;
		private byte _blockSizePow2Shift;
		private int _blockSizeMinus1;
		private bool _blockPoolingRequired;
		private int _nextItemBlockIndex;
		private int _nextItemIndex;
		private int _reservedBlockCount;

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
		public int LastBlockWithData => _nextItemBlockIndex - (_nextItemIndex > 0 ? 0 : 1);

		protected long _longCount;

		public long LongCount
		{
			get => _longCount;
			set => _longCount = value;
		}

		public int ReservedBlockCount => _reservedBlockCount;
		public int BlockSize => _blockSize;
		public int NextItemBlockIndex => _nextItemBlockIndex;
		public int NextItemIndex => _nextItemIndex;

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		private static void CopyItems(RecyclableLongListV3<T> list, long index)
		{
			T[][] memoryBlocks = list._memoryBlocks;
			int nextItemIndex = list._nextItemIndex;
			int blockSize = list._blockSize;
			int sourceBlockIndex = checked((int)((index + 1) >> list._blockSizePow2Shift));
			int sourceItemIndex = checked((int)((index + 1) & list._blockSizeMinus1));

			int targetBlockIndex = checked((int)(index >> list._blockSizePow2Shift));
			int targetItemIndex = checked((int)(index & list._blockSizeMinus1));

			int lastTakenBlockIndex = list.LastBlockWithData;
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
			return;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		protected static long DoIndexOf(T item, in T[][] memoryBlocks, int lastBlockIndex, int blockSize, int nextItemIndex)
		{
			Span<T[]> memoryBlocksSpan = new(memoryBlocks, 0, lastBlockIndex + 1);
			int itemIndex, blockIndex;
			for (blockIndex = 0; blockIndex < lastBlockIndex; blockIndex++)
			{
				itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex], item, 0, blockSize);
				if (itemIndex >= 0)
				{
					return itemIndex + ((long)blockIndex << (31 - BitOperations.LeadingZeroCount((uint)blockSize)));
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

			return itemIndex >= 0 ? itemIndex + ((long)lastBlockIndex << (31 - BitOperations.LeadingZeroCount((uint)blockSize))) : ItemNotFoundIndex;
		}

		/// <summary>
		/// Creates a set of new memory buffers, if needed, to allow storing at minimum <paramref name="newCapacity"/> no. of items.
		/// </summary>
		/// <param name="list"><see cref="RecyclableLongList{T}"/> that needs to be resized.</param>
		/// <param name="minBlockSize">Minimal requested block size. It MUST be rounded to the power of 2, see remarks.</param>
		/// <param name="minBlockSizePow2Shift">Pre-calculated bit shifting value for left & right shift operations against<paramref name="minBlockSize"/>.</param>
		/// <param name="newCapacity">The minimum no. of items <paramref name="list"/> MUST be able to store after <see cref="RecyclableLongList{T}.Resize(RecyclableLongList{T}, int, byte, long)"/>.</param>
		/// <remarks><para>
		/// For performance reasons, <paramref name="minBlockSize"/> MUST a power of 2. This simplifies a lot block & item
		/// index calculations, i.e. makes them logical operations on bits.
		/// </para>
		/// <para>This method checks for integral overflow.</para>
		/// </remarks>
		/// <returns>The maximum no. of items <paramref name="list"/> can store.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		protected static long Resize(RecyclableLongListV3<T> list, int minBlockSize, byte minBlockSizePow2Shift, long newCapacity)
		{
			ArrayPool<T> blockArrayPool = list._blockArrayPool;
			int sourceBlockCount = list._reservedBlockCount;
			int requiredBlockCount = checked((int)(newCapacity >> minBlockSizePow2Shift) + ((newCapacity & (minBlockSize - 1)) > 0 ? 1 : 0));
			int blockIndex;
			Span<T[]> memoryBlocksSpan;

			// Release excessive blocks if we're downsizing
			if (requiredBlockCount < sourceBlockCount && list._blockPoolingRequired)
			{
				memoryBlocksSpan = new(list._memoryBlocks, requiredBlockCount, requiredBlockCount - sourceBlockCount);
				blockIndex = 0;
				while (blockIndex < memoryBlocksSpan.Length)
				{
					blockArrayPool.Return(memoryBlocksSpan[blockIndex++], NeedsClearing);
				}
			}

			ArrayPool<T[]> memoryBlocksPool = list._memoryBlocksPool;

			// Allocate new memory block for all arrays
			bool memoryBlockPoolingRequired = requiredBlockCount >= RecyclableDefaults.MinPooledArrayLength;
			T[][] newMemoryBlocks = memoryBlockPoolingRequired ? memoryBlocksPool.Rent(requiredBlockCount) : new T[requiredBlockCount][];

			// Copy arrays from the old memory block for all arrays
			if (sourceBlockCount > 0)
			{
				Array.Copy(list._memoryBlocks, newMemoryBlocks, sourceBlockCount);
				// We can now return the old memory block for all arrays itself
				if (sourceBlockCount >= RecyclableDefaults.MinPooledArrayLength)
				{
					memoryBlocksPool.Return(list._memoryBlocks, true);
				}
			}

			list._memoryBlocks = newMemoryBlocks;

			// Allocate arrays for any new blocks
			if (requiredBlockCount > sourceBlockCount)
			{
				memoryBlocksSpan = new(newMemoryBlocks, sourceBlockCount, requiredBlockCount - sourceBlockCount);
				if (list._blockPoolingRequired)
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

			list._reservedBlockCount = requiredBlockCount;
			return (long)requiredBlockCount << minBlockSizePow2Shift;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		protected static void SetupBlockArrayPooling(RecyclableLongListV3<T> list, int blockSize, ArrayPool<T>? blockArrayPool = null)
		{
			list._blockSize = blockSize;
			list._blockSizePow2Shift = (byte)(31 - BitOperations.LeadingZeroCount((uint)blockSize));
			if (blockSize >= RecyclableDefaults.MinPooledArrayLength)
			{
				list._blockPoolingRequired = true;
				list._blockArrayPool = blockArrayPool ?? RecyclableArrayPool<T>.Shared(blockSize);
			}
			else
			{
				list._blockPoolingRequired = false;
				list._blockArrayPool = RecyclableArrayPool<T>.Null;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		protected static long EnsureCapacity(RecyclableLongListV3<T> list, long requestedCapacity)
		{
			long newCapacity = list._capacity > 0
				? checked((long)BitOperations.RoundUpToPowerOf2((ulong)requestedCapacity))
				: requestedCapacity;

			int blockSize = list._blockSize;
			newCapacity = Resize(list, blockSize, list._blockSizePow2Shift, newCapacity);
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
		public RecyclableLongListV3(int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
#pragma warning restore CS8618
		{
			_memoryBlocksPool = memoryBlocksPool ?? _defaultMemoryBlocksPool;

			if (expectedItemsCount > 0)
			{
				minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));
				SetupBlockArrayPooling(this, minBlockSize, blockArrayPool);
				_capacity = Resize(this, minBlockSize, _blockSizePow2Shift, expectedItemsCount.Value);
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
		public RecyclableLongListV3(IEnumerable<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default, ArrayPool<T[]>? memoryBlocksPool = default, ArrayPool<T>? blockArrayPool = default)
#pragma warning restore CS8618
		{
			_memoryBlocksPool = memoryBlocksPool ?? _defaultMemoryBlocksPool;
			if (expectedItemsCount > 0)
			{
				minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));
				SetupBlockArrayPooling(this, minBlockSize, blockArrayPool);
				_capacity = Resize(this, minBlockSize, _blockSizePow2Shift, expectedItemsCount.Value);
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
			get => _memoryBlocks[index >> _blockSizePow2Shift][index & _blockSizeMinus1];
			set => new Span<T>(_memoryBlocks[checked((int)(index >> _blockSizePow2Shift))])[checked((int)(index & _blockSizeMinus1))] = value;
		}

		public T this[int index]
		{
			get => _memoryBlocks[index >> _blockSizePow2Shift][index & _blockSizeMinus1];
			set => new Span<T>(_memoryBlocks[index >> _blockSizePow2Shift])[index & _blockSizeMinus1] = value;
		}

		public T[][] MemoryBlocks { get => _memoryBlocks; }

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
		}

		public void AddRange(RecyclableList<T> items)
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
		}

		public void AddRange(RecyclableLongListV3<T> items)
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
			if (source is RecyclableLongList<T> sourceRecyclableLongList)
			{
				AddRange(sourceRecyclableLongList);
				return;
			}

			if (source is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(sourceRecyclableList);
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
			if (_blockPoolingRequired)
			{
				Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, _reservedBlockCount);
				int memoryBlocksCount = memoryBlocksSpan.Length;
				ArrayPool<T> blockArrayPool = _blockArrayPool;
				for (int toRemoveIdx = 0; toRemoveIdx < memoryBlocksCount; toRemoveIdx++)
				{
					blockArrayPool.Return(memoryBlocksSpan[toRemoveIdx], NeedsClearing);
					memoryBlocksSpan[toRemoveIdx] = _emptyBlockArray;
				}
			}

			_capacity = 0;
			_reservedBlockCount = 0;
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

		public void CopyTo(T[] array, int arrayIndex) => RecyclableLongList<T>.RecyclableLongListHelpers.CopyTo(_memoryBlocks, 0, _blockSize, _longCount, array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => RecyclableLongList<T>.RecyclableLongListHelpers.Enumerate(_memoryBlocks, _blockSize, LongCount).GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T item) => _longCount == 0
				? ItemNotFoundIndex
				: _nextItemBlockIndex == 0 || (_nextItemBlockIndex == 1 && _nextItemIndex == 0)
				? Array.IndexOf(_memoryBlocks[0], item, 0, checked((int)_longCount))
				: checked((int)DoIndexOf(item, _memoryBlocks, LastBlockWithData, _blockSize, _nextItemIndex));

		public void Insert(int index, T item) => throw new NotSupportedException();

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public long LongIndexOf(T item) => _longCount == 0
				? ItemNotFoundIndex
				: _nextItemBlockIndex == 0 || (_nextItemBlockIndex == 1 && _nextItemIndex == 0)
				? Array.IndexOf(_memoryBlocks[0], item, 0, checked((int)_longCount))
				: DoIndexOf(item, _memoryBlocks, LastBlockWithData, _blockSize, _nextItemIndex);

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
			if (_blockPoolingRequired)
			{
				_blockArrayPool.Return(_memoryBlocks[index], NeedsClearing);
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

		IEnumerator IEnumerable.GetEnumerator() => RecyclableLongList<T>.RecyclableLongListHelpers.Enumerate(_memoryBlocks, _blockSize, LongCount).GetEnumerator();

		~RecyclableLongListV3()
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

	#endregion

	public partial class RecyclableLongListPocBenchmarks
	{
		public void RecyclableLongList_EnsureCapacityV1_ByPowOf2()
		{
			using var list = new RecyclableLongListV1<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			DoNothing(_ensureCapacityV1Func.Invoke(list, new object[] { TestObjectCount }));
		}

		public void RecyclableLongList_EnsureCapacityV2_ByPowOf2()
		{
			using var list = new RecyclableLongListV2<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			DoNothing(_ensureCapacityV2Func.Invoke(null, new object[] { list, TestObjectCount }));
		}

		public void RecyclableLongList_EnsureCapacityV3_ByPowOf2()
		{
			using var list = new RecyclableLongListV3<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			DoNothing(_ensureCapacityV3Func.Invoke(null, new object[] { list, TestObjectCount }));
		}

		public void RecyclableLongList_EnsureCapacity_ByPowOf2()
		{
			using var list = new RecyclableLongList<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			DoNothing(_ensureCapacityNewFunc.Invoke(null, new object[] { list, TestObjectCount }));
		}

		public void RecyclableLongList_EnsureCapacityV1_ByBlockSize()
		{
			using var list = new RecyclableLongListV1<long>();
			DoNothing(_ensureCapacityV1Func.Invoke(list, new object[] { TestObjectCount }));
		}

		public void RecyclableLongList_EnsureCapacityV2_ByBlockSize()
		{
			using var list = new RecyclableLongListV2<long>();
			DoNothing(_ensureCapacityV2Func.Invoke(null, new object[] { list, TestObjectCount }));
		}

		public void RecyclableLongList_EnsureCapacityV3_ByBlockSize()
		{
			using var list = new RecyclableLongListV3<long>();
			DoNothing(_ensureCapacityV3Func.Invoke(null, new object[] { list, TestObjectCount }));
		}

		public void RecyclableLongList_EnsureCapacity_ByBlockSize()
		{
			using var list = new RecyclableLongList<long>();
			DoNothing(_ensureCapacityNewFunc.Invoke(null, new object[] { list, TestObjectCount }));
		}
	}
}