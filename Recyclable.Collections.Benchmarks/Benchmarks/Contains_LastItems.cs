﻿using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Contains_LastItems()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.Contains(data[^(i + 1)]));
			}
		}

		public void List_Contains_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.Contains(data[^(i + 1)]));
			}
		}

		public void PooledList_Contains_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.Contains(data[^(i + 1)]));
			}
		}

		public void RecyclableList_Contains_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.Contains(data[^(i + 1)]));
			}
		}

		public void RecyclableLongList_Contains_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableLongList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.Contains(data[^(i + 1)]));
			}
		}
	}
}
