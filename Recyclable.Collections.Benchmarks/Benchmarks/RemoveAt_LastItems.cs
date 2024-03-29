﻿using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_RemoveAt_LastItems()
		{
			var data = TestObjects;
			var list = new List<long>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				list.RemoveAt(i);
			}
		}

		public void PooledList_RemoveAt_LastItems()
		{
			var data = TestObjects;
			using var list = new PooledList<long>(data, ClearMode.Auto);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				list.RemoveAt(i);
			}
		}

		public void RecyclableList_RemoveAt_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<long>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				list.RemoveAt(i);
			}
		}

		public void RecyclableLongList_RemoveAt_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableLongList<long>(data, minBlockSize: BlockSize, initialCapacity: base.TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				list.RemoveAt(i);
			}
		}
	}
}
