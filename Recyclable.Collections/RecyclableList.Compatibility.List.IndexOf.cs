namespace Recyclable.Collections
{
	[Obsolete("It'll be moved to Recyclable.Collections.Compatibility.List NuGet package in the next release.")]
	public static class zRecyclableListCompatibilityListIndexOf
	{
		public static int IndexOf<T>(this RecyclableList<T> list, T item, int index) => list._count != 0
				? Array.IndexOf(list._memoryBlock, item, index, list._count - index)
				: RecyclableDefaults.ItemNotFoundIndex;

		public static int IndexOf<T>(this RecyclableList<T> list, T item, int index, int count) => list._count != 0
				? Array.IndexOf(list._memoryBlock, item, index, count)
				: RecyclableDefaults.ItemNotFoundIndex;

		public static int LastIndexOf<T>(this RecyclableList<T> list, T item) => list._count != 0
				? Array.LastIndexOf(list._memoryBlock, item, list._count - 1)
				: RecyclableDefaults.ItemNotFoundIndex;

		public static int LastIndexOf<T>(this RecyclableList<T> list, T item, int index) => list._count != 0
				? Array.LastIndexOf(list._memoryBlock, item, index)
				: RecyclableDefaults.ItemNotFoundIndex;

		public static int LastIndexOf<T>(this RecyclableList<T> list, T item, int index, int count) => list._count != 0
				? Array.LastIndexOf(list._memoryBlock, item, index, count)
				: RecyclableDefaults.ItemNotFoundIndex;
	}
}
