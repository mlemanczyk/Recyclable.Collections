// Ignore Spelling: Poc

using BenchmarkDotNet.Attributes;
using Collections.Benchmarks.Core;

#pragma warning disable CA1822 // Mark members as static

namespace Recyclable.Collections.Benchmarks.POC
{
	public class GreaterOperatorVsNotEqualOperatorBenchmarks
	{
		private static readonly long _num = RecyclableDefaults.MinItemsCountForParallelization;

		[GlobalSetup]
		public void Setup()
		{
			Console.WriteLine($"*** We're comparing if {_num} is < 0 ***");
		}

		[Benchmark(Baseline = true)]
		public void CompareWithGreater()
		{
			DoNothing.With(_num > 0 ? 1 : 0);
		}

		[Benchmark]
		public void CompareWithGreaterEqual()
		{
			DoNothing.With(_num >= 0 ? 1 : 0);
		}

		[Benchmark]
		public void CompareWithNotEqual()
		{
			DoNothing.With(_num != 0 ? 1 : 0);
		}

		[Benchmark]
		public void CompareWithLess()
		{
			DoNothing.With(_num < 0 ? 1 : 0);
		}

		[Benchmark]
		public void CompareWithEqual()
		{
			DoNothing.With(_num == 0 ? 0 : 1);
		}
	}
}
