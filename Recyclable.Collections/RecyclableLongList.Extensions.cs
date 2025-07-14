using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
        [VersionedCollectionsGenerator.GenerateVersioned]
        public static class RecyclableLongListExtensions
        {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this Array values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this ICollection values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this ICollection<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this IEnumerable values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize: minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this IEnumerable<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize: minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this IReadOnlyList<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this List<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this ReadOnlySpan<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this RecyclableList<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this RecyclableLongList<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this Span<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this T[] values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize);
	}
}
