using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Count()
		{
			var data = TestObjects;
			DoNothing.With(data.Length);
		}

		public void List_Count()
		{
			var data = TestObjectsAsList;
			DoNothing.With(data.Count);
		}

		public void PooledList_Count()
		{
			var data = TestObjectsAsPooledList;
			DoNothing.With(data.Count);
		}

		public void RecyclableList_Count()
		{
			var data = TestObjectsAsRecyclableList;
			DoNothing.With(data.Count);
		}

		public void RecyclableLongList_Count()
		{
			var data = TestObjectsAsRecyclableLongList;
			DoNothing.With(data.Count);
		}
	}
}
