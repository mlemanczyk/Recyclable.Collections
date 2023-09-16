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
#pragma warning disable CA1825
		private static readonly T[][] _emptyMemoryBlocksArray = new T[0][];
#pragma warning restore CA1825
		private static readonly bool _defaultIsNull = default(T) == null;
		private static readonly bool _needsClearing = !typeof(T).IsValueType;

		internal int _blockSize;
		internal byte _blockSizePow2BitShift;
		internal int _blockSizeMinus1;
		internal long _capacity;
		internal int _lastBlockWithData = RecyclableDefaults.ItemNotFoundIndex;
		internal long _longCount;
#nullable disable
		internal T[][] _memoryBlocks;
#nullable restore
		internal int _nextItemBlockIndex;
		internal int _nextItemIndex;
		internal ulong _version;

		public int BlockSize => _blockSize;
		public int BlockSizeMinus1 => _blockSizeMinus1;
		public byte BlockSizePow2BitShift => _blockSizePow2BitShift;
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
		public int LastBlockWithData => _lastBlockWithData;
		public long LongCount => _longCount;
		public int NextItemBlockIndex => _nextItemBlockIndex;
		public int NextItemIndex => _nextItemIndex;
		public ulong Version => _version;

		private void AddRangeEnumerated(IEnumerable<T> items)
		{
			using IEnumerator<T> enumerator = items.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return;
			}

			long i, copied = _longCount;
			int blockSize = _blockSize,
				targetItemIdx = _nextItemIndex,
				targetBlockIdx = _nextItemBlockIndex;

			long capacity = _capacity;
			long available = capacity - copied;
			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			Span<T> blockArraySpan;

			while (true)
			{
				if (copied + blockSize > capacity)
				{
					capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)(capacity + blockSize))));
					memoryBlocksSpan = new(_memoryBlocks);
					available = capacity - copied;
				}

				blockArraySpan = new(memoryBlocksSpan[targetBlockIdx]);
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
						if (i == available)
						{
							break;
						}

						blockArraySpan = new(memoryBlocksSpan[targetBlockIdx]);
					}
				}

				copied += available;
			}
		}

		private void AddRangeWithKnownCount(IEnumerable<T> items, int requiredAdditionalCapacity)
		{
			long copied = _longCount;
			int blockSize = _blockSize;
			int targetItemIdx = _nextItemIndex;
			int targetBlockIdx = _nextItemBlockIndex;

			long requiredCapacity = copied + requiredAdditionalCapacity;
			if (requiredCapacity == 0)
			{
				return;
			}

			if (_capacity < requiredCapacity)
			{
				_capacity = Helpers.Resize(this, blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)requiredCapacity)));
				blockSize = _blockSize;
			}

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			Span<T> blockArraySpan = new(memoryBlocksSpan[targetBlockIdx]);
			foreach (var item in items)
			{
				blockArraySpan[targetItemIdx++] = item;
				if (targetItemIdx == blockSize)
				{
					targetBlockIdx++;
					targetItemIdx = 0;
					if (targetBlockIdx == memoryBlocksSpan.Length)
					{
						break;
					}

					blockArraySpan = new Span<T>(memoryBlocksSpan[targetBlockIdx]);
				}
			}

			_longCount = requiredCapacity;
			_nextItemBlockIndex = targetBlockIdx;
			_nextItemIndex = targetItemIdx;
			_lastBlockWithData = targetBlockIdx - (targetItemIdx > 0 ? 0 : 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public RecyclableLongList(int minBlockSize = RecyclableDefaults.BlockSize, long? initialCapacity = default)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			if (initialCapacity > 0)
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)initialCapacity.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				Helpers.SetupBlockArrayPooling(this, minBlockSize);
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}
		}

		public RecyclableLongList(in T[] source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.LongLength)));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(in Array source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.LongLength)));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(IReadOnlyList<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(ReadOnlySpan<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.Length)));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(RecyclableList<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(RecyclableLongList<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.Count)));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(Span<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.Length)));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(List<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(ICollection source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(ICollection<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			AddRange(source);
		}

		public RecyclableLongList(IEnumerable source, int minBlockSize = RecyclableDefaults.BlockSize, long? initialCapacity = default)
		{
			Helpers.SetupBlockArrayPooling(this, minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize)));
			if (initialCapacity > 0)
			{
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
				_blockSizeMinus1 = _blockSize - 1;
				_memoryBlocks = _emptyMemoryBlocksArray;
			}

			AddRange(source);
		}

		public RecyclableLongList(IEnumerable<T> source, int minBlockSize = RecyclableDefaults.BlockSize, long? initialCapacity = default)
		{
			Helpers.SetupBlockArrayPooling(this, minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize)));
			if (initialCapacity > 0)
			{
				_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity.Value)));
				_blockSizeMinus1 = minBlockSize - 1;
			}
			else
			{
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

		public void AddRange(in Array items)
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
				targetBlockIndex = _nextItemBlockIndex;

			long fullBlockItemsCount = items.LongLength - blockSize,
				 sourceItemIndex = Math.Min(blockSize - _nextItemIndex, items.LongLength);

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);

			Array.Copy(items, 0, memoryBlocksSpan[targetBlockIndex++], _nextItemIndex, sourceItemIndex);
			while (sourceItemIndex < fullBlockItemsCount)
			{
				Array.Copy(items, sourceItemIndex, memoryBlocksSpan[targetBlockIndex++], 0, blockSize);
				sourceItemIndex += blockSize;
			}

			// We're reusing a variable which is no longer needed. That's to avoid unnecessary
			// allocation.
			if ((blockSize = (int)(items.LongLength - sourceItemIndex)) > 0)
			{
				Array.Copy(items, sourceItemIndex, memoryBlocksSpan[targetBlockIndex], 0, blockSize);
				_lastBlockWithData = targetBlockIndex;
				_nextItemBlockIndex = targetBlockIndex;
				_nextItemIndex = blockSize;
			}
			else
			{
				_lastBlockWithData = targetBlockIndex - 1;
				if (_nextItemIndex + sourceItemIndex >= _blockSize)
				{
					_nextItemBlockIndex = targetBlockIndex;
					_nextItemIndex = 0;
				}
				else
				{
					_nextItemIndex += (int)sourceItemIndex;
				}
			}

			_longCount = targetCapacity;
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
				targetBlockIndex = _nextItemBlockIndex;

			Span<T> itemsSpan = new(items);
			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
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
				targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex]);
			}

			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(targetBlockArraySpan);
				if (_nextItemBlockIndex != targetBlockIndex)
				{
					_nextItemIndex = itemsSpan.Length;
					_nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					_nextItemIndex += itemsSpan.Length;
				}
			}
			else
			{
				_nextItemBlockIndex = targetBlockIndex + 1;
				_nextItemIndex = 0;
			}

			_longCount = targetCapacity;
			_lastBlockWithData = targetBlockIndex;
			_version++;
		}

		public void AddRange(RecyclableList<T> items)
		{
			if (items._count == 0)
			{
				return;
			}

			long targetCapacity = _longCount + items._count;
			if (_capacity < targetCapacity)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex;

			Span<T> itemsSpan = new(items._memoryBlock, 0, items._count);
			Span<T[]> targetMemoryBlocksSpan = new(_memoryBlocks);
			Span<T> targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			while (itemsSpan.Length >= targetBlockArraySpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				targetBlockIndex++;
				if (itemsSpan.Length == 0)
				{
					break;
				}

				targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex]);
			}

			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(targetBlockArraySpan);
				_lastBlockWithData = targetBlockIndex;
				if (_nextItemBlockIndex != targetBlockIndex)
				{
					_nextItemIndex = itemsSpan.Length;
					_nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					_nextItemIndex += itemsSpan.Length;
				}
			}
			else
			{
				_lastBlockWithData = targetBlockIndex - 1;
				_nextItemBlockIndex = targetBlockIndex;
				_nextItemIndex = 0;
			}

			_longCount = targetCapacity;
			_version++;
		}

		public void AddRange(ReadOnlySpan<T> items)
		{
			if (items.Length == 0)
			{
				return;
			}

			long targetCapacity = _longCount + items.Length;
			if (_capacity < targetCapacity)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex;

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			while (items.Length >= targetBlockArraySpan.Length)
			{
				items[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				items = items[targetBlockArraySpan.Length..];
				if (items.Length == 0)
				{
					break;
				}

				targetBlockIndex++;
				targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex]);
			}

			if (items.Length > 0)
			{
				items.CopyTo(targetBlockArraySpan);
				if (_nextItemBlockIndex != targetBlockIndex)
				{
					_nextItemIndex = items.Length;
					_nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					_nextItemIndex += items.Length;
				}
			}
			else
			{
				_nextItemBlockIndex = targetBlockIndex + 1;
				_nextItemIndex = 0;
			}

			_longCount = targetCapacity;
			_lastBlockWithData = targetBlockIndex;
			_version++;
		}

		public void AddRange(RecyclableLongList<T> items)
		{
			if (items._longCount == 0)
			{
				return;
			}

			long itemsCount = items._longCount,
				targetCapacity = _longCount + itemsCount;

			if (_capacity < targetCapacity)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			int blockSize = _blockSize,
				sourceBlockIndex = 0,
				targetBlockIndex = _nextItemBlockIndex,
				toCopy;

			Span<T[]> sourceMemoryBlocksSpan = new(items._memoryBlocks);
			Span<T> itemsSpan = new(sourceMemoryBlocksSpan[sourceBlockIndex]);
			Span<T[]> targetMemoryBlocksSpan = new(_memoryBlocks);
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
					targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex]);
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
						itemsSpan = new(sourceMemoryBlocksSpan[sourceBlockIndex]);
					}

					copiedCount += toCopy;
					if (targetBlockArraySpan.IsEmpty && copiedCount < itemsCount)
					{
						targetBlockIndex++;
						targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex]);
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

		public void AddRange(Span<T> items)
		{
			if (items.Length == 0)
			{
				return;
			}

			long targetCapacity = _longCount + items.Length;
			if (_capacity < targetCapacity)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			int blockSize = _blockSize,
				targetBlockIndex = _nextItemBlockIndex;

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
			while (items.Length >= targetBlockArraySpan.Length)
			{
				items[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				items = items[targetBlockArraySpan.Length..];
				if (items.Length == 0)
				{
					break;
				}

				targetBlockIndex++;
				targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex]);
			}

			if (items.Length > 0)
			{
				items.CopyTo(targetBlockArraySpan);
				if (_nextItemBlockIndex != targetBlockIndex)
				{
					_nextItemIndex = items.Length;
					_nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					_nextItemIndex += items.Length;
				}
			}
			else
			{
				_nextItemBlockIndex = targetBlockIndex + 1;
				_nextItemIndex = 0;
			}

			_longCount = targetCapacity;
			_lastBlockWithData = targetBlockIndex;
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

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
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

		public void AddRange(ICollection items)
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
				targetBlockIndex = _nextItemBlockIndex;

			// We're better off to temporarily copy it to a fixed array,
			// than copying them item by item. We may run into OOMs, though.
			T[] itemsBuffer = items.Count >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T>.RentShared((int)BitOperations.RoundUpToPowerOf2((uint)items.Count)) : new T[items.Count];
			try
			{
				items.CopyTo(itemsBuffer, 0);

				Span<T> itemsSpan = new(itemsBuffer, 0, items.Count);
				Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
				Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
				while (targetBlockArraySpan.Length <= itemsSpan.Length)
				{
					itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
					itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
					targetBlockIndex++;
					if (itemsSpan.Length == 0)
					{
						break;
					}

					targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex]);
				}

				if (itemsSpan.Length > 0)
				{
					itemsSpan.CopyTo(targetBlockArraySpan);
					if (_nextItemBlockIndex != targetBlockIndex)
					{
						_nextItemIndex = itemsSpan.Length;
						_nextItemBlockIndex = targetBlockIndex;
					}
					else
					{
						_nextItemIndex += itemsSpan.Length;
					}
				}
				else
				{
					_nextItemIndex = 0;
				}

				_longCount = targetCapacity;
				_nextItemBlockIndex = targetBlockIndex;
				_lastBlockWithData = targetBlockIndex - (itemsSpan.Length > 0 ? 0 : 1);
			}
			finally
			{
				if (items.Count >= RecyclableDefaults.MinPooledArrayLength)
				{
					RecyclableArrayPool<T>.ReturnShared(itemsBuffer, _needsClearing);
				}

				_version++;
			}
		}

		public void AddRange(ICollection<T> items)
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
				targetBlockIndex = _nextItemBlockIndex;

			// We're better off to temporarily copy it to a fixed array,
			// than copying them item by item. We may run into OOMs, though.
			T[] itemsBuffer = items.Count >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T>.RentShared((int)BitOperations.RoundUpToPowerOf2((uint)items.Count)) : new T[items.Count];
			try
			{
				items.CopyTo(itemsBuffer, 0);

				Span<T> itemsSpan = new(itemsBuffer, 0, items.Count);
				Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
				Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], _nextItemIndex, blockSize - _nextItemIndex);
				while (targetBlockArraySpan.Length <= itemsSpan.Length)
				{
					itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
					itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
					targetBlockIndex++;
					if (itemsSpan.Length == 0)
					{
						break;
					}

					targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex]);
				}

				if (itemsSpan.Length > 0)
				{
					itemsSpan.CopyTo(targetBlockArraySpan);
					if (_nextItemBlockIndex != targetBlockIndex)
					{
						_nextItemIndex = itemsSpan.Length;
						_nextItemBlockIndex = targetBlockIndex;
					}
					else
					{
						_nextItemIndex += itemsSpan.Length;
					}
				}
				else
				{
					_nextItemIndex = 0;
				}

				_longCount = targetCapacity;
				_nextItemBlockIndex = targetBlockIndex;
				_lastBlockWithData = targetBlockIndex - (itemsSpan.Length > 0 ? 0 : 1);
			}
			finally
			{
				if (items.Count >= RecyclableDefaults.MinPooledArrayLength)
				{
					RecyclableArrayPool<T>.ReturnShared(itemsBuffer, _needsClearing);
				}

				_version++;
			}
		}

		public void AddRange(IEnumerable source)
		{
			long i, copied = _longCount;
			int blockSize = _blockSize,
				targetItemIdx = _nextItemIndex,
				targetBlockIdx = _nextItemBlockIndex;

			IEnumerator enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return;
			}

			long capacity = _capacity;
			long available = capacity - copied;

			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			Span<T> blockArraySpan;
			while (true)
			{
				if (copied + blockSize > capacity)
				{
					capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)(capacity + blockSize))));
					memoryBlocksSpan = new(_memoryBlocks);
					available = capacity - copied;
				}

				blockArraySpan = new(memoryBlocksSpan[targetBlockIdx]);
				for (i = 1; i <= available; i++)
				{
					blockArraySpan[targetItemIdx++] = (T)enumerator.Current;

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

						if (i == available)
						{
							break;
						}

						blockArraySpan = new(memoryBlocksSpan[targetBlockIdx]);
					}
				}

				copied += available;
			}
		}

		public void AddRange(IEnumerable<T> items)
		{
			if (items is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(sourceRecyclableList);
			}
			else if (items is RecyclableLongList<T> sourceRecyclableLongList)
			{
				AddRange(sourceRecyclableLongList);
			}
			else if (items is T[] sourceArray)
			{
				AddRange(sourceArray);
			}
			else if (items is Array sourceArrayWithObjects)
			{
				AddRange(sourceArrayWithObjects);
			}
			else if (items is List<T> sourceList)
			{
				AddRange(sourceList);
			}
			else if (items is ICollection<T> sourceICollection)
			{
				AddRange(sourceICollection);
			}
			else if (items is ICollection sourceICollectionWithObjects)
			{
				AddRange(sourceICollectionWithObjects);
			}
			else if (items is IReadOnlyList<T> sourceIReadOnlyList)
			{
				AddRange(sourceIReadOnlyList);
			}
			else if (items.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
			{
				AddRangeWithKnownCount(items, requiredAdditionalCapacity);
				_version++;
			}
			else
			{
				AddRangeEnumerated(items);
				_version++;
			}
		}

		public void AddRange(IReadOnlyList<T> items)
		{
			long copied = _longCount;
			int blockSize = _blockSize;
			int targetItemIdx = _nextItemIndex;
			int targetBlockIdx = _nextItemBlockIndex;

			Span<T[]> memoryBlocksSpan;
			Span<T> blockArraySpan;
			long sourceItemsCount = items.Count;
			long requiredCapacity = copied + sourceItemsCount;
			if (requiredCapacity == 0)
			{
				return;
			}

			if (_capacity < requiredCapacity)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)requiredCapacity)));
				blockSize = _blockSize;
			}

			memoryBlocksSpan = new(_memoryBlocks);
			blockArraySpan = new(memoryBlocksSpan[targetBlockIdx]);
			for(var itemIndex = 0; itemIndex < sourceItemsCount; itemIndex++)
			{
				blockArraySpan[targetItemIdx++] = items[itemIndex];
				if (targetItemIdx == blockSize)
				{
					targetItemIdx = 0;
					targetBlockIdx++;
					if (itemIndex + 1 == sourceItemsCount)
					{
						break;
					}

					blockArraySpan = new Span<T>(memoryBlocksSpan[targetBlockIdx]);
				}
			}

			_longCount = requiredCapacity;
			_nextItemBlockIndex = targetBlockIdx;
			_nextItemIndex = targetItemIdx;
			_lastBlockWithData = targetBlockIdx - (targetItemIdx > 0 ? 0 : 1);
		}

		public void Clear()
		{
			if (_longCount == 0)
			{
				return;
			}

			T[][] memoryBlocks = _memoryBlocks;
			int toRemoveIdx = 0;

			if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
			{
				while (toRemoveIdx < memoryBlocks.Length && memoryBlocks[toRemoveIdx] != null)
				{
					RecyclableArrayPool<T>.ReturnShared(memoryBlocks[toRemoveIdx++], _needsClearing);
				}
			}
			else if (_needsClearing)
			{
				while (toRemoveIdx < memoryBlocks.Length && memoryBlocks[toRemoveIdx] != null)
				{
					Array.Clear(memoryBlocks[toRemoveIdx++]);
				}
			}

			// TODO: Do we want to initialize with empty arrays or null refs? Null refs may be faster.
			Array.Fill(_memoryBlocks, null);
			_capacity = 0;
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

		public void Insert(int index, T item)
		{
			if (index > _longCount || index < 0)
			{
				Helpers.ThrowIndexOutOfRangeException($"Argument \"{nameof(index)}\" = {index} is out of range. Expected value between 0 and {_longCount}");
			}

			if (_capacity < ++_longCount)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)_longCount)));
			}

			if (index + 1 < _longCount)
			{
				Helpers.MakeRoomAndSet(this, index, item);
			}
			else
			{
				new Span<T>(_memoryBlocks[_nextItemBlockIndex])[_nextItemIndex] = item;
			}

			if (++_nextItemIndex == _blockSize)
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

			_version++;
		}

		public void Insert(long index, T item)
		{
			if (index > _longCount || index < 0)
			{
				Helpers.ThrowIndexOutOfRangeException($"Argument \"{nameof(index)}\" = {index} is out of range. Expected value between 0 and {_longCount}");
			}

			if (_capacity < ++_longCount)
			{
				_capacity = Helpers.Resize(this, _blockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)_longCount)));
			}

			if (index + 1 < _longCount)
			{
				Helpers.MakeRoomAndSet(this, index, item);
			}
			else
			{
				new Span<T>(_memoryBlocks[_nextItemBlockIndex])[_nextItemIndex] = item;
			}

			if (++_nextItemIndex == _blockSize)
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

			_version++;
		}

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
					_nextItemIndex--;
				}
				else
				{
					_nextItemIndex = _blockSizeMinus1;
					_nextItemBlockIndex--;
				}

				if (_nextItemIndex == 0)
				{
					_lastBlockWithData--;
				}

				if (_needsClearing)
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
				RecyclableArrayPool<T>.ReturnShared(_memoryBlocks[index], _needsClearing);
			}

			_memoryBlocks[index] = null;
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
				_nextItemIndex--;
			}
			else
			{
				_nextItemIndex = _blockSizeMinus1;
				_nextItemBlockIndex--;
			}

			if (_nextItemIndex == 0)
			{
				_lastBlockWithData--;
			}

			if (_needsClearing)
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
				_nextItemIndex--;
			}
			else
			{
				_nextItemIndex = _blockSizeMinus1;
				_nextItemBlockIndex--;
			}

			if (_nextItemIndex == 0)
			{
				_lastBlockWithData--;
			}

			if (_needsClearing)
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
