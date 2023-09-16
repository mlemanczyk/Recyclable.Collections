using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks.POC
{
	static class RoundUpToPowerOf2Alternatives
	{
		private static readonly int[] PowersOfTwo =
		{
			1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024,
			2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576,
			2097152, 4194304, 8388608, 16777216, 33554432, 67108864, 134217728, 268435456, 536870912, 1073741824
		};

		public static int BinarySearch(int value)
		{
			if (value <= 0)
			{
				throw new ArgumentException("Input must be a positive integer.");
			}

			// Find the smallest power of 2 greater than or equal to value using a lookup table
			int index = Array.BinarySearch(PowersOfTwo, value);
			if (index < 0)
			{
				// If value is not a power of 2, use the next power of 2 from the table
				index = ~index;
			}

			return PowersOfTwo[index];
		}

		public static int RightBitShifting(int value)
		{
			// Decrease value by 1 to handle the case where the input is already a power of 2
			value--;

			value |= value >> 1;
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;

			return value + 1;
		}

		public static int LeftBitShifting(int value)
		{
			if (value == 0)
			{
				return 1;
			}

			// Ensure value is a power of 2 or the next power of 2
			int result = 1;
			while (result < value)
			{
				result <<= 1;
			}

			return result;
		}
	}

	public enum RoundUpToPowerOf2BenchmarkType
	{
		Undefined,
		BitOperations,
		RightBitShifting,
		LeftBitShifting,
		BinarySearch,
		IsPowOf2,
		IsPowOf2_GreaterThanFirst,
		IsPowOf2_WithIf,
		IsPowOf2_GreaterEqual,
	}

	public class RoundUpToPowerOf2vsOtherBenchmarks : PocBenchmarkBase<RoundUpToPowerOf2BenchmarkType>
	{
		// [Params(-850_000, 850_000)]
		[Params(-int.MaxValue, int.MaxValue)]
		public override int TestObjectCount { get => base.TestObjectCount; set => base.TestObjectCount = value; }

		[Params(RoundUpToPowerOf2BenchmarkType.IsPowOf2)]
		public override RoundUpToPowerOf2BenchmarkType BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

		[Params(RoundUpToPowerOf2BenchmarkType.IsPowOf2_GreaterThanFirst, RoundUpToPowerOf2BenchmarkType.IsPowOf2_WithIf, RoundUpToPowerOf2BenchmarkType.IsPowOf2_GreaterEqual)]
		// [Params(RoundUpToPowerOf2BenchmarkType.RightBitShifting, RoundUpToPowerOf2BenchmarkType.LeftBitShifting, RoundUpToPowerOf2BenchmarkType.BinarySearch)]
		public override RoundUpToPowerOf2BenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		private void BitOperations()
		{
			DoNothing(System.Numerics.BitOperations.RoundUpToPowerOf2((uint)base.TestObjectCount));
		}

		private void RightBitShifting()
		{
			DoNothing(RoundUpToPowerOf2Alternatives.RightBitShifting(TestObjectCount));
		}

		private void LeftBitShifting()
		{
			DoNothing(RoundUpToPowerOf2Alternatives.LeftBitShifting(TestObjectCount));
		}

		private void BinarySearch()
		{
			DoNothing(RoundUpToPowerOf2Alternatives.BinarySearch(TestObjectCount));
		}

		private void IsPowOf2()
		{
			DoNothing((TestObjectCount & (TestObjectCount - 1)) == 0 && TestObjectCount > 0);
		}

		private void IsPowOf2_GreaterThanFirst()
		{
			DoNothing(TestObjectCount > 0 && (TestObjectCount & (TestObjectCount - 1)) == 0);
		}

		private void IsPowOf2_WithIf()
		{
			DoNothing(TestObjectCount > 0 ? (TestObjectCount & (TestObjectCount - 1)) == 0 : false);
		}

		private void IsPowOf2_GreaterEqual()
		{
			DoNothing(TestObjectCount >= 0 && TestObjectCount != 0 && (TestObjectCount & (TestObjectCount - 1)) == 0);
		}

		protected override Action? GetTestMethod(RoundUpToPowerOf2BenchmarkType benchmarkType) => benchmarkType switch
		{
			RoundUpToPowerOf2BenchmarkType.BitOperations => BitOperations,
			RoundUpToPowerOf2BenchmarkType.RightBitShifting => RightBitShifting,
			RoundUpToPowerOf2BenchmarkType.LeftBitShifting => LeftBitShifting,
			RoundUpToPowerOf2BenchmarkType.BinarySearch => BinarySearch,
			RoundUpToPowerOf2BenchmarkType.IsPowOf2 => IsPowOf2,
			RoundUpToPowerOf2BenchmarkType.IsPowOf2_GreaterThanFirst => IsPowOf2_GreaterThanFirst,
			RoundUpToPowerOf2BenchmarkType.IsPowOf2_WithIf => IsPowOf2_WithIf,
			RoundUpToPowerOf2BenchmarkType.IsPowOf2_GreaterEqual => IsPowOf2_GreaterEqual,
			_ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
		};

		protected override void PrepareData<T>(T benchmarkType)
		{
		}
	}
}