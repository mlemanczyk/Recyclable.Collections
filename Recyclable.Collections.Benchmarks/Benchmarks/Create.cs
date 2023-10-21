using Collections.Pooled;

#pragma warning disable CA1822, CA1825

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Create()
		{
			var list = new int[0];
			DoNothing(list);
		}

		public void List_Create()
		{
			var list = new List<int>();
			DoNothing(list);
		}

		public void PooledList_Create()
		{
			using var list = new PooledList<int>(ClearMode.Auto);
			DoNothing(list);
		}

		public void RecyclableList_Create()
		{
			using var list = new RecyclableList<int>();
			DoNothing(list);
		}

		public void RecyclableLongList_Create()
		{
			using var list = new RecyclableLongList<int>();
			DoNothing(list);
		}
	}
}
