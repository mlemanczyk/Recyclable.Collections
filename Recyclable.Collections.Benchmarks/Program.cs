using BenchmarkDotNet.Running;
using Recyclable.Collections.Benchmarks;

BenchmarkRunner.Run<ListVsLinkedListBenchmark>();
//BenchmarkRunner.Run<DelegateVsComparerBenchmark>();

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
		benchmark.RecyclableLongList_IndexOf_BestAndWorstCases();
		benchmark.Cleanup();

		//foreach (var _ in Enumerable.Range(1, 1000))
		//{
			// benchmark.RecyclableLongList_Remove_FirstItems();
			// benchmark.RecyclableLongList_RemoveAt_LastItems();
			// benchmark.List_RemoveAt_LastItems();
		//}

		//benchmark.Cleanup();
	}

	static void RunPocBenchmarks()
	{
		//_ = BenchmarkRunner.Run<BoolOrComparePocBenchmarks>(BenchmarkConfig);
		//_ = BenchmarkRunner.Run<DelegateVsComparerPocBenchmarks>(BenchmarkConfig);
		//_ = BenchmarkRunner.Run<ModuloPocBenchmarks>(BenchmarkConfig);
		// _ = BenchmarkRunner.Run<RecyclableLongListPocBenchmarks>(BenchmarkConfig);
		_ = BenchmarkRunner.Run<TaskRunVsTaskFactoryStartNewBenchmarks>(BenchmarkConfig);
		//_ = BenchmarkRunner.Run<RefVsInstanceMemberPocBenchmarks>(BenchmarkConfig);
		//_ = BenchmarkRunner.Run<WhenParallelSearchBenchmarks>(BenchmarkConfig);
		//_ = BenchmarkRunner.Run<RoundBenchmarks>(BenchmarkConfig);
		// _ = BenchmarkRunner.Run<SpanVsArrayBenchmarks>(BenchmarkConfig);
		// ArraySizeLimitPocBenchmarks>(BenchmarkConfig);
	}

	private static void Main(string[] args)
	{
		RunRecyclableCollectionsBenchmarks();
		// RunPocBenchmarks();
		// RunSelectedBenchmarks();
		// RunAssemblyBenchmarks();
	}
}
