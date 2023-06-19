namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_ForEach()
		{
			foreach (var item in TestObjects)
			{
				DoNothing(item);
			}
		}

		public void List_ForEach()
		{
			foreach (var item in TestObjectsAsList)
			{
				DoNothing(item);
			}
		}

		public void PooledList_ForEach()
		{
			foreach (var item in TestObjectsAsPooledList)
			{
				DoNothing(item);
			}
		}

		public void RecyclableList_ForEach()
		{
			foreach (var item in TestObjectsAsRecyclableList)
			{
				DoNothing(item);
			}
		}

		public void RecyclableLongList_ForEach()
		{
			foreach (var item in TestObjectsAsRecyclableLongList)
			{
				DoNothing(item);
			}
		}
	}
}
