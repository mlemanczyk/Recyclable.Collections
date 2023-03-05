using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		//[Benchmark]
		public void RecyclableList_Create()
		{
			using var list = new RecyclableList<object>();
			DoNothing(list);
		}

		//[Benchmark]
		public void RecyclableList_Create_WithCapacity()
		{
			using var list = new RecyclableList<object>(minBlockSize: 102_400, expectedItemsCount: TestObjectCount);
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
			using var list = new RecyclableList<object>(minBlockSize: 102_400, expectedItemsCount: dataCount);
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

		//[Benchmark]
		public void RecyclableList_Contains_FirstItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Contains(data[i]));
			}
		}

		//[Benchmark]
		public void RecyclableList_Contains_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Contains(data[^(i + 1)]));
			}
		}

		[Benchmark]
		public void RecyclableList_IndexOf_FirstItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
			}
		}

		[Benchmark]
		public void RecyclableList_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}
	}
}
