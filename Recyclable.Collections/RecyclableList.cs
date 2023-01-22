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
				memoryBlocks[^1].ReturnToPool(_blockArrayPool);
				list._capacity -= blockSize;
			}
		}

		private static void ThrowArgumentOutOfRangeException()
		{
			throw new ArgumentOutOfRangeException("index");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static T[][] SetNewLength(in T[][]? source, in int blockSize, in long newCapacity)
		{
			ArrayPool<T[]> arrayPool = _arrayPool;
			ArrayPool<T> blockArrayPool = _blockArrayPool;
			var sourceBlockCount = source?.Length ?? 0;
			int requiredBlockCount = (int)(newCapacity / blockSize) + (newCapacity % blockSize > 0 ? 1 : 0);
			if (requiredBlockCount == sourceBlockCount)
			{
				return source!;
			}

			Span<T[]> newArraySpan;
			T[][] newMemoryBlocks = requiredBlockCount.RentArrayFromPool(arrayPool);
			switch (requiredBlockCount >= sourceBlockCount)
			{
				case true:
					switch (sourceBlockCount > 0)
					{
						case true:
							Memory<T[]> sourceMemory = new(source);
							Memory<T[]> newArrayMemory = new(newMemoryBlocks);
							sourceMemory.CopyTo(newArrayMemory);
							source!.ReturnToPool(arrayPool);
							newArraySpan = new Span<T[]>(newMemoryBlocks)[sourceBlockCount..];
							break;

						case false:
							newArraySpan = new Span<T[]>(newMemoryBlocks);
							break;
					}

					int uninitializedBlocksCount = newArraySpan.Length;
					for (int i = 0; i < uninitializedBlocksCount; i++)
					{
						newArraySpan[i] = blockSize.RentArrayFromPool(blockArrayPool);
					}

					return newMemoryBlocks;

				case false:
					var sourceSpan = new Span<T[]>(source)[..requiredBlockCount];
					newArraySpan = new Span<T[]>(newMemoryBlocks);
					sourceSpan.CopyTo(newArraySpan);
					sourceSpan = new Span<T[]>(source)[newArraySpan.Length..];
					int sourceLength = sourceSpan.Length;
					for (int i = 0; i < sourceLength; i++)
					{
						sourceSpan[i].ReturnToPool(blockArrayPool);
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

		public RecyclableList(int blockSize = RecyclableDefaults.BlockSize, long? initialCapacity = default)
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

		public RecyclableList(IEnumerable<T> source, int blockSize = RecyclableDefaults.BlockSize, long? expectedItemsCount = default)
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

		public void Add(T item)
		{
			int blockSize = _blockSize;
			long oldCount = _longCount;
			long requiredCapacity = oldCount + 1;
			if (_capacity < requiredCapacity)
			{
				_ = EnsureCapacity(requiredCapacity);
			}

			_memoryBlocks[(int)(oldCount / blockSize)][(int)(oldCount % blockSize)] = item;
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
			var memoryBlocksSpan = new Span<T[]>(_memoryBlocks);
			var memoryBlocksCount = memoryBlocksSpan.Length;
			var targetBlockSpan = new Span<T>(memoryBlocksSpan[targetBlockIdx]);
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
			var memoryBlocksSpan = new Span<T[]>(_memoryBlocks);
			var memoryBlocksCount = memoryBlocksSpan.Length;
			var targetBlockSpan = new Span<T>(memoryBlocksSpan[targetBlockIdx]);
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
			var memoryBlocksSpan = new Span<T[]>(_memoryBlocks);
			var memoryBlocksCount = memoryBlocksSpan.Length;
			var targetBlockSpan = new Span<T>(memoryBlocksSpan[targetBlockIdx]);
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
			int targetBlockIdx = (int)(targetItemIdx / blockSize) + (targetItemIdx > 0 ? 1 : 0);

			Span<T[]> memoryBlocksSpan;
			Span<T> blockArraySpan;
			if (source.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
			{

				int requiredCapacity = targetItemIdx + requiredAdditionalCapacity;
				if (capacity < requiredCapacity)
				{
					_ = EnsureCapacity(requiredCapacity);
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

				_longCount = targetItemIdx;
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
		}

		public void Clear()
		{
			ArrayPool<T> blockArrayPool = _blockArrayPool;
			Span<T[]> memoryBlocksSpan = new(_memoryBlocks);
			int memoryBlocksCount = memoryBlocksSpan.Length;
			for (var toRemoveIdx = 0; toRemoveIdx < memoryBlocksCount; toRemoveIdx++)
			{
				memoryBlocksSpan[toRemoveIdx].ReturnToPool(blockArrayPool);
			}

			_capacity = 0;
			_longCount = 0;
		}

		public bool Contains(T item) => _memoryBlocks.Any(x => x.Contains(item));

		public void CopyTo(T[] array, int arrayIndex) => _memoryBlocks.CopyTo(0, _blockSize, (int)LongCount % _blockSize, array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		public int IndexOf(T item) => (int)_memoryBlocks.LongIndexOf(_longCount, item, _equalityComparer);
		public void Insert(int index, T item) => throw new NotSupportedException();
		public long LongIndexOf(T item) => _memoryBlocks.LongIndexOf(_blockSize, item, _equalityComparer);
		public bool Remove(T item) => throw new NotSupportedException();
		public void RemoveBlock(int index)
		{
			_memoryBlocks[index].ReturnToPool(_blockArrayPool);
			_capacity -= _blockSize;
		}

		public void RemoveAt(int index) => RemoveAt(this, index);
		public void RemoveAt(long index) => RemoveAt(this, index);

		IEnumerator IEnumerable.GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

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
