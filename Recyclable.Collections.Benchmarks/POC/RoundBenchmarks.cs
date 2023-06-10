using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks.POC
{
	public enum RoundBenchmarkType
	{
		MathFloor,
		MathRound,
		WithTypecasting,
	}

	public class RoundBenchmarks : PocBenchmarkBase<RoundBenchmarkType>
	{
		[Params(RoundBenchmarkType.MathFloor)]
		public override RoundBenchmarkType BaselineBenchmarkType { get => base.BaselineBenchmarkType; set => base.BaselineBenchmarkType = value; }

		[Params(RoundBenchmarkType.MathRound, RoundBenchmarkType.WithTypecasting)]
		public override RoundBenchmarkType BenchmarkType { get => base.BenchmarkType; set => base.BenchmarkType = value; }

		private const double OptimalParallelSearchStep = 0.329;

		public void RoundWithMathFloor()
		{
			DoNothing((long)Math.Floor(TestObjectCount * OptimalParallelSearchStep));
		}

		public void RoundWithMathRound()
		{
			DoNothing((long)Math.Round(TestObjectCount * OptimalParallelSearchStep, 0));
		}

		public void RoundWithTypeCasting()
		{
			DoNothing((long)(TestObjectCount * OptimalParallelSearchStep));
		}

		protected override Action GetTestMethod(RoundBenchmarkType benchmarkType) => benchmarkType switch
        {
            RoundBenchmarkType.MathFloor => RoundWithMathFloor,
            RoundBenchmarkType.MathRound => RoundWithMathRound,
            RoundBenchmarkType.WithTypecasting => RoundWithTypeCasting,
            _ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
        };
	}
}
