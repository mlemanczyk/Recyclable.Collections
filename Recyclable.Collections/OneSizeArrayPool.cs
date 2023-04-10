using System.Buffers;
using System.Collections.Concurrent;

namespace Recyclable.Collections
{
	public class MemoryBucket<T>
	{
		public MemoryBucket(T[] memory, MemoryBucket<T> nextBucket)
		{
			_memory = memory;
			_nextBucket = nextBucket;
		}

		public MemoryBucket(T[] memory)
		{
			_memory = memory;
		}

		private T[] _memory;
		public T[] Memory { get => _memory; set => _memory = value; }

		private MemoryBucket<T>? _nextBucket;
		public MemoryBucket<T>? NextBucket { get => _nextBucket; set => _nextBucket = value; }
	}

	public static class RecyclableArrayPool<T>
	{
		public static readonly OneSizeArrayPool<T> Null = new(0, 0);

		private static readonly ConcurrentDictionary<int, ArrayPool<T>> _pools = new();

		public static ArrayPool<T> Shared(int blockSize)
		{
			//lock (_lock)
			{
				if (!_pools.TryGetValue(blockSize, out ArrayPool<T>? pool))
				{
					pool = new OneSizeArrayPool<T>(blockSize, 100);
					pool = _pools.TryAdd(blockSize, pool) ? pool : _pools[blockSize];
				}

				return pool;
			}

			//_lock.Wait();
			//try
			//{
			//}
			//finally
			//{
			//	_lock.Release();
			//}
		}
	}

	public class OneSizeArrayPool<T> : ArrayPool<T>
	{
		private readonly int _blockSize;
		private readonly int _minBlockCount;
		private int _blockCount;
		private MemoryBucket<T>? _currentBucket;
		//SemaphoreSlim _lock = new(1);
		//ManualResetEventSlim _lock = new(true, 1);
		private readonly object _updateLock = new();
		//private Task? _bufferFillUpTask;

		//private Task CreateAdditionalBucket(int minimumLength, int blockCount)
		//{
		//	return Task.Factory.StartNew(() =>
		//	{
		//		MemoryBucket<T> firstBucket = new(new T[minimumLength]);
		//		MemoryBucket<T> currentBucket = firstBucket;
		//		for (var i = 0; i < blockCount; i++)
		//		{
		//			currentBucket = new(new T[minimumLength], currentBucket);
		//		}

		//		lock (_updateLock)
		//		//_lock.Wait();
		//		//try
		//		{
		//			currentBucket.NextBucket = _currentBucket;
		//			_currentBucket = firstBucket;
		//			_blockCount += blockCount;
		//			//_bufferFillUpTask = null;
		//		}
		//		//finally
		//		//{
		//		//	_ = _lock.Release();
		//		//	//_lock.Set();
		//		//}
		//	});
		//}

		public OneSizeArrayPool(int blockSize, int minBlockCount)
		{
			_blockSize = blockSize;
			_minBlockCount = minBlockCount;
		}

		public override T[] Rent(int minimumLength)
		{
			if (minimumLength != _blockSize)
			{
				ThrowWrongRentedArraySizeException(minimumLength, _blockSize);
			}

			MemoryBucket<T>? bucket;
			lock (_updateLock)
			//_lock.Wait();
			//try
			{
				if (_currentBucket != null)
				{
					_blockCount--;
					bucket = _currentBucket;
					_currentBucket = _currentBucket.NextBucket;
				}
				else
				{
					bucket = null;
					//if (_bufferFillUpTask == null)
					//{
					//	_bufferFillUpTask = CreateAdditionalBucket(minimumLength, _minBlockCount);
					//}
				}
			}
			//finally
			//{
			//	_ = _lock.Release();
			//	//_lock.Set();
			//}

			return bucket != null ? bucket.Memory : new T[_blockSize];
		}

		public override void Return(T[] array, bool clearArray = false)
		{
			if (array.Length != _blockSize)
			{
				ThrowWrongReturnedArraySizeException(array.Length, _blockSize);
			}

			MemoryBucket<T> bucket = new(array);
			if (clearArray)
			{
				Array.Clear(array, 0, array.Length);
			}

			lock (_updateLock)
			//lock (_lock)
			//_lock.Wait();
			//try
			{
				if (_blockCount < _minBlockCount)
				{
					bucket.NextBucket = _currentBucket;
					_currentBucket = bucket;
					_blockCount++;
				}
			}
			//finally
			//{
			//	_ = _lock.Release();
			//	//_lock.Set();
			//}
		}

		private static void ThrowWrongReturnedArraySizeException(int returnedLength, int blockSize)
		{
			throw new InvalidOperationException($"The array size {returnedLength} doesn't match the size {blockSize} of the pool buffer and cannot be returned to this pool.");
		}

		private static void ThrowWrongRentedArraySizeException(int requestedSize, int blockSize)
		{
			throw new InvalidOperationException($"The requested size {requestedSize} doesn't match the size {blockSize} of the pool buffer and cannot be taken from this pool.");
		}
	}
}
