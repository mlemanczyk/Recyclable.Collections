using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	internal static class RecyclableQueueHelpers<T>
	{
		[Obsolete("This method MAY be removed in the near future.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool Contains(RecyclableList<T[]> arrays, T item) => arrays.Any(x => x.Contains(item));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToArrayIndex(long index, int blockSize) => (int)(index / blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ToItemIndex(long index, int blockSize) => index % blockSize;

		[Obsolete("This method MAY be removed in the near future.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void CopyTo(RecyclableList<T[]> arrays, long startingIndex, int blockSize, int lastBlockSize, T[] destinationArray, int destinationArrayIndex)
		{
			Span<T> arrayMemory = destinationArray;
			arrayMemory = arrayMemory[destinationArrayIndex..];
			// TODO: Convert "/" & "%" to bit-shifting
			int startingArrayIndex = (int)(startingIndex / blockSize);
			startingIndex %= blockSize;
			if (arrays._count > 0)
			{
				ReadOnlySpan<T> sourceMemory = arrays[0];
				var maxToCopy = (int)Math.Min(blockSize - startingIndex, arrayMemory.Length);
				if (arrays._count == 1)
				{
					maxToCopy = Math.Min(maxToCopy, lastBlockSize);
				}

				sourceMemory = sourceMemory[checked((int)startingIndex)..maxToCopy];
				sourceMemory.CopyTo(arrayMemory);
				arrayMemory = arrayMemory[maxToCopy..];
			}

			for (var arrayIdx = startingArrayIndex + 1; arrayIdx < arrays._count - 1 && arrayMemory.Length > 0; arrayIdx++)
			{
				ReadOnlySpan<T> sourceMemory = arrays[arrayIdx];
				var maxToCopy = Math.Min(blockSize, arrayMemory.Length);
				sourceMemory = sourceMemory[..maxToCopy];
				sourceMemory.CopyTo(arrayMemory);
				arrayMemory = arrayMemory[maxToCopy..];
			}

			if (arrays._count > 1 && arrayMemory.Length > 0)
			{
				ReadOnlySpan<T> sourceMemory = arrays[^1];
				var maxToCopy = Math.Min(lastBlockSize, arrayMemory.Length);
				sourceMemory = sourceMemory[..maxToCopy];
				sourceMemory.CopyTo(arrayMemory);
			}
		}

		[Obsolete("This method WILL be removed in the near future. Please use the existing enumerators instead of it.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static IEnumerable<T> Enumerate(RecyclableList<T[]> arrays, int chunkSize, long totalCount)
		{
			long currentCount = 0;
			for (var arrayIdx = 0; (arrayIdx < arrays._count) && (currentCount < totalCount); arrayIdx++)
			{
				var array = arrays[arrayIdx];
				currentCount += Math.Min(chunkSize, totalCount);
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

		[Obsolete]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static long LongIndexOf(RecyclableList<T[]> arrays, int blockSize, T item, IEqualityComparer<T> comparer)
		{
			for (var arrayIdx = 0; arrayIdx < arrays._count; arrayIdx++)
			{
				var array = arrays[arrayIdx];
				ReadOnlySpan<T> arrayMemory = array;
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
	}
}
