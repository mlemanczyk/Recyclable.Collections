namespace Recyclable.Collections
{
	internal class FrequencyTupleComparer<TKey, TValue> : IComparer<(TKey Key, TValue Value)>
	{
		private static readonly IComparer<TKey> _baseComparer = Comparer<TKey>.Default;

		public int Compare((TKey Key, TValue Value) x, (TKey Key, TValue Value) y)
			=> _baseComparer.Compare(x.Key, y.Key);
	}
}
