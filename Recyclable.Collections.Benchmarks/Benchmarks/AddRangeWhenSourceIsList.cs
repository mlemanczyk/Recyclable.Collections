using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void List_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			var list = new List<long>();
			list.AddRange(data);
		}

		//[Benchmark(Baseline = true)]
		public void PooledList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new PooledList<long>(capacity: TestObjectCount, ClearMode.Auto);
			list.AddRange(data);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new RecyclableArrayList<long>();
			list.AddRange(data);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new RecyclableList<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}
	}
}
