using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void List_AddRangeWhenSourceIsIList()
		{
			var data = (IList<object>)TestObjectsAsRecyclableList;
			var list = new List<object>();
			list.AddRange(data);
		}

		//[Benchmark(Baseline = true)]
		public void PooledList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<object>)TestObjectsAsPooledList;
			using var list = new PooledList<object>(capacity: checked((int)TestObjectCount), ClearMode.Always);
			list.AddRange(data);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<object>)TestObjectsAsRecyclableList;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<object>)TestObjectsAsRecyclableList;
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}
	}
}
