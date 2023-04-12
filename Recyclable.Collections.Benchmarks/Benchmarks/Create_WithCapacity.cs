using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void Array_Create_WithCapacity()
		{
			var data = new object[TestObjectCount];
			DoNothing(data);
		}

		//[Benchmark(Baseline = false)]
		public void List_Create_WithCapacity()
		{
			var list = new List<object>(checked((int)TestObjectCount));
			DoNothing(list);
		}

		//[Benchmark(Baseline = true)]
		public void PooledList_Create_WithCapacity()
		{
			using var list = new PooledList<object>(capacity: checked((int)TestObjectCount), ClearMode.Always);
			DoNothing(list);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_Create_WithCapacity()
		{
			using var list = new RecyclableArrayList<object>(checked((int)TestObjectCount));
			DoNothing(list);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_Create_WithCapacity()
		{
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			DoNothing(list);
		}
	}
}
