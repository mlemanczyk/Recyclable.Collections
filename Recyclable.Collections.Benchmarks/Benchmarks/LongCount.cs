using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void Array_LongCount()
		{
			var data = _testArray;
			DoNothing(data.LongLength);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_LongCount()
		{
			var data = _testRecyclableList;
			DoNothing(data.LongCount);
		}

	}
}
