using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
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

		public void RecyclableArrayList_Create()
		{
			using var list = new RecyclableArrayList<long>();
			DoNothing(list);
		}

		public void RecyclableList_Create()
		{
			using var list = new RecyclableList<long>();
			DoNothing(list);
		}
	}
}
