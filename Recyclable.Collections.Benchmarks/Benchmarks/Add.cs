﻿using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
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

		//[Benchmark(Baseline = true)]
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

		//[Benchmark(Baseline = false)]
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

		//[Benchmark(Baseline = false)]
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
	}
}
