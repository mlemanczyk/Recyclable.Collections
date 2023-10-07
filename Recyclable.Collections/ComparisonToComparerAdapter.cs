namespace Recyclable.Collections
{
	public readonly struct ComparisonToComparerAdapter<T> : IComparer<T>
	{
		private readonly Comparison<T> _comparison;

		public ComparisonToComparerAdapter(Comparison<T> comparison)
		{
			_comparison = comparison;
		}

		public int Compare(T? x, T? y)
		{
			if (x == null && y == null)
			{
				return CompareResult.XEqualY;
			}
			else if (x != null && y == null)
			{
				return CompareResult.XGreaterThenY;
			}
			else if (y != null && x == null)
			{
				return CompareResult.XLessThanY;
			}
			else
			{
				return _comparison(x!, y!);
			}
		}
	}
}
