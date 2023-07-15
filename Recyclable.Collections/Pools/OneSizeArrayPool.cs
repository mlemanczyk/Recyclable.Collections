namespace Recyclable.Collections.Pools
{
	public struct OneSizeArrayPool<T>
	{
		private volatile MemoryBucket<T>? _currentBucket;
		private volatile int _blockCount;
		private volatile int _locked;
		private readonly int _blockSize;
		private readonly int _minBlockCount;
		internal volatile bool _memoryExceeded;

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
			_blockCount = 0;
			_currentBucket = null;
			_locked = 0;
			_memoryExceeded = false;
			_blockSize = blockSize;
			_minBlockCount = minBlockCount;
			_ = Task.Run(ReleaseMemoryOnExcess);
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

		public void Return(T[] array, bool clearArray = false)
		{
			if (array.Length != _blockSize)
			{
				ThrowHelper.ThrowWrongReturnedArraySizeException(array.Length, _blockSize);
			}

			if (clearArray)
			{
				// TODO: Measure performance
				// new Span<T>(array).Clear();
				Array.Clear(array, 0, array.Length);
			}

			MemoryBucket<T> bucket = new(array);
			Enter();
			if (_blockCount < _minBlockCount)
			{
				bucket.NextBucket = _currentBucket;
				_currentBucket = bucket;
				_blockCount++;
			}

			Exit();
		}

		private const int MemoryReleaseTaskSleepMilliseconds = 500;

		private async Task ReleaseMemoryOnExcess()
		{
			while (true)
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
	}
}
