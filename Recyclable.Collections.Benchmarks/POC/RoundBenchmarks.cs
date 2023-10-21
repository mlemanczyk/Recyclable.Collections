// Ignore Spelling: Poc

using BenchmarkDotNet.Attributes;
using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
	public enum RoundBenchmarkType
	{
		MathFloor,
		MathRound,
		WithTypecasting,
	}

	public class RoundBenchmarks : BaselineVsActualBenchmarkBase<RoundBenchmarkType>
	{
		[Params(RoundBenchmarkType.MathFloor)]
		public override RoundBenchmarkType BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

		[Params(RoundBenchmarkType.MathRound, RoundBenchmarkType.WithTypecasting)]
		public override RoundBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		private const double OptimalParallelSearchStep = 0.329;

		public void RoundWithMathFloor()
		{
			DoNothing.With((long)Math.Floor(TestObjectCount * OptimalParallelSearchStep));
		}

		public void RoundWithMathRound()
		{
			DoNothing.With((long)Math.Round(TestObjectCount * OptimalParallelSearchStep, 0));
		}

		public void RoundWithTypeCasting()
		{
			DoNothing.With((long)(TestObjectCount * OptimalParallelSearchStep));
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
