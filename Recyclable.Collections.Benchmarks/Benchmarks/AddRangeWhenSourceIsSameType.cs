using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = true)]
		public void PooledList_AddRangeWhenSourceIsPooledList()
		{
			var data = TestObjectsAsPooledList;
			using var list = new PooledList<long>(capacity: TestObjectCount, ClearMode.Auto);
			list.AddRange(data);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_AddRangeWhenSourceIsRecyclableArrayList()
		{
			var data = TestObjectsAsRecyclableArrayList;
			using var list = new RecyclableArrayList<long>();
			list.AddRange(data);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_AddRangeWhenSourceIsRecyclableArrayList()
		{
			var data = TestObjectsAsRecyclableArrayList;
			using var list = new RecyclableList<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}
	}
}
