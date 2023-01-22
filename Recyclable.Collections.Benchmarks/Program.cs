using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Recyclable.Collections.Benchmarks;

//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(new[] { "-f*" });

BenchmarkRunner.Run<RecyclableCollectionsBenchmarks>(
	ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.DisableOptimizationsValidator)
);


//BenchmarkRunner.Run<RefVsInstanceMemberBenchmarks>();

//var benchmark = new RecyclableCollectionsBenchmarks();
//benchmark.Setup();
//foreach (var _ in Enumerable.Range(1, 1000))
//{
//	//benchmark.RecyclableArrayList_AddRange_WithCapacity();
//	//benchmark.RecyclableArrayList_Add_WithCapacity();

//	//benchmark.RecyclableList_Create();
//	benchmark.RecyclableList_Create_WithCapacity();
//	//benchmark.RecyclableList_Add();
//	//benchmark.RecyclableList_Add_WithCapacity();
//}

//benchmark.Cleanup();

//	benchmark.Setup();
//	benchmark.Cleanup();

//benchmark.Setup();
//benchmark.Cleanup();

//}


//BenchmarkRunner.Run<DelegateVsComparerBenchmark>();

//var tests = new ListVsLinkedListBenchmark();
//tests.RecyclableList();
