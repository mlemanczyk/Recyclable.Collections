namespace Recyclable.Collections
{
	internal class RecyclableSortedList<TKey, TValue> : RecyclableLongList<(TKey Key, TValue Value)>
		where TKey : notnull
	{
		private static readonly IComparer<(TKey, TValue)> _comparer = new FrequencyTupleComparer<TKey, TValue>();
		private uint _isUpdating;
		private readonly SystemRandomNumberGenerator _randomNumberGenerator = new();

		protected void ListUpdated()
		{
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

		public RecyclableSortedList(int blockSize = RecyclableDefaults.BlockSize)
			: base(blockSize)
		{
		}

		public bool IsUpdating => _isUpdating > 0;

		public void Add(TKey key, TValue value) => Add((key, value));

		public void BeginUpdate() => _isUpdating++;

		public new void Clear()
		{
			BeginUpdate();
			try
			{
				base.Clear();
			}
			finally
			{
				EndUpdate();
			}
		}

		public void EndUpdate(bool raiseListUpdated = true)
		{
			if (_isUpdating > 0)
			{
				_isUpdating--;
				if (raiseListUpdated && _isUpdating == 0)
				{
					ListUpdated();
				}
			}
		}

		public void QuickSort()
		{
			BeginUpdate();
			try
			{
				RecyclableList<(TKey Key, TValue Value)>.RecyclableListHelpers.QuickSort(this, _comparer, _randomNumberGenerator);
			}
			finally
			{
				EndUpdate(false);
			}
		}
	}
}