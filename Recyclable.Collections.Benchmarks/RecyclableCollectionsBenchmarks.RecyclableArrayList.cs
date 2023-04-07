using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		//[Benchmark]
		public void RecyclableArrayList_Create()
		{
			using var list = new RecyclableArrayList<object>();
			DoNothing(list);
		}

		//[Benchmark]
		public void RecyclableArrayList_Create_WithCapacity()
		{
			using var list = new RecyclableArrayList<object>(TestObjectCount);
			DoNothing(list);
		}

		//[Benchmark]
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

		//[Benchmark]
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

		//[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<object>)TestObjectsAsRecyclableList;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableArrayList_AddRangeWhenSourceIsRecyclableArrayList()
		{
			var data = TestObjectsAsRecyclableArrayList;
			using var list = new RecyclableArrayList<object>();
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableArrayList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableArrayList<object>(dataCount);
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableArrayList_SetItem()
		{
			var data = TestObjectsAsRecyclableArrayList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		//[Benchmark]
		public void RecyclableArrayList_GetItem()
		{
			var data = TestObjectsAsRecyclableArrayList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		//[Benchmark]
		public void RecyclableArrayList_Count()
		{
			var data = TestObjectsAsRecyclableArrayList;
			DoNothing(data.Count);
		}

		//[Benchmark]
		public void RecyclableArrayList_IndexOf_FirstItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableArrayList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
			}
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_Remove_FirstItems()
		{
			var data = TestObjects;
			using var list = new RecyclableArrayList<object>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_Remove_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableArrayList<object>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_RemoveAt_FirstItems()
		{
			var data = TestObjects;
			using var list = new RecyclableArrayList<object>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				list.RemoveAt(i);
			}
		}

		[Benchmark(Baseline = false)]
		public void RecyclableArrayList_RemoveAt_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableArrayList<object>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				list.RemoveAt(i);
			}
		}
	}
}
