using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Recyclable.Collections.Benchmarks;
using Recyclable.Collections.Benchmarks.POC;

internal class Program
{
	static IConfig BenchmarkConfig { get; } = ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator | ConfigOptions.JoinSummary);

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
		//var benchmark = new RecyclableCollectionsBenchmarks();
		//benchmark.Setup();

		//foreach (var _ in Enumerable.Range(1, 1000))
		//{
		//benchmark.RecyclableList_Remove_FirstItems();
		//benchmark.RecyclableList_RemoveAt_LastItems();
		//benchmark.List_RemoveAt_LastItems();
		//}

		//benchmark.Cleanup();
	}

	static void RunPocBenchmarks()
	{
		_ = BenchmarkRunner.Run<RecyclableListPocBenchmarks>(BenchmarkConfig);
		_ = BenchmarkRunner.Run<DelegateVsComparerPocBenchmarks>(BenchmarkConfig);
		_ = BenchmarkRunner.Run<ModuloPocBenchmarks>(BenchmarkConfig);
		_ = BenchmarkRunner.Run<RefVsInstanceMemberPocBenchmarks>(BenchmarkConfig);
		_ = BenchmarkRunner.Run<ArraySizeLimitPocBenchmarks>(BenchmarkConfig);
	}

	private static void Main(string[] args)
	{
		RunRecyclableCollectionsBenchmarks();
		//RunPocBenchmarks();
		//RunSelectedBenchmarks();
		//RunAssemblyBenchmarks();
	}
}
