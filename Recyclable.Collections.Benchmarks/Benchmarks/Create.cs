using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public static void List_Create()
		{
			var list = new List<long>();
			DoNothing(list);
		}

		public static void PooledList_Create()
		{
			using var list = new PooledList<long>(ClearMode.Auto);
			DoNothing(list);
		}

		public static void RecyclableArrayList_Create()
		{
			using var list = new RecyclableArrayList<long>();
			DoNothing(list);
		}

		public static void RecyclableLongList_Create()
		{
			using var list = new RecyclableLongList<long>();
			DoNothing(list);
		}
	}
}
