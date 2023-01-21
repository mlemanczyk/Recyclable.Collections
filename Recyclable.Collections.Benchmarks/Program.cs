using BenchmarkDotNet.Running;
using Recyclable.Collections.Benchmarks;

//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(new[] { "-f*" });

//BenchmarkRunner.Run<RecyclableCollectionsBenchmarks>();
//BenchmarkRunner.Run<RefVsInstanceMemberBenchmarks>();

var benchmark = new RecyclableCollectionsBenchmarks();
benchmark.Setup();
//benchmark.RecyclableArrayList_AddRange_WithCapacity();
//benchmark.RecyclableArrayList_Add_WithCapacity();
benchmark.RecyclableList_Add_WithCapacity();


//BenchmarkRunner.Run<DelegateVsComparerBenchmark>();

//var tests = new ListVsLinkedListBenchmark();
//tests.RecyclableList();
