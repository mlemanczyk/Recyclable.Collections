using BenchmarkDotNet.Running;
using Recyclable.Collections.Benchmarks;

//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(new[] { "-f*" });

BenchmarkRunner.Run<RecyclableCollectionsBenchmarks>();

//var benchmark = new RecyclableCollectionsBenchmarks();
//benchmark.RecyclableArrayList_Add();
//benchmark.RecyclableArrayList_Add_WithCapacity();


//BenchmarkRunner.Run<DelegateVsComparerBenchmark>();

//var tests = new ListVsLinkedListBenchmark();
//tests.RecyclableList();
