using System.Buffers;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public static class RecyclableListExtensions
	{
		private const int _minPooledArraySize = RecyclableDefaults.MinPooledArrayLength;

		public static void CopyItems<T>(this T[] memory, long startingIndex, long count, ref T[] destArray, long offset = 1)
		{
			for (long destItemIndex = startingIndex, sourceItemIndex = offset; destItemIndex < startingIndex + count; destItemIndex++, sourceItemIndex++)
			{
				destArray[destItemIndex] = memory[sourceItemIndex];
			}
		}

		public static void CopyTo<T>(this RecyclableList<T[]> arrays, long startingIndex, int blockSize, int lastBlockSize, T[] destinationArray, int destinationArrayIndex)
		{
			Span<T> arrayMemory = destinationArray.AsSpan();
			arrayMemory = arrayMemory[destinationArrayIndex..];
			int startingArrayIndex = (int)(startingIndex / blockSize);
			startingIndex %= blockSize;
			if (arrays.LongCount > 0)
			{
				ReadOnlySpan<T> sourceMemory = arrays[0].AsSpan();
				var maxToCopy = (int)Math.Min(blockSize - startingIndex, arrayMemory.Length);
				if (arrays.LongCount == 1)
				{
					maxToCopy = (int)Math.Min(maxToCopy, lastBlockSize);
				}

				sourceMemory = sourceMemory[(int)startingIndex..maxToCopy];
				sourceMemory.CopyTo(arrayMemory);
				arrayMemory = arrayMemory[maxToCopy..];
			}

			for (var arrayIdx = startingArrayIndex + 1; arrayIdx < arrays.Count - 1 && arrayMemory.Length > 0; arrayIdx++)
			{
				ReadOnlySpan<T> sourceMemory = arrays[arrayIdx].AsSpan();
				var maxToCopy = Math.Min(blockSize, arrayMemory.Length);
				sourceMemory = sourceMemory[..maxToCopy];
				sourceMemory.CopyTo(arrayMemory);
				arrayMemory = arrayMemory[maxToCopy..];
			}

			if (arrays.Count > 1 && arrayMemory.Length > 0)
			{
				ReadOnlySpan<T> sourceMemory = arrays[^1].AsSpan();
				var maxToCopy = Math.Min(lastBlockSize, arrayMemory.Length);
				sourceMemory = sourceMemory[..maxToCopy];
				sourceMemory.CopyTo(arrayMemory);
			}
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public static bool Contains<T>(this RecyclableList<T[]> arrays, T item) => arrays.Any(x => x.Contains(item));

		//public static IEnumerable<T> Enumerate<T>(this RecyclableList<T[]> arrays, int chunkSize, long totalCount)
		//{
		//	long currentCount = 0;
		//	for (var arrayIdx = 0; arrayIdx < arrays.Count; arrayIdx++)
		//	{
		//		var array = arrays[arrayIdx];
		//		currentCount += chunkSize;
		//		switch (currentCount < totalCount)
		//		{
		//			case true:
		//				for (var valueIdx = 0; valueIdx < chunkSize; valueIdx++)
		//				{
		//					yield return array[valueIdx];
		//				}

		//				break;

		//			case false:
		//				var partialCount = (int)(totalCount % chunkSize);
		//				int maxCount = partialCount > 0 ? partialCount : chunkSize;
		//				for (var valueIdx = 0; valueIdx < maxCount; valueIdx++)
		//				{
		//					yield return array[valueIdx];
		//				}

		//				break;
		//		}
		//	}
		//}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public static T LimitTo<T>(this T value, T maxValue, IComparer<T> comparer)
		//	=> comparer.Compare(value, maxValue) > 0 ? maxValue : value;

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long LongIndexOf<T>(this RecyclableList<T> arrays, long itemsCount, T itemToFind, IEqualityComparer<T> comparer)
		{
			for (var itemIdx = 0L; itemIdx < itemsCount; itemIdx++)
			{
				var item = arrays[itemIdx];
				if (comparer.Equals(itemToFind, item))
				{
					return itemIdx;
				}

				itemIdx++;
			}

			return -1;
		}

		public static long LongIndexOf<T>(this RecyclableArrayList<T> arrays, int itemsCount, T itemToFind, IEqualityComparer<T> comparer)
		{
			for (var itemIdx = 0; itemIdx < itemsCount; itemIdx++)
			{
				var item = arrays[itemIdx];
				if (comparer.Equals(itemToFind, item))
				{
					return itemIdx;
				}
			}

			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this IList<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this List<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableArrayList<T> ToRecyclableArrayList<T>(this IEnumerable<T> values) => new(values, values.TryGetNonEnumeratedCount(out int count) ? count : RecyclableDefaults.Capacity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableArrayList<T> ToRecyclableArrayList<T>(this T[] values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableArrayList<T> ToRecyclableArrayList<T>(this List<T> values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableArrayList<T> ToRecyclableArrayList<T>(this RecyclableArrayList<T> values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this IEnumerable<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] RentArrayFromPool<T>(this int minSize, ArrayPool<T> arrayPool) => (minSize >= _minPooledArraySize)
			? arrayPool.Rent(minSize)
			: new T[minSize];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] RentArrayFromPool<T>(this long minSize, ArrayPool<T> arrayPool) => (minSize is >= _minPooledArraySize and <= int.MaxValue)
			? arrayPool.Rent((int)minSize)
			: new T[minSize];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReturnToPool<T>(this T[] array, ArrayPool<T> arrayPool)
		{
			if (array.LongLength is > _minPooledArraySize and <= int.MaxValue)
			{
				arrayPool.Return(array);
			}
		}

		//public static void AddRange<T>(this RecyclableList<T> destination, IEnumerable<T> source)
		//{
		//	foreach (var item in source)
		//	{
		//		destination.Add(item);
		//	}
		//}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long LimitTo(this long value, int limit)
			=> (value <= limit) ? value : limit;

		public static int IndexOf<T>(this T[] memory, int itemCount, T? itemToFind, IEqualityComparer<T> equalityComparer)
		{
			for (var itemIdx = 0; itemIdx < itemCount; itemIdx++)
			{
				var item = memory[itemIdx];
				if (equalityComparer.Equals(item, itemToFind))
				{
					return itemIdx;
				}
			}

			return -1;
		}
	}
}
