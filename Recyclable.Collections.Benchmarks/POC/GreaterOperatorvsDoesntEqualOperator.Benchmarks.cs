using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

#pragma warning disable CA1822

namespace Recyclable.Collections.Benchmarks.POC
{
	public class GreaterOperatorVsDoesntEqualOperatorBenchmarks
	{
		private static readonly long num = RecyclableDefaults.MinItemsCountForParallelization;

		public void Setup()
		{
			Console.WriteLine($"*** We're comparing if {num} is < 0 ***");
		}

		public void Cleanup() { }

#pragma warning disable RCS1163, IDE0060
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void DoNothing<T>(T item)
		{
		}

#pragma warning restore RCS1163

		[Benchmark(Baseline = true)]
		public void CompareWithGreater()
		{
			DoNothing(num > 0 ? 1 : 0);
		}

		[Benchmark]
		public void CompareWithGreaterEqual()
		{
			DoNothing(num >= 0 ? 1 : 0);
		}

		[Benchmark]
		public void CompareWithDoesntEqual()
		{
			DoNothing(num != 0 ? 1 : 0);
		}

		[Benchmark]
		public void CompareWithLess()
		{
			DoNothing(num < 0 ? 1 : 0);
		}

		[Benchmark]
		public void CompareWithEqual()
		{
			DoNothing(num == 0 ? 0 : 1);
		}
	}
}
