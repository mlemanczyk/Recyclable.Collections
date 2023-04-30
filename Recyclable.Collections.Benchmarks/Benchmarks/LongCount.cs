using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void Array_LongCount()
		{
			var data = TestObjects;
			DoNothing(data.LongLength);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_LongCount()
		{
			var data = TestObjectsAsRecyclableList;
			DoNothing(data.LongCount);
		}

	}
}
