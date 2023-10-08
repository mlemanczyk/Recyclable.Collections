using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks.POC
{
    public enum SpanConstructorVsTypecastingPocBenchmarkType
	{
		Unknown,
		Constructor,
		TypeCasting,
	}

	[MemoryDiagnoser]
	public class SpanConstructorVsTypecastingPocBenchmarks : PocBenchmarkBase<SpanConstructorVsTypecastingPocBenchmarkType>
	{
		private int[]? TestData { get; set; }

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		private void DoNothing(ReadOnlySpan<int> span) { }

		[Params(SpanConstructorVsTypecastingPocBenchmarkType.TypeCasting)]
		public override SpanConstructorVsTypecastingPocBenchmarkType BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

		[Params(SpanConstructorVsTypecastingPocBenchmarkType.Constructor)]
		public override SpanConstructorVsTypecastingPocBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		[Params(8)]
		public override int TestObjectCount { get => base.TestObjectCount; set => base.TestObjectCount = value; }

		public static void Run()
		{
			var benchmark = new SpanConstructorVsTypecastingPocBenchmarks();
			benchmark.Setup();
			benchmark.Constructor();
			benchmark.TypeCasting();
			benchmark.Cleanup();
		}

		public void Constructor()
		{
			ReadOnlySpan<int> span = new(TestData);
			DoNothing(span);
		}

		public void TypeCasting()
		{
			ReadOnlySpan<int> span = TestData;
			DoNothing(span);
		}

		public override void Setup()
		{
			TestData = new int[TestObjectCount];
			base.Setup();
		}

		protected override Action? GetTestMethod(SpanConstructorVsTypecastingPocBenchmarkType benchmarkType) => benchmarkType switch
		{
			SpanConstructorVsTypecastingPocBenchmarkType.Constructor => Constructor,
			SpanConstructorVsTypecastingPocBenchmarkType.TypeCasting => TypeCasting,
			_ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
		};
    }
}