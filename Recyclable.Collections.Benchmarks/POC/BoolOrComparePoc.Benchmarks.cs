using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks.POC
{
    public enum BoolOrComparePocBenchmarkType
	{
		Bool,
		Compare,
	}

	[MemoryDiagnoser]
	public class BoolOrComparePocBenchmarks : PocBenchmarkBase<BoolOrComparePocBenchmarkType>
	{
		[Params(BoolOrComparePocBenchmarkType.Bool, BoolOrComparePocBenchmarkType.Compare)]
		public override BoolOrComparePocBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		[Params(8)]
		public override int TestObjectCount { get => base.TestObjectCount; set => base.TestObjectCount = value; }

		[Params(true)]
		public bool TestBool = true;

		public static void Run()
		{
			var benchmark = new BoolOrComparePocBenchmarks();
			benchmark.Setup();
			benchmark.Bool();
			benchmark.Compare();
			benchmark.Cleanup();
		}

		public void Bool()
		{
			if (TestBool)
			{
				DoNothing(TestBool);
			}
			else
			{
				DoNothing(TestBool);
			}
		}

		public void Compare()
		{
			if (TestObjectCount >= RecyclableDefaults.MinPooledArrayLength)
			{
				DoNothing(TestBool);
			}
			else
			{
				DoNothing(TestBool);
			}
		}

        protected override Action? GetTestMethod(BoolOrComparePocBenchmarkType benchmarkType) => benchmarkType switch
		{
			BoolOrComparePocBenchmarkType.Bool => Bool,
			BoolOrComparePocBenchmarkType.Compare => Compare,
			_ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
		};
    }
}