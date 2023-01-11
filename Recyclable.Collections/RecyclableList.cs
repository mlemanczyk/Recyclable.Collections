using System.Buffers;
using System.Collections;

namespace Recyclable.Collections
{
	public class RecyclableList<T> : IDisposable, IList<T>
	{
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
		private const int _minPooledArraySize = 100;
		private readonly int _blockSize;
		private readonly RecyclableArrayList<T[]> _memoryBlocks;

		private static void AddCapacity(RecyclableList<T> list, RecyclableArrayList<T[]> memoryBlocks, int additionalCapacity)
		{
			T[] newMemoryBlock = RentArrayFromPool(additionalCapacity);
			memoryBlocks.Add(newMemoryBlock);
			list.Capacity += additionalCapacity;
		}

		private static void AddRange(RecyclableList<T> destination, IEnumerable<T> source)
		{
			foreach (var item in source)
			{
				destination.Add(item);
			}
		}

		private static void RemoveAt(RecyclableList<T> list, long index)
		{
			if (index == list.LongCount - 1)
			{
				list.LongCount--;
				if (list.Capacity - list.LongCount == list._blockSize)
				{
					list.RemoveBlock(list.BlockCount - 1);
				}
			}
			else
			{
				throw new NotSupportedException("Only removal of the last element is supported");
			}
		}

		private static T[] RentArrayFromPool(int minSize) => (minSize >= _minPooledArraySize)
			? ArrayPool<T>.Shared.Rent(minSize)
			: new T[minSize];

		private static void ReturnToPool(in T[] array)
		{
			if (array.LongLength is < _minPooledArraySize or > int.MaxValue)
			{
				return;
			}

			ArrayPool<T>.Shared.Return(array);
		}

		private static void RemoveBlock(RecyclableList<T> owner, RecyclableArrayList<T[]> memoryBlocks, int blockSize, int index)
		{
			try
			{
				ReturnToPool(memoryBlocks[index]);
				memoryBlocks.RemoveAt(index);
			}
			finally
			{
				owner.Capacity -= blockSize;
			}
		}

		private static RecyclableArrayList<T[]> SetupMemoryBlocks(RecyclableList<T> owner, int blockSize, long? totalItemsCount)
		{
			totalItemsCount ??= 8 * blockSize;
			int additionalArray = totalItemsCount % blockSize > 0 ? 1 : 0;
			int memoryBlockCount = (int)(totalItemsCount.Value / blockSize).LimitTo(int.MaxValue) + additionalArray;
			RecyclableArrayList<T[]> memoryBlocks = new(memoryBlockCount);
			for (var blockIdx = 0; blockIdx < memoryBlockCount; blockIdx++)
			{
				memoryBlocks[blockIdx] = RentArrayFromPool(blockSize);
			}

			memoryBlocks.Count = memoryBlockCount;
			owner.Capacity = totalItemsCount.Value;
			return memoryBlocks;
		}


		public RecyclableList(int blockSize = RecyclableDefaults.BlockSize, long? totalItemsCount = default)
		{
			_blockSize = blockSize;
			_memoryBlocks = SetupMemoryBlocks(this, blockSize, totalItemsCount);
		}

		public RecyclableList(IEnumerable<T> source, int blockSize = RecyclableDefaults.BlockSize, long? totalItemsCount = default)
		{
			_blockSize = blockSize;
			_memoryBlocks = SetupMemoryBlocks(this, blockSize, totalItemsCount);
			AddRange(this, source);
		}

		public T this[long index]
		{
			get => _memoryBlocks[(int)(index / _blockSize)][index % _blockSize];
			set => _memoryBlocks[(int)(index / _blockSize)][index % _blockSize] = value;
		}

		public T this[int index]
		{
			get => _memoryBlocks[index / _blockSize][index % _blockSize];
			set => _memoryBlocks[index / _blockSize][index % _blockSize] = value;
		}

		public long Capacity { get; protected set; }
		public int Count => (int)LongCount;
		public long LongCount { get; set; }
		public bool IsReadOnly { get; } = false;
		public int BlockCount => _memoryBlocks.Count;

		public void Add(T item)
		{
			long oldCount = LongCount;
			int blockSize = _blockSize;
			if (oldCount == Capacity)
			{
				AddCapacity(this, _memoryBlocks, blockSize);
			}

			_memoryBlocks[(int)(oldCount / blockSize)][oldCount % blockSize] = item;
			LongCount++;
		}

		public void Clear()
		{
			try
			{
				// Remove in reversed order for performance savings
				while (_memoryBlocks.Count > 0)
				{
					try
					{
						RemoveBlock(this, _memoryBlocks, _blockSize, _memoryBlocks.Count - 1);
					}
					catch (Exception)
					{
						// We want to try returning as many arrays, as possible, before
						// the list is cleared.
					}
				}
			}
			finally
			{
				LongCount = 0;
				_memoryBlocks.Clear();
			}
		}

		public bool Contains(T item) => _memoryBlocks.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => _memoryBlocks.CopyTo(0, _blockSize, (int)LongCount % _blockSize, array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		public int IndexOf(T item) => (int)_memoryBlocks.LongIndexOf(_blockSize, item, _equalityComparer);

		public void Insert(int index, T item) => throw new NotSupportedException();

		public long LongIndexOf(T item) => _memoryBlocks.LongIndexOf(_blockSize, item, _equalityComparer);

		public bool Remove(T item) => throw new NotSupportedException();

		public void RemoveAt(int index) => RemoveAt(this, index);
		public void RemoveAt(long index) => RemoveAt(this, index);

		IEnumerator IEnumerable.GetEnumerator() => _memoryBlocks.Enumerate(_blockSize, LongCount).GetEnumerator();

		public void Dispose()
		{
			Clear();
			GC.SuppressFinalize(this);
		}

		public void RemoveBlock(int index) => RemoveBlock(this, _memoryBlocks, _blockSize, index);
	}
}
