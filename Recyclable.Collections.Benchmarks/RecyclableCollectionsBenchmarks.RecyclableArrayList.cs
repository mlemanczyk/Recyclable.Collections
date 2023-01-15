using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		private readonly RecyclableArrayList<object> _testRecyclableArrayList;

		public RecyclableCollectionsBenchmarks()
		{
			_testRecyclableArrayList = new(TestObjects, TestObjectCount);
			_testRecyclableList = new(TestObjects, TestObjectCount);
			_testArray = TestObjects.ToArray();
			_testList = new(TestObjects);
		}

		~RecyclableCollectionsBenchmarks()
		{
			_testRecyclableArrayList?.Dispose();
			_testRecyclableList?.Dispose();
		}

		//[Benchmark]
		public void RecyclableArrayList_Create()
		{
			using var list = new RecyclableArrayList<object>();
			DoNothing(list);
		}

		//[Benchmark]
		public void RecyclableArrayList_Create_0Capacity()
		{
			using var list = new RecyclableArrayList<object>(0);
			DoNothing(list);
		}

		//[Benchmark]
		public void RecyclableArrayList_Create_WithCapacity()
		{
			using var list = new RecyclableArrayList<object>(TestObjectCount);
			DoNothing(list);
		}

		[Benchmark]
		public void RecyclableArrayList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableArrayList<object>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		[Benchmark]
		public void RecyclableArrayList_Add_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableArrayList<object>(dataCount);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<object>)TestObjectsAsList;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = (IEnumerable<object>)TestObjects;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsRecyclableArrayList()
		{
			var data = TestObjectsAsRecyclableArrayList;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		[Benchmark]
		public void RecyclableArrayList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableArrayList<object>(dataCount);
			list.AddRange(data);
		}

		[Benchmark]
		public void RecyclableArrayList_SetItem()
		{
			var data = TestObjectsAsRecyclableArrayList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}

			data.Count = dataCount;
		}

		[Benchmark]
		public void RecyclableArrayList_GetItem()
		{
			var data = TestObjectsAsRecyclableArrayList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		[Benchmark]
		public void RecyclableArrayList_Count()
		{
			var data = _testRecyclableArrayList;
			DoNothing(data.Count);
		}
	}
}
