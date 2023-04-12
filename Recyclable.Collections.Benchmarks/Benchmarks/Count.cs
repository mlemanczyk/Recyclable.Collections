using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void Array_Count()
		{
			var data = _testArray;
			DoNothing(data.Length);
		}

		//[Benchmark(Baseline = false)]
		public void List_Count()
		{
			var data = TestObjectsAsList;
			DoNothing(data.Count);
		}

		//[Benchmark(Baseline = true)]
		public void PooledList_Count()
		{
			var data = _testPooledList;
			DoNothing(data.Count);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_Count()
		{
			var data = TestObjectsAsRecyclableArrayList;
			DoNothing(data.Count);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_Count()
		{
			var data = _testRecyclableList;
			DoNothing(data.Count);
		}
	}
}
