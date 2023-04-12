﻿using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void List_RemoveAt_LastItems()
		{
			var data = TestObjects;
			var list = new List<object>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				list.RemoveAt(i);
			}
		}

		//[Benchmark(Baseline = true)]
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

		//[Benchmark(Baseline = false)]
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

		//[Benchmark(Baseline = false)]
		public void RecyclableList_RemoveAt_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<object>(data, minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				list.RemoveAt(i);
			}
		}
	}
}
