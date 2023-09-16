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
		private static readonly bool _defaultIsNull = default(T) == null;
		internal static readonly bool _needsClearing = !typeof(T).IsValueType;

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

		public RecyclableLongList(int minBlockSize = RecyclableDefaults.BlockSize, long initialCapacity = RecyclableDefaults.InitialCapacity)
		{			
			if (!BitOperations.IsPow2(minBlockSize))
			{
				minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));
			}

			_blockSizePow2BitShift = (byte)(31 - BitOperations.LeadingZeroCount((uint)minBlockSize));
			_capacity = RecyclableLongList<T>.Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)initialCapacity)));
			_blockSize = minBlockSize;
			_blockSizeMinus1 = minBlockSize - 1;
		}

		public RecyclableLongList(in T[] source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.LongLength)));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
		}

		public RecyclableLongList(in Array source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.LongLength)));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
		}

		public RecyclableLongList(IReadOnlyList<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
		}

		public RecyclableLongList(ReadOnlySpan<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.Length)));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
		}

		public RecyclableLongList(RecyclableList<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
		}

		public RecyclableLongList(RecyclableLongList<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.Count)));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
		}

		public RecyclableLongList(Span<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)source.Length)));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
		}

		public RecyclableLongList(List<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
		}

		public RecyclableLongList(ICollection source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
		}

		public RecyclableLongList(ICollection<T> source, int minBlockSize = RecyclableDefaults.BlockSize)
		{
			minBlockSize = checked((int)BitOperations.RoundUpToPowerOf2((uint)minBlockSize));

			Helpers.SetupBlockArrayPooling(this, minBlockSize);
			_capacity = Helpers.Resize(this, minBlockSize, _blockSizePow2BitShift, BitOperations.RoundUpToPowerOf2((uint)source.Count));
			_blockSizeMinus1 = minBlockSize - 1;

			zRecyclableLongListAddRange.AddRange(this, source);
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

			zRecyclableLongListAddRange.AddRange(this, source);
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

			zRecyclableLongListAddRange.AddRange(this, source);
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
