using BenchmarkDotNet.Attributes;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections.Benchmarks.POC
{
	public class PocBenchmarkBase
	{
		// 0 2 4 8 16 32 64 128 256 512 1024 2048 4096 8192 16384 32768 65536 131072 262144 524288
		// 1048576 2097152 4194304 8388608 16777216 33554432 67108864 134217728 268435456 536870912
		// 1073741824 2147483648

		//[Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 1_000_000)]
		//[Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 1_000_000)]
		//[Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 100_000_000)]
		//[Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 1_000_000, RecyclableDefaults.MaxPooledBlockSize)]
		//[Params(1_000_000, 10_000_000, 100_000_000, 1_000_000_000)]
		//[Params(1, 8, 16, 128, 192, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 10_000_000)]
		//[Params(RecyclableDefaults.MaxPooledBlockSize)]
		public int TestObjectCount = 1_000_000;

		//[Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200)]
		//[Params(10, 20, 50, 100)]
		//[Params(1, 10)]
		public int Divider = 10;
		//public int BlockSize = 1_048_576;//int.MaxValue - 1;
		public int BlockSize => TestObjectCount switch
		{
			0 => 1,
			> 0 and <= 10 => (int)TestObjectCount,
			_ => TestObjectCount / Divider > 0 ? checked((int)(TestObjectCount / Divider)) : checked((int)TestObjectCount)
		};

		public byte BlockSizePow2BitShift => checked((byte)(31 - BitOperations.LeadingZeroCount((uint)BlockSize)));

		[MethodImpl(MethodImplOptions.NoInlining)]
		protected static void DoNothing<T>(in T item)
		{
			_ = item;
		}
	}
}
