﻿using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		private readonly List<object> _testList;

		//[Benchmark(Baseline = false)]
		public void List_Create()
		{
			var list = new List<object>();
			DoNothing(list);
		}

		//[Benchmark]
		public void List_Create_0Capacity()
		{
			var list = new List<object>(0);
			DoNothing(list);
		}

		//[Benchmark]
		public void List_Create_WithCapacity()
		{
			var list = new List<object>(TestObjectCount);
			DoNothing(list);
		}

		//[Benchmark]
		public void List_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<object>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		//[Benchmark(Baseline = false)]
		public void List_Add_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<object>(dataCount);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		//[Benchmark]
		public void List_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			var list = new List<object>();
			list.AddRange(data);
		}

		//[Benchmark]
		public void List_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			var list = new List<object>();
			list.AddRange(data);
		}

		//[Benchmark]
		public void List_AddRangeWhenSourceIsIList()
		{
			var data = (IList<object>)TestObjectsAsList;
			var list = new List<object>();
			list.AddRange(data);
		}

		[Benchmark]
		public void List_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			var list = new List<object>();
			list.AddRange(data);
		}

		//[Benchmark(Baseline = false)]
		public void List_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<object>(dataCount);
			list.AddRange(data);
		}

		//[Benchmark]
		public void List_SetItem()
		{
			var data = TestObjectsAsList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		//[Benchmark]
		public void List_GetItem()
		{
			var data = TestObjectsAsList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		//[Benchmark]
		public void List_Count()
		{
			var data = _testList;
			DoNothing(data.Count);
		}
	}
}
