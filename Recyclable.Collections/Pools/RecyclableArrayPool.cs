using System.Numerics;

namespace Recyclable.Collections.Pools
{
	public readonly struct RecyclableArrayPool<T>
	{
		private const int MaxArraySize = 32;
		internal static readonly OneSizeArrayPool<T>[] _pools = new OneSizeArrayPool<T>[MaxArraySize];
		internal static readonly Dictionary<int, OneSizeArrayPool<T>[]> _poolsWithBoundaries = new();
		
		public static Array RentShared(int blockSize, int lowerBound)
		{
			OneSizeArrayPool<T>[]? pools;
			lock(_poolsWithBoundaries)
			{
				if (!_poolsWithBoundaries.TryGetValue(lowerBound, out pools))
				{
					pools = _poolsWithBoundaries[lowerBound] = new OneSizeArrayPool<T>[MaxArraySize];
					InitializePools(pools);
				}
			}

			return pools[31 - BitOperations.LeadingZeroCount((uint)blockSize)].Rent();
		}

		public static Array RentShared(int blockSize) => _pools[31 - BitOperations.LeadingZeroCount((uint)blockSize)].Rent();
		public static T[] RentSharedAsTyped(int blockSize) => (T[])_pools[31 - BitOperations.LeadingZeroCount((uint)blockSize)].Rent();

		public static void ReturnShared(in T[] memoryBlock, bool needsClearing)
		{
			if (memoryBlock.GetLowerBound(0) == 0)
			{
				_pools[31 - BitOperations.LeadingZeroCount((uint)memoryBlock.Length)].ReturnUnchecked(memoryBlock, needsClearing);
			}
			else
			{
				OneSizeArrayPool<T>[] pools;
				lock(_poolsWithBoundaries)
				{
					pools = _poolsWithBoundaries[memoryBlock.GetLowerBound(0)];
				}
				
				pools[31 - BitOperations.LeadingZeroCount((uint)memoryBlock.Length)].ReturnUnchecked(memoryBlock, needsClearing);
			}
		}

		public static void ReturnShared(in Array memoryBlock, bool needsClearing)
		{
			if (memoryBlock.GetLowerBound(0) == 0)
			{
				_pools[31 - BitOperations.LeadingZeroCount((uint)memoryBlock.Length)].ReturnUnchecked(memoryBlock, needsClearing);
			}
			else
			{
				OneSizeArrayPool<T>[] pools;
				lock(_poolsWithBoundaries)
				{
					pools = _poolsWithBoundaries[memoryBlock.GetLowerBound(0)];
				}
				
				pools[31 - BitOperations.LeadingZeroCount((uint)memoryBlock.Length)].ReturnUnchecked(memoryBlock, needsClearing);
			}
		}

		public static ref OneSizeArrayPool<T> Shared(int blockSize)
			=> ref _pools[31 - BitOperations.LeadingZeroCount((uint)blockSize)];

		public static ref OneSizeArrayPool<T> SharedWithBoundaries(int blockSize, int lowerBound)
		{
			OneSizeArrayPool<T>[]? pools;
			lock(_poolsWithBoundaries)
			{
				if (!_poolsWithBoundaries.TryGetValue(lowerBound, out pools))
				{
					pools = _poolsWithBoundaries[lowerBound] = new OneSizeArrayPool<T>[MaxArraySize];
					InitializePools(pools);
				}
			}
			
			return ref pools[31 - BitOperations.LeadingZeroCount((uint)blockSize)];
		}

		static RecyclableArrayPool()
		{
			InitializePools(_pools);
			_ = Task.Run(CheckAndReportMemoryPressure);
		}

		private static void InitializePools(OneSizeArrayPool<T>[] pools)
		{
			for (var arraySize = 0; arraySize < MaxArraySize; arraySize++)
			{
				int blockSize = 1 << arraySize;
				pools[arraySize] = new OneSizeArrayPool<T>(blockSize, 1000);
			}
		}

		public static readonly OneSizeArrayPool<T> Null = new(0, 0);

		private static async Task CheckAndReportMemoryPressure()
		{
			var previousValue = false;
			while (true)
			{
				bool memoryExceeded = MemoryUtils.GetMemoryPressure() == MemoryPressure.High;
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
	}
}
