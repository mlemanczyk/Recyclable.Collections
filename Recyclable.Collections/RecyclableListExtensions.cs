namespace Recyclable.Collections
{
	public static class RecyclableListExtensions
	{
		public static RecyclableList<T> ToRecyclableList<T>(this IList<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);
		public static RecyclableList<T> ToRecyclableList<T>(this IEnumerable<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);
	}
}
