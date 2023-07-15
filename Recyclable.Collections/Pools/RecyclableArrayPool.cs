using System.Numerics;

namespace Recyclable.Collections.Pools
{
	public readonly struct RecyclableArrayPool<T>
	{
		internal static readonly OneSizeArrayPool<T>[] _pools = new OneSizeArrayPool<T>[32];
		public static T[] RentShared(int blockSize) => _pools[31 - BitOperations.LeadingZeroCount((uint)blockSize)].Rent();
		public static void ReturnShared(in T[] memoryBlock, bool needsClearing)
			=> _pools[31 - BitOperations.LeadingZeroCount((uint)memoryBlock.Length)].ReturnUnchecked(memoryBlock, needsClearing);

		public static ref OneSizeArrayPool<T> Shared(int blockSize)
			=> ref _pools[31 - BitOperations.LeadingZeroCount((uint)blockSize)];

		static RecyclableArrayPool()
		{
			var pools = _pools;
			for (var arraySize = 0; arraySize < 32; arraySize++)
			{
				int blockSize = 1 << arraySize;
				pools[arraySize] = new OneSizeArrayPool<T>(blockSize, 1000);
			}

			_ = Task.Run(CheckAndReportMemoryPressure);
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
