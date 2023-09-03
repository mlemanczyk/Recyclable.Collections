using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
	internal static class RecyclableListHelpers<T>
	{
		private static readonly bool NeedsClearing = !typeof(T).IsValueType;

		[Obsolete("This method MAY be removed in the near future.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains(RecyclableList<T[]> arrays, T item) => arrays.Any(x => x.Contains(item));

		[Obsolete("This method MAY be removed in the near future.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyTo(RecyclableList<T[]> arrays, long startingIndex, int blockSize, int lastBlockSize, T[] destinationArray, int destinationArrayIndex)
		{
			Span<T> arrayMemory = destinationArray.AsSpan();
			arrayMemory = arrayMemory[destinationArrayIndex..];
			// TODO: Convert "/" & "%" to bit-shifting
			int startingArrayIndex = (int)(startingIndex / blockSize);
			startingIndex %= blockSize;
			if (arrays._count > 0)
			{
				ReadOnlySpan<T> sourceMemory = arrays[0].AsSpan();
				var maxToCopy = (int)Math.Min(blockSize - startingIndex, arrayMemory.Length);
				if (arrays._count == 1)
				{
					maxToCopy = Math.Min(maxToCopy, lastBlockSize);
				}

				sourceMemory = sourceMemory[(int)startingIndex..maxToCopy];
				sourceMemory.CopyTo(arrayMemory);
				arrayMemory = arrayMemory[maxToCopy..];
			}

			for (var arrayIdx = startingArrayIndex + 1; arrayIdx < arrays._count - 1 && arrayMemory.Length > 0; arrayIdx++)
			{
				ReadOnlySpan<T> sourceMemory = arrays[arrayIdx].AsSpan();
				var maxToCopy = Math.Min(blockSize, arrayMemory.Length);
				sourceMemory = sourceMemory[..maxToCopy];
				sourceMemory.CopyTo(arrayMemory);
				arrayMemory = arrayMemory[maxToCopy..];
			}

			if (arrays._count > 1 && arrayMemory.Length > 0)
			{
				ReadOnlySpan<T> sourceMemory = arrays[^1].AsSpan();
				var maxToCopy = Math.Min(lastBlockSize, arrayMemory.Length);
				sourceMemory = sourceMemory[..maxToCopy];
				sourceMemory.CopyTo(arrayMemory);
			}
		}

		[Obsolete("This method WILL be removed in the near future. Please use the existing enumerators instead of it.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> Enumerate(RecyclableList<T[]> arrays, int chunkSize, long totalCount)
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long LongIndexOf(RecyclableList<T[]> arrays, int blockSize, T item, IEqualityComparer<T> comparer)
		{
			for (var arrayIdx = 0; arrayIdx < arrays._count; arrayIdx++)
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

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void ResizeAndCopy(RecyclableList<T> list)
		{
			// TODO: Measure performance
			// T[] newMemoryBlock = RecyclableListHelpers<T>._arrayPool.Rent(_capacity <<= 1);
			T[] oldMemoryBlock = list._memoryBlock;
			list._memoryBlock = (list._capacity <<= 1) < RecyclableDefaults.MinPooledArrayLength
				? new T[list._capacity]
				: RecyclableArrayPool<T>.RentShared(list._capacity);

			// & WAS SLOWER WITHOUT
			new Span<T>(oldMemoryBlock, 0, list._count).CopyTo(list._memoryBlock);

			if (oldMemoryBlock.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				// TODO: Measure gain vs relying on arrayPool to clear
				//if (NeedsClearing)
				//{
				//	Array.Clear(source);
				//}

				// // If anything, it has been already cleared above, so we don't need to repeat it.
				RecyclableArrayPool<T>.ReturnShared(oldMemoryBlock, NeedsClearing);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static int ResizeAndCopy(RecyclableList<T> sourceList, int oldSize, int newSize)
		{
			T[] oldMemoryBlock = sourceList._memoryBlock;
			sourceList._memoryBlock = newSize >= RecyclableDefaults.MinPooledArrayLength
				? RecyclableArrayPool<T>.RentShared(newSize)
				: new T[newSize];

			// & WAS SLOWER WITHOUT

			// & WAS SLOWER AS ARRAY
			new Span<T>(oldMemoryBlock, 0, oldSize).CopyTo(sourceList._memoryBlock);

			if (oldMemoryBlock.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				// TODO: Measure gain vs relying on arrayPool to clear
				//if (NeedsClearing)
				//{
				//	Array.Clear(source);
				//}

				// If anything, it has been already cleared above, so we don't need to repeat it.
				RecyclableArrayPool<T>.ReturnShared(oldMemoryBlock, NeedsClearing);
			}

			return newSize;
		}
	}
}
