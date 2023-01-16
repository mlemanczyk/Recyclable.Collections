using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		private RecyclableList<object> _testRecyclableList;

		//[Benchmark]
		public void RecyclableList_Create()
		{
			using var list = new RecyclableList<object>();
			DoNothing(list);
		}

		//[Benchmark]
		public void RecyclableList_Create_0Capacity()
		{
			using var list = new RecyclableList<object>(totalItemsCount: 0);
			DoNothing(list);
		}

		//[Benchmark]
		public void RecyclableList_Create_WithCapacity()
		{
			using var list = new RecyclableList<object>(totalItemsCount: TestObjectCount);
			DoNothing(list);
		}

		//[Benchmark]
		public void RecyclableList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<object>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		//[Benchmark]
		public void RecyclableList_Add_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<object>(dataCount);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		//[Benchmark]
		public void RecyclableList_GetItem()
		{
			var data = _testRecyclableList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		//[Benchmark]
		public void RecyclableList_SetItem()
		{
			var data = _testRecyclableList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		//[Benchmark]
		public void RecyclableList_Count()
		{
			var data = _testRecyclableList;
			DoNothing(data.Count);
		}

		//[Benchmark]
		public void RecyclableList_LongCount()
		{
			var data = _testRecyclableList;
			DoNothing(data.LongCount);
		}
	}
}
