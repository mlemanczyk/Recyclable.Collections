// Ignore Spelling: Poc

using BenchmarkDotNet.Attributes;
using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
	public enum BoolOrCompareBenchmarkType
	{
		Bool,
		Compare,
	}

	[MemoryDiagnoser]
	public class BoolOrCompareBenchmarks : BaselineVsActualBenchmarkBase<BoolOrCompareBenchmarkType>
	{
		[Params(BoolOrCompareBenchmarkType.Bool, BoolOrCompareBenchmarkType.Compare)]
		public override BoolOrCompareBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		[Params(8)]
		public override int TestObjectCount { get => base.TestObjectCount; set => base.TestObjectCount = value; }

		[Params(true)]
		public bool TestBool = true;

		public static void Run()
		{
			var benchmark = new BoolOrCompareBenchmarks();
			benchmark.Setup();
			benchmark.Bool();
			benchmark.Compare();
			benchmark.Cleanup();
		}

		public void Bool()
		{
			if (TestBool)
			{
				DoNothing.With(TestBool);
			}
			else
			{
				DoNothing.With(TestBool);
			}
		}

		public void Compare()
		{
			if (TestObjectCount >= RecyclableDefaults.MinPooledArrayLength)
			{
				DoNothing.With(TestBool);
			}
			else
			{
				DoNothing.With(TestBool);
			}
		}

		protected override Action? GetTestMethod(BoolOrCompareBenchmarkType benchmarkType) => benchmarkType switch
		{
			BoolOrCompareBenchmarkType.Bool => Bool,
			BoolOrCompareBenchmarkType.Compare => Compare,
			_ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
		};
	}
}