using System.Buffers;

namespace Recyclable.Collections.Pools
{
	public sealed class OneSizeArrayPool<T> : ArrayPool<T>
	{
		private const int MemoryReleaseTaskSleepMilliseconds = 500;
		internal bool _memoryExceeded;
		private bool _disposed;

		~OneSizeArrayPool()
		{
			_disposed = true;
		}

		private async Task ReleaseMemoryOnExcess()
		{
			while (!_disposed)
			{
				if (_memoryExceeded)
				{
					Enter();
					if (_currentBucket != null)
					{
						_blockCount--;
						_currentBucket = _currentBucket.NextBucket;
					}

					if (_currentBucket != null)
					{
						_blockCount--;
						_currentBucket = _currentBucket.NextBucket;
					}

					if (_currentBucket != null)
					{
						_blockCount--;
						_currentBucket = _currentBucket.NextBucket;
					}

					if (_currentBucket != null)
					{
						_blockCount--;
						_currentBucket = _currentBucket.NextBucket;
					}

					Exit();
				}

				await Task.Delay(MemoryReleaseTaskSleepMilliseconds);
			}
		}

		private readonly int _blockSize;
		private readonly int _minBlockCount;
		private volatile int _blockCount;
		private volatile MemoryBucket<T>? _currentBucket;
		private volatile int _locked;

		private void Enter()
		{
			if (Interlocked.CompareExchange(ref _locked, 1, 0) == 0)
			{
				return;
			}

			SpinWait waiter = new();
			while (Interlocked.CompareExchange(ref _locked, 1, 0) == 1)
			{
				waiter.SpinOnce();
			}
		}

		private void Exit()
		{
			_locked = 0;
		}

		public OneSizeArrayPool(int blockSize, int minBlockCount)
		{
			_blockSize = blockSize;
			_minBlockCount = minBlockCount;
			_ = Task.Run(ReleaseMemoryOnExcess);
		}

		public override T[] Rent(int minimumLength)
		{
			if (minimumLength != _blockSize)
			{
				ThrowWrongRentedArraySizeException(minimumLength, _blockSize);
			}

			MemoryBucket<T>? bucket;
			Enter();
			if (_currentBucket != null)
			{
				_blockCount--;
				bucket = _currentBucket;
				_currentBucket = _currentBucket.NextBucket;
			}
			else
			{
				bucket = null;
			}

			Exit();
			return bucket != null ? bucket.Memory : new T[_blockSize];
		}

		public T[] Rent()
		{
			MemoryBucket<T>? bucket;
			Enter();
			if (_currentBucket != null)
			{
				_blockCount--;
				bucket = _currentBucket;
				_currentBucket = _currentBucket.NextBucket;
			}
			else
			{
				bucket = null;
			}

			Exit();
			return bucket != null ? bucket.Memory : new T[_blockSize];
		}

		public void ReturnUnchecked(T[] array, bool clearArray)
		{
			MemoryBucket<T> bucket = new(array);
			if (clearArray)
			{
				// TODO: Measure performance
				// new Span<T>(array).Clear();
				Array.Clear(array, 0, array.Length);
			}

			Enter();
			bucket.NextBucket = _currentBucket;
			_currentBucket = bucket;
			_blockCount++;
			Exit();
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
				// TODO: Measure performance
				// new Span<T>(array).Clear();
				Array.Clear(array, 0, array.Length);
			}

			Enter();
			if (_blockCount < _minBlockCount)
			{
				bucket.NextBucket = _currentBucket;
				_currentBucket = bucket;
				_blockCount++;
			}

			Exit();
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
