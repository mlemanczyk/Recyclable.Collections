using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_ForEach()
		{
			foreach (var item in TestObjects)
			{
				DoNothing.With(item);
			}
		}

		public void Array_VersionedForEach()
		{
			foreach (var item in TestObjects)
			{
				DoNothing.With(item);
			}
		}

		public void List_ForEach()
		{
			foreach (var item in TestObjectsAsList)
			{
				DoNothing.With(item);
			}
		}

		public void List_VersionedForEach()
		{
			foreach (var item in TestObjectsAsList)
			{
				DoNothing.With(item);
			}
		}

		public void PooledList_ForEach()
		{
			foreach (var item in TestObjectsAsPooledList)
			{
				DoNothing.With(item);
			}
		}

		public void PooledList_VersionedForEach()
		{
			foreach (var item in TestObjectsAsPooledList)
			{
				DoNothing.With(item);
			}
		}

		public void RecyclableList_ForEach()
		{
			foreach (var item in TestObjectsAsRecyclableList)
			{
				DoNothing.With(item);
			}
		}

		public void RecyclableList_VersionedForEach()
		{
			foreach (var item in (IRecyclableVersionedList<int>)TestObjectsAsRecyclableList)
			{
				DoNothing.With(item);
			}
		}

		public void RecyclableLongList_ForEach()
		{
			foreach (var item in TestObjectsAsRecyclableLongList)
			{
				DoNothing.With(item);
			}
		}

		public void RecyclableLongList_VersionedForEach()
		{
			foreach (var item in (IRecyclableVersionedLongList<int>)TestObjectsAsRecyclableLongList)
			{
				DoNothing.With(item);
			}
		}
	}
}
