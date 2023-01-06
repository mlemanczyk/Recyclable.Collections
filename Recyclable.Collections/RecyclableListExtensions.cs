using System.Buffers;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public static class RecyclableListExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyTo<T>(this List<T[]> arrays, long startingIndex, int blockSize, int lastBlockSize, T[] destinationArray, int destinationArrayIndex)
		{
			Span<T> arrayMemory = destinationArray.AsSpan();
			arrayMemory = arrayMemory[destinationArrayIndex..];
			int startingArrayIndex = (int)(startingIndex / blockSize);
			startingIndex %= blockSize;
			if (arrays.Count > 0)
			{
				ReadOnlySpan<T> sourceMemory = arrays[0].AsSpan();
				var maxToCopy = (int)Math.Min(blockSize - startingIndex, arrayMemory.Length);
				if (arrays.Count == 1)
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(this List<T[]> arrays, T item) => arrays.Any(x => x.Contains(item));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> Enumerate<T>(this List<T[]> arrays, int chunkSize, long totalCount)
		{
			long currentCount = 0;
			for (var arrayIdx = 0; arrayIdx < arrays.Count; arrayIdx++)
			{
				var array = arrays[arrayIdx];
				currentCount += chunkSize;
				switch (currentCount < totalCount)
				{
					case true:
						for (var valueIdx = 0; valueIdx < chunkSize; valueIdx++)
						{
							yield return array[valueIdx];
						}

						break;

					case false:
						var partialCount = (int)(totalCount % chunkSize);
						int maxCount = partialCount > 0 ? partialCount : chunkSize;
						for (var valueIdx = 0; valueIdx < maxCount; valueIdx++)
						{
							yield return array[valueIdx];
						}

						break;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T LimitTo<T>(this T value, T maxValue, IComparer<T> comparer)
			=> comparer.Compare(value, maxValue) > 0 ? maxValue : value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long LongIndexOf<T>(this List<T[]> arrays, int blockSize, T item, IEqualityComparer<T> comparer)
		{
			for (var arrayIdx = 0; arrayIdx < arrays.Count; arrayIdx++)
			{
				var array = arrays[arrayIdx];
				Span<T> arrayMemory = array.AsSpan();
				for (int memoryIdx = 0; memoryIdx < arrayMemory.Length; memoryIdx++)
				{
					if (comparer.Equals(arrayMemory[memoryIdx], item))
					{
						return (arrayIdx * (long)blockSize) + memoryIdx;
					}
				}
			}

			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToArrayIndex(this long index, int blockSize) => (int)(index / blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ToItemIndex(this long index, int blockSize) => index % blockSize;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this IList<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this IEnumerable<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] RentArrayFromPool<T>(this int minSize) => ArrayPool<T>.Shared.Rent(minSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReturnToPool<T>(this T[] array) => ArrayPool<T>.Shared.Return(array);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetItem<T>(this List<T[]> arrays, in int blockSize, in long index, in T value)
			=> arrays[(int)(index / blockSize)][index % blockSize] = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetItem<T>(this List<T[]> arrays, in int blockSize, in long index)
			=> arrays[(int)(index / blockSize)][index % blockSize];

	}
}
