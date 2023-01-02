namespace Recyclable.Collections
{
	public class RecyclableSortedList<TKey, TValue> : RecyclableList<(TKey Key, TValue Value)>
		where TKey : notnull
	{
		private static readonly IComparer<(TKey, TValue)> _comparer = new FrequencyTupleComparer<TKey, TValue>();
		private readonly SystemRandomNumberGenerator _randomNumberGenerator = new SystemRandomNumberGenerator();

		protected override void ListUpdated()
		{
			base.ListUpdated();
			QuickSort();
		}

		public RecyclableSortedList() : base()
		{
		}

		public RecyclableSortedList(Dictionary<TKey, TValue> source, int blockSize = RecyclableDefaults.BlockSize)
			: this(source.Select(x => (x.Key, x.Value)), blockSize)
		{
		}

		public RecyclableSortedList(IEnumerable<(TKey Key, TValue Value)> source, int blockSize = RecyclableDefaults.BlockSize)
			: base(source, blockSize)
		{
			QuickSort();
		}

		public void QuickSort()
		{
			_isUpdating++;
			try
			{
				this.QuickSort(_comparer, _randomNumberGenerator);
			}
			finally
			{
				_isUpdating--;
			}		
		}
	}
}
