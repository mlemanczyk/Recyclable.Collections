﻿using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<long>(dataCount);
			list.AddRange(data);
		}

		public void PooledList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new PooledList<long>(capacity: dataCount, ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableArrayList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableArrayList<long>(dataCount);
			list.AddRange(data);
		}

		public void RecyclableList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<long>(minBlockSize: BlockSize, expectedItemsCount: dataCount);
			list.AddRange(data);
		}
	}
}
