using BenchmarkDotNet.Running;
using Recyclable.Collections.Benchmarks;

BenchmarkRunner.Run<ListVsLinkedListBenchmark>();
//BenchmarkRunner.Run<DelegateVsComparerBenchmark>();

//var tests = new ListVsLinkedListBenchmark();
//tests.RecyclableList();