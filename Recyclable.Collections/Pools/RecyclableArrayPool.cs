using System.Numerics;

namespace Recyclable.Collections.Pools
{
	public static class RecyclableArrayPool<T>
	{
		private enum MemoryPressure
		{
			High,
			Medium,
			Low
		}

        private static MemoryPressure GetMemoryPressure()
        {
            const double HighPressureThreshold = .90;       // Percent of GC memory pressure threshold we consider "high"
            const double MediumPressureThreshold = .70;     // Percent of GC memory pressure threshold we consider "medium"

            GCMemoryInfo memoryInfo = GC.GetGCMemoryInfo();

            if (memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * HighPressureThreshold)
            {
                return MemoryPressure.High;
            }

            if (memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * MediumPressureThreshold)
            {
                return MemoryPressure.Medium;
            }

            return MemoryPressure.Low;
        }

		private static async Task CheckAndReportMemoryPressure()
		{
			var previousValue = false;
			while (true)
			{
				bool memoryExceeded = GetMemoryPressure() == MemoryPressure.High;
				if (memoryExceeded != previousValue)
				{
					previousValue = memoryExceeded;
					for (var i = 0; i < _pools!.Length; i++)
					{
						_pools[i]._memoryExceeded = memoryExceeded;
					}
				}

				await Task.Delay(500);
			}
		}

		static RecyclableArrayPool()
		{
			var pools = _pools;
			for (var arraySize = 0; arraySize < 32; arraySize++)
			{
				int blockSize = 2 << arraySize;
				pools[arraySize] = new OneSizeArrayPool<T>(blockSize, 1000);
			}

			_ = Task.Run(CheckAndReportMemoryPressure);
		}

		public static readonly OneSizeArrayPool<T> Null = new(0, 0);

		private static readonly OneSizeArrayPool<T>[] _pools = new OneSizeArrayPool<T>[32];
		public static T[] Rent(int blockSize) => _pools[31 - BitOperations.LeadingZeroCount((uint)blockSize) - 1].Rent();
		public static void Return(in T[] memoryBlock, bool needsClearing) => _pools[31 - BitOperations.LeadingZeroCount((uint)memoryBlock.Length) - 1].ReturnUnchecked(memoryBlock, needsClearing);

		// private static readonly ConcurrentDictionary<int, ArrayPool<T>> _pools = new();

		public static OneSizeArrayPool<T> Shared(int blockSize) =>
				//lock (_lock)
				//{

				// if (!_pools.TryGetValue(blockSize, out ArrayPool<T>? pool))
				// {
				// 	pool = new OneSizeArrayPool<T>(blockSize, 100);
				// 	pool = _pools.TryAdd(blockSize, pool) ? pool : _pools[blockSize];
				// }
				_pools[31 - BitOperations.LeadingZeroCount((uint)blockSize) - 1];//}//_lock.Wait();//try//{//}//finally//{//	_lock.Release();//}
	}
}
