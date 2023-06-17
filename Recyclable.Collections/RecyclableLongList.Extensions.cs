using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public static class RecyclableLongListExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this IList<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this List<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this IEnumerable<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize: minBlockSize);
	}
}
