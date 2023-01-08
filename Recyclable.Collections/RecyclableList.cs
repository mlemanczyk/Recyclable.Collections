using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableList<T> : IDisposable, IList<T>
	{
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;

		private readonly RecyclableBlockList<T[]> _memoryBlocks;
		private bool _disposedValue;

		public RecyclableList(int blockSize = RecyclableDefaults.BlockSize, long? initialCapacity = default)
		{
			BlockSize = blockSize;
			initialCapacity ??= 8 * BlockSize;
			int additionalArray = initialCapacity % BlockSize > 0 ? 1 : 0;
			long targetCapacity = (initialCapacity.Value / BlockSize) + additionalArray;
			_memoryBlocks = new(targetCapacity);
			Capacity = initialCapacity.Value;
			for (var blockIdx = 0; blockIdx < targetCapacity; blockIdx++)
			{
				_memoryBlocks.Add(BlockSize.RentArrayFromPool<T>());
			}

			//MemoryBlocks.LongCount = MemoryBlocks.Capacity;
		}

		public RecyclableList(IEnumerable<T> source, int blockSize = RecyclableDefaults.BlockSize, long? initialCapacity = default)
		{
			BlockSize = blockSize;
			initialCapacity ??= 8 * BlockSize;
			int additionalCapacity = initialCapacity % BlockSize > 0 ? 1 : 0;
			long targetCapacity = (initialCapacity.Value / BlockSize) + additionalCapacity;
			_memoryBlocks = new(targetCapacity);
			Capacity = initialCapacity.Value;
			for (var blockIdx = 0; blockIdx < targetCapacity; blockIdx++)
			{
				_memoryBlocks.Add(BlockSize.RentArrayFromPool<T>());
			}

			foreach (var item in source)
			{
				Add(item);
			}
		}

		public int BlockSize { get; }

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

		public long Capacity { get; set; }
		public int Count => (int)LongCount;
		public long LongCount { get; set; }
		public bool IsReadOnly { get; } = false;
		public long BlockCount => _memoryBlocks.LongCount;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			if (LongCount == Capacity)
			{
				T[] newMemoryBlock = BlockSize.RentArrayFromPool<T>();
				_memoryBlocks.Add(newMemoryBlock);
				Capacity += BlockSize;
			}

			memoryBlocks[oldCount / blockSize][oldCount % blockSize] = item;
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
						RemoveBlock(_memoryBlocks.Count - 1);
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

		public void CopyTo(T[] array, int arrayIndex) => _memoryBlocks.CopyTo(0, BlockSize, (int)LongCount % BlockSize, array, arrayIndex);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator() => _memoryBlocks.Enumerate(BlockSize, LongCount).GetEnumerator();

		public int IndexOf(T item) => (int)_memoryBlocks.LongIndexOf(BlockSize, item, _equalityComparer);

		public void Insert(int index, T item) => throw new NotSupportedException();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long LongIndexOf(T item) => _memoryBlocks.LongIndexOf(BlockSize, item, _equalityComparer);

		public bool Remove(T item) => throw new NotSupportedException();

		public void RemoveAt(int index) => RemoveAt(this, index);
		public void RemoveAt(long index) => RemoveAt(this, index);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void RemoveAt(RecyclableList<T> list, long index)
		{
			if (index == list.LongCount - 1)
			{
				list.LongCount--;
				if (list.Capacity - list.LongCount == list.BlockSize)
				{
					list.RemoveBlock(list.BlockCount - 1);
				}
			}
			else
			{
				throw new NotSupportedException("Only removal of the last element is supported");
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => _memoryBlocks.Enumerate(BlockSize, LongCount).GetEnumerator();

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					Clear();
				}

				_disposedValue = true;
			}
		}

		// override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~RecyclableList()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void RemoveBlock(long index)
		{
			try
			{
				_memoryBlocks[index].ReturnToPool();
				_memoryBlocks.RemoveAt(index);
			}
			finally
			{
				Capacity -= BlockSize;
				LongCount -= BlockSize;
			}
		}
	}
}
