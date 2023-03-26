using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Recyclable.Collections.Benchmarks;

//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(new[] { "-f*" });

//BenchmarkRunner.Run<RefVsInstanceMemberBenchmarks>();



internal class Program
{
	static void RunModuloBenchmarks()
	{
		var benchmark = new ModuloBenchmarks();
		//benchmark.AddWithFor();
		//benchmark.DividerAndModulusOperator();
		//benchmark.MathDivRem();
		//benchmark.SubtracWithWhile();
		//benchmark.XDivYMulX();
		//benchmark.XDivYMulXWithLocalVar();
		//benchmark.AddWithForOptimized();
		//benchmark.MixedApproachWithForLoopOptimized();
		//benchmark.MixedApproachWithSubtractWithWhile();
		benchmark.MixedApproachWithSubtractWithWhileAsVector();
	}

	static void RunBenchmark()
	{
		var benchmark = new RecyclableCollectionsBenchmarks();
		benchmark.Setup();
		foreach (var _ in Enumerable.Range(1, 1000))
		{
			//benchmark.RecyclableList_Contains();
			//benchmark.RecyclableArrayList_AddRange_WithCapacity();
			//benchmark.RecyclableArrayList_Add_WithCapacity();

			//benchmark.RecyclableList_Create();
			//benchmark.RecyclableList_Create_WithCapacity();
			//benchmark.RecyclableList_Add();
			//benchmark.RecyclableList_Add_WithCapacity();
			//benchmark.RecyclableList_Contains_LastItems();
			//benchmark.RecyclableList_IndexOf_FirstItems();
			benchmark.RecyclableList_AddRangeWhenSourceIsList();
		}

		benchmark.Cleanup();
	}

	private static void Main(string[] args)
	{
		BenchmarkRunner.Run<RecyclableCollectionsBenchmarks>(
			ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator | ConfigOptions.JoinSummary)
		);

		//RunBenchmark();


		//BenchmarkRunner.Run<ModuloBenchmarks>(
		//	ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator | ConfigOptions.JoinSummary)
		//);

		//RunModuloBenchmarks();
	}
}
//	benchmark.Setup();
//	benchmark.Cleanup();

//benchmark.Setup();
//benchmark.Cleanup();

//}


//BenchmarkRunner.Run<DelegateVsComparerBenchmark>();

//var tests = new ListVsLinkedListBenchmark();
//tests.RecyclableList();
