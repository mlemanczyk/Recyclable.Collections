using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(Array.IndexOf(list, data[^(i + 1)]));
			}
		}

		public void List_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void PooledList_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void RecyclableList_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void RecyclableLongList_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableLongList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void RecyclableLongList_LongIndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableLongList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing.With(list.LongIndexOf(data[^(i + 1)]));
			}
		}
	}
}
