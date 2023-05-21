using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	// The source code was taken from https://codereview.stackexchange.com/questions/205407/quicksort-without-recursion.
	// It was only beautified for .Net 6.0+.
	public static class ListExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(this RecyclableList<T[]> arrays, T item) => arrays.Any(x => x.Contains(item));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyTo<T>(this RecyclableList<T[]> arrays, long startingIndex, int blockSize, int lastBlockSize, T[] destinationArray, int destinationArrayIndex)
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
		public static IEnumerable<T> Enumerate<T>(this RecyclableList<T[]> arrays, int chunkSize, long totalCount)
		{
			long currentCount = 0;
			for (var arrayIdx = 0; (arrayIdx < arrays.Count) && (currentCount < totalCount); arrayIdx++)
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

		public static void InsertionSort<T>(this IList<T> values, int startIndex, int endIndex, IComparer<T> comparer)
		{
			var left = startIndex;
			while (left < endIndex)
			{
				int right = left;
				T? temp = values[++left];
				while (right >= startIndex && 0 < comparer.Compare(values[right], temp))
				{
					values[right + 1] = values[right--];
				}

				values[right + 1] = temp;
			}
		}
		public static void InsertionSort<T>(this IList<T> values, IComparer<T> comparer)
		{
			values.InsertionSort(0, values.Count - 1, comparer);
		}
		public static void InsertionSort<T>(this IList<T> values)
		{
			values.InsertionSort(Comparer<T>.Default);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long LongIndexOf<T>(this RecyclableList<T[]> arrays, int blockSize, T item, IEqualityComparer<T> comparer)
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

		public static void QuickSort<T>(this IList<T> values, int startIndex, int endIndex, IComparer<T> comparer, IRandomNumberGenerator randomNumberGenerator)
		{
			var range = (startIndex, endIndex);
			var stack = new Stack<(int, int)>();

			do
			{
				startIndex = range.startIndex;
				endIndex = range.endIndex;

				if (endIndex - startIndex + 1 < 31)
				{
					values.InsertionSort(startIndex, endIndex, comparer);

					continue;
				}

				var pivot = values.SampleMedian(startIndex, endIndex, comparer, randomNumberGenerator);
				var left = startIndex;
				var right = endIndex;

				while (left <= right)
				{
					while (0 > comparer.Compare(values[left], pivot))
					{
						left++;
					}

					while (0 > comparer.Compare(pivot, values[right]))
					{
						right--;
					}

					if (left <= right)
					{
						values.Swap(left++, right--);
					}
				}

				if (startIndex < right)
				{
					stack.Push((startIndex, right));
				}

				if (left < endIndex)
				{
					stack.Push((left, endIndex));
				}
			}
			while (stack.TryPop(out range));
		}
		public static void QuickSort<T>(this IList<T> values, IComparer<T> comparer, IRandomNumberGenerator randomNumberGenerator)
			=> values.QuickSort(0, values.Count - 1, comparer, randomNumberGenerator);

		public static void QuickSort<T>(this IList<T> values)
			=> values.QuickSort(Comparer<T>.Default, new SystemRandomNumberGenerator());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static T SampleMedian<T>(this IList<T> values, int startIndex, int endIndex, IComparer<T> comparer, IRandomNumberGenerator randomNumberGenerator)
		{
			var left = randomNumberGenerator.NextInt32(startIndex, endIndex);
			var middle = randomNumberGenerator.NextInt32(startIndex, endIndex);
			var right = randomNumberGenerator.NextInt32(startIndex, endIndex);

			if (0 > comparer.Compare(values[right], values[left]))
			{
				values.Swap(right, left);
			}

			if (0 > comparer.Compare(values[middle], values[left]))
			{
				values.Swap(middle, left);
			}

			if (0 > comparer.Compare(values[right], values[middle]))
			{
				values.Swap(right, middle);
			}

			return values[middle];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Swap<T>(this IList<T> values, int xIndex, int yIndex)
			=> (values[yIndex], values[xIndex]) = (values[xIndex], values[yIndex]);
	}
}
