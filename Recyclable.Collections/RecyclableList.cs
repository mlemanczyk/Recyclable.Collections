using System;
using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableList<T> : IDisposable, IList<T>
	{
		private static readonly ArrayPool<T[]> _arrayPool = ArrayPool<T[]>.Create();
		private static readonly ArrayPool<T> _blockArrayPool = ArrayPool<T>.Create();
		private static readonly T[][] _emptyBlockArray = new T[0][];
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
		private readonly int _blockSize;
		protected T[][] _memoryBlocks;

		private long _capacity;
		public long Capacity
		{
			get => _capacity;
			protected set => _capacity = value;
		}

		public int Count => (int)LongCount;

		protected long _longCount;
		public long LongCount
		{
			get => _longCount;
			set => _longCount = value;
		}

		public bool IsReadOnly { get; } = false;
		public int BlockCount => _memoryBlocks.Length;

		private static void RemoveAt(RecyclableList<T> list, long index)
		{
			long oldCount = list._longCount;
			long oldCountMinus1 = oldCount - 1;
			if (index != oldCountMinus1)
			{
				ThrowArgumentOutOfRangeException();
			}

			list._longCount--;
			int blockSize = list._blockSize;
			if ((list._capacity * blockSize) - oldCountMinus1 == blockSize)
			{
				T[][] memoryBlocks = list._memoryBlocks;
				ReleaseBlock(list, memoryBlocks, blockSize, memoryBlocks.Length - 1);
			}
		}

		private static void ThrowArgumentOutOfRangeException()
		{
			throw new ArgumentOutOfRangeException("index");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static T[][] SetNewLength(in T[][]? source, in int blockSize, in long newSize)
		{
			ArrayPool<T[]> arrayPool = _arrayPool;
			ArrayPool<T> blockArrayPool = _blockArrayPool;
			var sourceBlockCount = source?.Length ?? 0;
			int requiredBlockCount = (int)(newSize / blockSize) + (newSize % blockSize > 0 ? 1 : 0);
			if (requiredBlockCount == sourceBlockCount)
			{
				return source!;
			}

			Span<T[]> newArraySpan;
			T[][] newMemoryBlocks = requiredBlockCount.RentArrayFromPool(arrayPool);
			switch (requiredBlockCount >= sourceBlockCount)
			{
				case true:
					if (sourceBlockCount > 0)
					{
						Memory<T[]> sourceMemory = new(source);
						Memory<T[]> newArrayMemory = new(newMemoryBlocks);
						sourceMemory.CopyTo(newArrayMemory);
						source!.ReturnToPool(arrayPool);
						newArraySpan = new Span<T[]>(newMemoryBlocks)[sourceBlockCount..];
					}
					else
					{
						newArraySpan = new Span<T[]>(newMemoryBlocks);
					}

					for (int i = 0; i < newArraySpan.Length; i++)
					{
						newArraySpan[i] = blockSize.RentArrayFromPool(blockArrayPool);
					}

					return newMemoryBlocks;

				case false:
					var sourceSpan = new Span<T[]>(source)[..requiredBlockCount];
					newArraySpan = new Span<T[]>(newMemoryBlocks);
					sourceSpan.CopyTo(newArraySpan);
					sourceSpan = new Span<T[]>(source)[newArraySpan.Length..];
					for (int i = 0; i < sourceSpan.Length; i++)
					{
						sourceSpan[i].ReturnToPool(_blockArrayPool);
					}

					source!.ReturnToPool(arrayPool);
					return newMemoryBlocks;
			}
		}

		protected long EnsureCapacity(in long requestedCapacity)
		{
			long oldCapacity = _capacity;
			ref T[][] memory = ref _memoryBlocks;

			long newCapacity;
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

			int blockSize = _blockSize;
			memory = SetNewLength(memory, blockSize, newCapacity);
			newCapacity = memory.Length * blockSize;
			_capacity = newCapacity;
			return newCapacity;
		}

#pragma warning disable CS8618 // _memoryBlocks will be initialized when the 1st item is added
		public RecyclableList(int blockSize = RecyclableDefaults.BlockSize, long? initialCapacity = default)
#pragma warning restore CS8618
		{
			_blockSize = blockSize;
			if (initialCapacity > 0)
			{
				_memoryBlocks = SetNewLength(_memoryBlocks, blockSize, initialCapacity.Value);
				_capacity = _memoryBlocks.Length * blockSize;
			}
			else
			{
				_memoryBlocks = _emptyBlockArray;
			}
		}

#pragma warning disable CS8618 // _memoryBlocks will be initialized when the 1st item is added
		public RecyclableList(IEnumerable<T> source, int blockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
#pragma warning restore CS8618
		{
			_blockSize = blockSize;
			if (expectedItemsCount > 0)
			{
				_memoryBlocks = SetNewLength(_memoryBlocks, blockSize, expectedItemsCount.Value);
				_capacity = _memoryBlocks.Length * blockSize;
			}
			else
			{
				_memoryBlocks = _emptyBlockArray;
			}

			AddRange(this, source);
		}

		public T this[long index]
		{
			get => _memoryBlocks[(int)(index / _blockSize)][index % _blockSize];
			set => new Span<T>(_memoryBlocks[(int)(index / _blockSize)])[(int)index % _blockSize] = value;
		}

		public T this[int index]
		{
			get => _memoryBlocks[index / _blockSize][index % _blockSize];
			set => new Span<T>(_memoryBlocks[index / _blockSize])[index % _blockSize] = value;
		}

		public void Add(T item)
		{
			int blockSize = _blockSize;
			long oldCount = _longCount;
			long requiredCapacity = oldCount + 1;
			if (_capacity < requiredCapacity)
			{
				_ = EnsureCapacity(requiredCapacity);
			}

			_memoryBlocks[(int)(oldCount / blockSize)][oldCount % blockSize] = item;
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
			var memoryBlockSpan = new Span<T[]>(_memoryBlocks);
			var targetBlockSpan = new Span<T>(memoryBlockSpan[targetBlockIdx]);
			for (long i = 0; i < sourceItemsCount; i++)
			{
				targetBlockSpan[targetItemIdx++] = items[i];
				if (targetItemIdx == blockSize)
				{
					targetBlockIdx++;
					targetBlockSpan = new Span<T>(memoryBlockSpan[targetBlockIdx]);
					targetItemIdx = 0;
				}
			}

			_longCount = targetCapacity;
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
			var memoryBlockSpan = new Span<T[]>(_memoryBlocks);
			var targetBlockSpan = new Span<T>(memoryBlockSpan[targetBlockIdx]);
			for (int i = 0; i < sourceItemsCount; i++)
			{
				targetBlockSpan[targetItemIdx++] = items[i];
				if (targetItemIdx == blockSize)
				{
					targetBlockIdx++;
					targetBlockSpan = new Span<T>(memoryBlockSpan[targetBlockIdx]);
					targetItemIdx = 0;
				}
			}

			_longCount = targetCapacity;
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
			var memoryBlockSpan = new Span<T[]>(_memoryBlocks);
			var targetBlockSpan = new Span<T>(memoryBlockSpan[targetBlockIdx]);
			for (int i = 0; i < sourceItemsCount; i++)
			{
				targetBlockSpan[targetItemIdx++] = items[i];
				if (targetItemIdx == blockSize)
				{
					targetBlockIdx++;
					targetBlockSpan = new Span<T>(memoryBlockSpan[targetBlockIdx]);
					targetItemIdx = 0;
				}
			}

			_longCount = targetCapacity;
		}

		private static void AddRange(RecyclableList<T> destination, IEnumerable<T> source)
		{
			foreach (var item in source)
			{
				destination.Add(item);
			}
		}

		public void Clear()
		{
			try
			{
				// Remove in reversed order for performance savings
				var toRemoveIdx = _memoryBlocks.Length - 1;
				while (toRemoveIdx >= 0)
				{
					try
					{
						ReleaseBlock(this, _memoryBlocks, _blockSize, toRemoveIdx);
					}
					catch (Exception)
					{
						// We want to try returning as many arrays, as possible, before
						// the list is cleared.
					}

					toRemoveIdx--;
				}
			}
			finally
			{
				_longCount = 0;
			}
		}

		public bool Contains(T item) => _memoryBlocks.Any(x => x.Contains(item));

		public void CopyTo(T[] array, int arrayIndex) => _memoryBlocks.CopyTo(0, _blockSize, (int)LongCount % _blockSize, array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		public int IndexOf(T item) => (int)_memoryBlocks.LongIndexOf(_longCount, item, _equalityComparer);
		public void Insert(int index, T item) => throw new NotSupportedException();
		public long LongIndexOf(T item) => _memoryBlocks.LongIndexOf(_blockSize, item, _equalityComparer);
		public bool Remove(T item) => throw new NotSupportedException();
		public void RemoveBlock(int index) => ReleaseBlock(this, _memoryBlocks, _blockSize, index);
		public void RemoveAt(int index) => RemoveAt(this, index);
		public void RemoveAt(long index) => RemoveAt(this, index);

		IEnumerator IEnumerable.GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		private static void ReleaseBlock(RecyclableList<T> owner, in T[][] memoryBlocks, int blockSize, int blockIndex)
		{
			try
			{
				memoryBlocks[blockIndex].ReturnToPool(_blockArrayPool);
			}
			finally
			{
				owner._capacity -= blockSize;
			}
		}

		//private static RecyclableArrayList<T[]> SetupMemoryBlocks(RecyclableList<T> owner, int blockSize, long? totalItemsCount)
		//{
		//	if (blockSize <= 0)
		//	{
		//		return new();
		//	}

		//	totalItemsCount ??= 8 * blockSize;
		//	int additionalArray = totalItemsCount % blockSize > 0 ? 1 : 0;
		//	int memoryBlockCount = (int)(totalItemsCount.Value / blockSize).LimitTo(int.MaxValue) + additionalArray;
		//	RecyclableArrayList<T[]> memoryBlocks = new(memoryBlockCount);
		//	for (var blockIdx = 0; blockIdx < memoryBlockCount; blockIdx++)
		//	{
		//		memoryBlocks[blockIdx] = RentArrayFromPool(blockSize);
		//	}

		//	memoryBlocks.Count = memoryBlockCount;
		//	owner.Capacity = totalItemsCount.Value;
		//	return memoryBlocks;
		//}


		public void Dispose()
		{
			if (_capacity > 0)
			{
				Clear();
				_capacity = 0;
				_memoryBlocks.ReturnToPool(_arrayPool);
				GC.SuppressFinalize(this);
			}
		}
	}
}
