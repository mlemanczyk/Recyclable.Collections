// Ignore Spelling: Poc

using BenchmarkDotNet.Attributes;
using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
	public enum SpanConstructorVsTypecastingBenchmarkType
	{
		Unknown,
		Constructor,
		TypeCasting,
	}

	[MemoryDiagnoser]
	public class SpanConstructorVsTypecastingBenchmarks : BaselineVsActualBenchmarkBase<SpanConstructorVsTypecastingBenchmarkType>
	{
		private int[]? TestData { get; set; }

		[Params(SpanConstructorVsTypecastingBenchmarkType.TypeCasting)]
		public override SpanConstructorVsTypecastingBenchmarkType BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

		[Params(SpanConstructorVsTypecastingBenchmarkType.Constructor)]
		public override SpanConstructorVsTypecastingBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		[Params(8)]
		public override int TestObjectCount { get => base.TestObjectCount; set => base.TestObjectCount = value; }

		public static void Run()
		{
			var benchmark = new SpanConstructorVsTypecastingBenchmarks();
			benchmark.Setup();
			benchmark.Constructor();
			benchmark.TypeCasting();
			benchmark.Cleanup();
		}

		public void Constructor()
		{
			ReadOnlySpan<int> span = new(TestData);
			DoNothing.With(span);
		}

		public void TypeCasting()
		{
			ReadOnlySpan<int> span = TestData;
			DoNothing.With(span);
		}

		public override void Setup()
		{
			TestData = new int[TestObjectCount];
			base.Setup();
		}

		protected override Action? GetTestMethod(SpanConstructorVsTypecastingBenchmarkType benchmarkType) => benchmarkType switch
		{
			SpanConstructorVsTypecastingBenchmarkType.Constructor => Constructor,
			SpanConstructorVsTypecastingBenchmarkType.TypeCasting => TypeCasting,
			_ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
		};
	}
}