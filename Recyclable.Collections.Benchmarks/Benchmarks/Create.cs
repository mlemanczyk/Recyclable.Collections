using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Create()
		{
			var list = new long[0];
			DoNothing(list);
		}

		public void List_Create()
		{
			var list = new List<long>();
			DoNothing(list);
		}

		public void PooledList_Create()
		{
			using var list = new PooledList<long>(ClearMode.Auto);
			DoNothing(list);
		}

		public void RecyclableList_Create()
		{
			using var list = new RecyclableList<long>();
			DoNothing(list);
		}

		public void RecyclableLongList_Create()
		{
			using var list = new RecyclableLongList<long>();
			DoNothing(list);
		}
	}
}
