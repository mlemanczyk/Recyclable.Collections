using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Recyclable.Collections.Benchmarks.POC;

#pragma warning disable RCS1036, RCS1213, IDE0051

namespace Recyclable.Collections.Benchmarks
{
	public static class Program
	{
		static IConfig BenchmarkConfig { get; } = ManualConfig.Create(DefaultConfig.Instance)
			.WithOptions(ConfigOptions.DisableOptimizationsValidator | ConfigOptions.JoinSummary);

		static void RunAssemblyBenchmarks()
		{
			_ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(new[] { "-f*" });
		}

		private static void RunRecyclableCollectionsBenchmarks()
		{
			_ = BenchmarkRunner.Run<RecyclableCollectionsBenchmarks>(BenchmarkConfig);
		}

		static void RunSelectedBenchmarks()
		{
			// ****************
			// *** Template ***
			// ****************
			var benchmark = new RecyclableCollectionsBenchmarks();
			benchmark.Setup();

			// foreach (var _ in Enumerable.Range(1, 1000))
			// {
			benchmark.List_ForEach();
			benchmark.RecyclableList_ForEach();
			// }

			benchmark.Cleanup();
		}

		static void RunPocBenchmarks()
		{
			_ = BenchmarkRunner.Run<BoolOrComparePocBenchmarks>(BenchmarkConfig);
			_ = BenchmarkRunner.Run<DelegateVsComparerPocBenchmarks>(BenchmarkConfig);
			_ = BenchmarkRunner.Run<ModuloPocBenchmarks>(BenchmarkConfig);
			_ = BenchmarkRunner.Run<RecyclableLongListPocBenchmarks>(BenchmarkConfig);
			_ = BenchmarkRunner.Run<TaskRunVsTaskFactoryStartNewBenchmarks>(BenchmarkConfig);
			_ = BenchmarkRunner.Run<RefVsInstanceMemberPocBenchmarks>(BenchmarkConfig);
			_ = BenchmarkRunner.Run<WhenParallelSearchBenchmarks>(BenchmarkConfig);
			_ = BenchmarkRunner.Run<RoundBenchmarks>(BenchmarkConfig);
			_ = BenchmarkRunner.Run<SpanVsArrayBenchmarks>(BenchmarkConfig);
			_ = BenchmarkRunner.Run<LessOperatorVsAndOperatorBenchmarks>(BenchmarkConfig);
			// ArraySizeLimitPocBenchmarks>(BenchmarkConfig);
		}

		static void Main()
		{
			RunRecyclableCollectionsBenchmarks();
			// RunPocBenchmarks();
			// RunSelectedBenchmarks();
			// RunAssemblyBenchmarks();
		}
	}
}
