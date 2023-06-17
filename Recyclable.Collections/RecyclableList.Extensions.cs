using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public static class RecyclableListExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this IEnumerable<T> values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this T[] values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this List<T> values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this RecyclableList<T> values) => new(values);
	}
}
