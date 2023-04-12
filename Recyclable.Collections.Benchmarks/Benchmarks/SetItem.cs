using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void Array_SetItem()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			long index = 0;
			for (var i = 0; i < dataCount; i++)
			{
				data[index] = TestObjects[index++];
			}
		}

		//[Benchmark(Baseline = false)]
		public void List_SetItem()
		{
			var data = TestObjectsAsList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		//[Benchmark(Baseline = true)]
		public void PooledList_SetItem()
		{
			var data = _testPooledList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_SetItem()
		{
			var data = TestObjectsAsRecyclableArrayList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_SetItem()
		{
			var data = _testRecyclableList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}
	}
}
