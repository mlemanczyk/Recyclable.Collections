using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks.POC
{
	public class RoundBenchmarks : PocBenchmarkBase
	{
		private const double OptimalParallelSearchStep = 0.329;

		[Benchmark(Baseline = true)]
		public void RoundWithMathFloor()
		{
			DoNothing((long)Math.Floor(TestObjectCount * OptimalParallelSearchStep));
		}

		[Benchmark]
		public void RoundWithMathRound()
		{
			DoNothing((long)Math.Round(TestObjectCount * OptimalParallelSearchStep, 0));
		}

		[Benchmark(Baseline = false)]
		public void RoundWithTypeCasting()
		{
			DoNothing((long)(TestObjectCount * OptimalParallelSearchStep));
		}
	}
}
