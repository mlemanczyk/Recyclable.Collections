using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		private readonly List<object> _testList;

		//[Benchmark(Baseline = true)]
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

		[Benchmark]
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

		[Benchmark(Baseline = true)]
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

		[Benchmark]
		public void List_AddRange()
		{
			var data = TestObjects;
			var list = new List<object>();
			list.AddRange(data);
		}

		[Benchmark]
		public void List_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<object>(dataCount);
			list.AddRange(data);
		}

		//[Benchmark]
		public void List_GetItem()
		{
			var data = _testList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		//[Benchmark]
		public void List_SetItem()
		{
			var data = _testList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
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
