﻿using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void List_Remove_FirstItems()
		{
			var data = TestObjects;
			var list = new List<object>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		//[Benchmark(Baseline = true)]
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
		public void RecyclableList_Remove_FirstItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<object>(data, minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

	}
}
