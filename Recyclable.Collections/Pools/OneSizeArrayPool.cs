using System.Buffers;

namespace Recyclable.Collections.Pools
{
	public sealed class OneSizeArrayPool<T>
	{
		private readonly int _blockSize;
		private readonly int _minBlockCount;
		private int _blockCount;
		private MemoryBucket<T>? _currentBucket;
		private readonly SpinLock _updateLock = new(false);

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
			bool lockTaken = false;
			_updateLock.Enter(ref lockTaken);
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

			if (lockTaken)
			{
				_updateLock.Exit(false);
			}

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

			bool lockTaken = false;
			_updateLock.Enter(ref lockTaken);
			if (_blockCount < _minBlockCount)
			{
				bucket.NextBucket = _currentBucket;
				_currentBucket = bucket;
				_blockCount++;
			}

			if (lockTaken)
			{
				_updateLock.Exit(false);
			}
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
