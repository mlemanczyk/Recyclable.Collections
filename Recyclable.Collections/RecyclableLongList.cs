using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

[assembly: InternalsVisibleTo("Recyclable.Collections.Benchmarks")]
[assembly: InternalsVisibleTo("Recyclable.CollectionsTests")]

namespace Recyclable.Collections
{
	public partial class RecyclableLongList<T> : IList<T>, IReadOnlyList<T>, IDisposable
	{
		private static readonly T[][] _emptyMemoryBlocksArray = new T[0][];
		private static readonly T[] _emptyBlockArray = new T[0];
		private static readonly bool _defaultIsNull = default(T) == null;
		private static readonly bool NeedsClearing = !typeof(T).IsValueType;

		internal int _blockSize;
		internal byte _blockSizePow2BitShift;
		internal int _blockSizeMinus1;
		internal int _nextItemBlockIndex;
		internal int _nextItemIndex;
		private int _reservedBlockCount;

#nullable disable
		internal T[][] _memoryBlocks;
#nullable restore

		private long _capacity;
		public long Capacity
		{
			get => _capacity;
			private set
			{
				_capacity = value;
				_version++;
				if (_capacity != value)
				{
					_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)value)));
					_version++;
				}
			}
		}

		public int Count => checked((int)_longCount);
		public bool IsReadOnly => false;
		internal int _lastBlockWithData = RecyclableDefaults.ItemNotFoundIndex;
		public int LastBlockWithData => _lastBlockWithData;

		internal long _longCount;
		public long LongCount => _longCount;

		public int ReservedBlockCount => _reservedBlockCount;
		public int BlockSize => _blockSize;
		public int BlockSizeMinus1 => _blockSizeMinus1;
		public byte BlockSizePow2BitShift => _blockSizePow2BitShift;
		public int NextItemBlockIndex => _nextItemBlockIndex;
		public int NextItemIndex => _nextItemIndex;

		internal ulong _version;
		public ulong Version => _version;

		private void AddRangeEnumerated(IEnumerable<T> source, int growByCount)
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
					capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)(capacity + growByCount))));
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

						_capacity = capacity;
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

		private void AddRangeWithKnownCount(IEnumerable<T> source, int requiredAdditionalCapacity)
		{
			long copied = _longCount;
			int blockSize = _blockSize;
			int targetItemIdx = _nextItemIndex;
			int targetBlockIdx = _nextItemBlockIndex;

			Span<T[]> memoryBlocksSpan;
			Span<T> blockArraySpan;
			long requiredCapacity = copied + requiredAdditionalCapacity;
			if (requiredCapacity == 0)
			{
				return;
			}

			if (_capacity < requiredCapacity)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)requiredCapacity)));
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

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public RecyclableLongList(int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			if (expectedItemsCount > 0)
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)expectedItemsCount.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}
		}

		public RecyclableLongList(in T[] source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			if (expectedItemsCount > 0)
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)expectedItemsCount.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public RecyclableLongList(ReadOnlySpan<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			if (expectedItemsCount > 0)
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)expectedItemsCount.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public RecyclableLongList(RecyclableList<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			if (expectedItemsCount > 0)
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)expectedItemsCount.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public RecyclableLongList(RecyclableLongList<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			if (expectedItemsCount > 0)
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)expectedItemsCount.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public RecyclableLongList(List<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			if (expectedItemsCount > 0)
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)expectedItemsCount.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public RecyclableLongList(IList<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			if (expectedItemsCount > 0)
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)expectedItemsCount.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public RecyclableLongList(IEnumerable<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			if (expectedItemsCount > 0)
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((int)BitOperations.RoundUpToPowerOf2((uint)expectedItemsCount.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public T this[long index]
		{
			get => _memoryBlocks[index >> _blockSizePow2BitShift][index & _blockSizeMinus1];
			set
			{
				new Span<T>(_memoryBlocks[checked((int)(index >> _blockSizePow2BitShift))])[checked((int)(index & _blockSizeMinus1))] = value;
				_version++;
			}
		}

		public T this[int index]
		{
			get => _memoryBlocks[index >> _blockSizePow2BitShift][index & _blockSizeMinus1];
			set
			{
				new Span<T>(_memoryBlocks[index >> _blockSizePow2BitShift])[index & _blockSizeMinus1] = value;
				_version++;
			}
		}

		public T[][] AsArray => _memoryBlocks;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			if (_capacity < _longCount + 1)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)(_longCount + 1))));
			}

			_memoryBlocks[_nextItemBlockIndex][_nextItemIndex++] = item;
			if (_nextItemIndex == _blockSize)
			{
				if (_nextItemBlockIndex > _lastBlockWithData)
				{
					_lastBlockWithData++;
				}

				_nextItemBlockIndex++;
				_nextItemIndex = 0;
			}
			else if (_nextItemIndex == 1 && _nextItemBlockIndex > _lastBlockWithData)
			{
				_lastBlockWithData++;
			}

			_longCount++;
			_version++;
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
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _reservedBlockCount;

			Span<T> itemsSpan = new(items);
			Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, memoryBlockCount);
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			while (itemsSpan.Length >= targetBlockArraySpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				if (itemsSpan.Length == 0)
				{
					break;
				}

				targetBlockIndex++;
				targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], 0, blockSize);
			}

			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(targetBlockArraySpan);
				_nextItemBlockIndex = targetBlockIndex;
			}
			else
			{
				_nextItemBlockIndex = targetBlockIndex + 1;
			}

			_nextItemIndex = itemsSpan.Length;
			_longCount = targetCapacity;
			_lastBlockWithData = targetBlockIndex;
			_version++;
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
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
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
			_version++;
		}

		public void AddRange(ReadOnlySpan<T> itemsSpan)
		{
			if (itemsSpan.Length == 0)
			{
				return;
			}

			long targetCapacity = _longCount + itemsSpan.Length;
			if (_capacity < targetCapacity)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _reservedBlockCount;

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks, 0, memoryBlockCount);
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			while (itemsSpan.Length >= targetBlockArraySpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				if (itemsSpan.Length == 0)
				{
					break;
				}

				targetBlockIndex++;
				targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], 0, blockSize);
			}

			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(targetBlockArraySpan);
				_nextItemBlockIndex = targetBlockIndex;
			}
			else
			{
				_nextItemBlockIndex = targetBlockIndex + 1;
			}

			_nextItemIndex = itemsSpan.Length;
			_longCount = targetCapacity;
			_lastBlockWithData = targetBlockIndex;
			_version++;
		}

		public void AddRange(RecyclableLongList<T> items)
		{
			if (items.LongCount == 0)
			{
				return;
			}

			long itemsCount = items.LongCount,
				targetCapacity = _longCount + itemsCount;

			if (_capacity < targetCapacity)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
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
			_version++;
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
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
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
			_version++;
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
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex,
				memoryBlockCount = _reservedBlockCount;

			// We're better off to temporarily copy it to a fixed array,
			// than copying them item by item. We may run into OOMs, though.
			T[] itemsBuffer = items.Count >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T>.RentShared((int)BitOperations.RoundUpToPowerOf2((uint)items.Count)) : new T[items.Count];
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
					RecyclableArrayPool<T>.ReturnShared(itemsBuffer, NeedsClearing);
				}

				_version++;
			}
		}

		public void AddRange(IEnumerable<T> source, int growByCount = RecyclableDefaults.BlockSize)
		{
			if (source is RecyclableLongList<T> sourceRecyclableLongList)
			{
				AddRange(sourceRecyclableLongList);
			}
			else if (source is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(sourceRecyclableList);
			}
			else if (source is T[] sourceArray)
			{
				AddRange(sourceArray);
			}
			else if (source is List<T> sourceList)
			{
				AddRange(sourceList);
			}
			else if (source is IList<T> sourceIList)
			{
				AddRange(sourceIList);
			}
			else if (source.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
			{
				AddRangeWithKnownCount(source, requiredAdditionalCapacity);
				_version++;
			}
			else
			{
				AddRangeEnumerated(source, growByCount);
				_version++;
			}
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
				int toRemoveIdx = 0;
				while (toRemoveIdx < memoryBlocksCount)
				{
					RecyclableArrayPool<T>.ReturnShared(memoryBlocks[toRemoveIdx++], NeedsClearing);
				}
			}

			// TODO: Do we want to initialize with empty arrays or null refs? Null refs may be faster.
			Array.Fill(_memoryBlocks, _emptyBlockArray, 0, _reservedBlockCount);
			_capacity = 0;
			_reservedBlockCount = 0;
			_nextItemBlockIndex = 0;
			_nextItemIndex = 0;
			_lastBlockWithData = RecyclableDefaults.ItemNotFoundIndex;
			_longCount = 0;
			_version++;
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
			int lastBlockIndex = _lastBlockWithData;
			for (int blockIndex = 0; blockIndex < lastBlockIndex; blockIndex++)
			{
				if (Array.IndexOf(memoryBlocksSpan[blockIndex], item, 0, _blockSize) >= 0)
				{
					return true;
				}
			}

			return Array.IndexOf(memoryBlocksSpan[lastBlockIndex], item, 0, _nextItemIndex > 0 ? _nextItemIndex : _blockSize) >= 0;
		}

		public void CopyTo(T[] array, int arrayIndex) => Helpers.CopyTo(_memoryBlocks, 0, _blockSize, _longCount, array, arrayIndex);

		public Enumerator GetEnumerator() => new(this);
		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

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
				return (int)IndexOfHelpers.IndexOfSequential(this, item);
			}
			else
			{
				if (_longCount > _blockSize)
				{
					itemIndex = Array.IndexOf(_memoryBlocks[1], item, 0, (int)Math.Min(_blockSize, _longCount - _blockSize));
					return itemIndex >= 0 ? _blockSize + itemIndex : (int)IndexOfHelpers.IndexOfParallel(this, item);
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
				return IndexOfHelpers.IndexOfSequential(this, item);
			}
			else
			{
				if (_longCount > _blockSize)
				{
					itemIndex = Array.IndexOf(_memoryBlocks[1], item, 0, (int)Math.Min(_blockSize, _longCount - _blockSize));
					return itemIndex >= 0 ? _blockSize + itemIndex : IndexOfHelpers.IndexOfParallel(this, item);
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
			long index;
			if ((index = LongIndexOf(item)) >= 0)
			{
				if (index < --_longCount)
				{
					Helpers.CopyFollowingItems(this, index);
				}

				if (_nextItemIndex > 0)
				{
					if (--_nextItemIndex == 0)
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
#nullable disable
					new Span<T>(_memoryBlocks[_nextItemBlockIndex])[_nextItemIndex] = default;
#nullable restore
				}

				_version++;
				return true;
			}

			return false;
		}

		public void RemoveBlock(int index)
		{
			if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
			{
				RecyclableArrayPool<T>.ReturnShared(_memoryBlocks[index], NeedsClearing);
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
			_version++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void RemoveAt(int index)
		{
			if (index >= _longCount || index < 0)
			{
				Helpers.ThrowIndexOutOfRangeException($"Argument \"{nameof(index)}\" = {index} is out of range. Expected value between 0 and {_longCount - 1}");
			}

			if (index < --_longCount)
			{
				Helpers.CopyFollowingItems(this, index);
			}

			if (_nextItemIndex > 0)
			{
				if (--_nextItemIndex == 0)
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
#nullable disable
				new Span<T>(_memoryBlocks[_nextItemBlockIndex])[_nextItemIndex] = default;
#nullable restore
			}

			_version++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void RemoveAt(long index)
		{
			if (index >= _longCount || index < 0)
			{
				Helpers.ThrowIndexOutOfRangeException($"Argument \"{nameof(index)}\" = {index} is out of range. Expected value between 0 and {_longCount - 1}");
			}

			if (index < --_longCount)
			{
				Helpers.CopyFollowingItems(this, index);
			}

			if (_nextItemIndex > 0)
			{
				if (--_nextItemIndex == 0)
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
#nullable disable
				new Span<T>(_memoryBlocks[_nextItemBlockIndex])[_nextItemIndex] = default;
#nullable restore
			}

			_version++;
		}

		public void Dispose()
		{
			_version++;
			if (_capacity > 0)
			{
				Clear();
				if (_memoryBlocks.Length >= RecyclableDefaults.MinPooledArrayLength)
				{
					// If anything, it has been already cleared by .Clear(), Remove() or RemoveAt() methods, as the list was modified / disposed.
					RecyclableArrayPool<T[]>.ReturnShared(_memoryBlocks, false);
				}
			}

			GC.SuppressFinalize(this);
		}
	}
}
