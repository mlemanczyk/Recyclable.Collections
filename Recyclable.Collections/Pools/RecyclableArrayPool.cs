using System.Buffers;
using System.Collections.Concurrent;

namespace Recyclable.Collections.Pools
{
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
}
