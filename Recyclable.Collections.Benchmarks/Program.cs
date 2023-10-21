using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

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

			foreach (var _ in Enumerable.Range(1, 1000))
			{
				//benchmark.RecyclableList_Create_WithCapacity();
			}

			benchmark.Cleanup();
		}

		static void RunPocBenchmarks()
		{
			//_ = BenchmarkRunner.Run<BoolOrCompareBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<DelegateVsComparerBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<ModuloBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<TaskRunVsTaskFactoryStartNewBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<RefVsInstanceMemberBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<RoundBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<SpanVsArrayBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<LessOperatorVsAndOperatorBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<EqualVsContainsInConsecutiveOrderBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<RoundUpToPowerOf2vsOtherBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<GreaterOperatorVsNotEqualOperatorBenchmarks>(BenchmarkConfig);
			//_ = BenchmarkRunner.Run<SpanConstructorVsTypecastingBenchmarks>(BenchmarkConfig);

			//ArraySizeLimitBenchmarks.Run();
		}

		static void Main()
		{
			// RunRecyclableCollectionsBenchmarks();
			RunPocBenchmarks();
			// RunSelectedBenchmarks();
			// RunAssemblyBenchmarks();
		}
	}
}
