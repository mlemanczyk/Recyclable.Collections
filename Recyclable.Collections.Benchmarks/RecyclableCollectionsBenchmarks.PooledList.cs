using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		[Benchmark(Baseline = false)]
		public void PooledList_Create()
		{
			using var list = new PooledList<object>(ClearMode.Always);
			DoNothing(list);
		}

		[Benchmark(Baseline = false)]
		public void PooledList_Create_WithCapacity()
		{
			using var list = new PooledList<object>(capacity: checked((int)TestObjectCount), ClearMode.Always);
			DoNothing(list);
		}

		[Benchmark(Baseline = false)]
		public void PooledList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new PooledList<object>(ClearMode.Always);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_Add_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new PooledList<object>(capacity: checked((int)dataCount), ClearMode.Always);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new PooledList<object>(capacity: checked((int)TestObjectCount), ClearMode.Always);
			list.AddRange(data);
		}

		[Benchmark(Baseline = false)]
		public void PooledList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new PooledList<object>(capacity: checked((int)TestObjectCount), ClearMode.Always);
			list.AddRange(data);
		}

		[Benchmark(Baseline = false)]
		public void PooledList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<object>)TestObjectsAsPooledList;
			using var list = new PooledList<object>(capacity: checked((int)TestObjectCount), ClearMode.Always);
			list.AddRange(data);
		}

		[Benchmark(Baseline = false)]
		public void PooledList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new PooledList<object>(capacity: checked((int)TestObjectCount), ClearMode.Always);
			list.AddRange(data);
		}

		[Benchmark(Baseline = false)]
		public void PooledList_AddRangeWhenSourceIsPooledList()
		{
			var data = TestObjectsAsPooledList;
			using var list = new PooledList<object>(capacity: checked((int)TestObjectCount), ClearMode.Always);
			list.AddRange(data);
		}

		[Benchmark(Baseline = false)]
		public void PooledList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new PooledList<object>(capacity: checked((int)TestObjectCount), ClearMode.Always);
			list.AddRange(data);
		}

		[Benchmark(Baseline = false)]
		public void PooledList_GetItem()
		{
			var data = _testPooledList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_SetItem()
		{
			var data = _testPooledList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_Count()
		{
			var data = _testPooledList;
			DoNothing(data.Count);
		}

		[Benchmark(Baseline = false)]
		public void PooledList_Contains_FirstItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Contains(data[i]));
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_Contains_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Contains(data[^(i + 1)]));
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_IndexOf_FirstItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_Remove_FirstItems()
		{
			var data = TestObjects;
			using var list = new PooledList<object>(data, ClearMode.Always);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_Remove_LastItems()
		{
			var data = TestObjects;
			using var list = new PooledList<object>(data, ClearMode.Always);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_RemoveAt_FirstItems()
		{
			var data = TestObjects;
			using var list = new PooledList<object>(data, ClearMode.Always);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				list.RemoveAt(i);
			}
		}

		[Benchmark(Baseline = false)]
		public void PooledList_RemoveAt_LastItems()
		{
			var data = TestObjects;
			using var list = new PooledList<object>(data, ClearMode.Always);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				list.RemoveAt(i);
			}
		}
	}
}
