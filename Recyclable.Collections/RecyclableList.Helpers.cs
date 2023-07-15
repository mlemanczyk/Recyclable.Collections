﻿using System.Buffers;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
	internal static class RecyclableListHelpers<T>
	{
		[Obsolete("This method MAY be removed in the near future.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains(RecyclableList<T[]> arrays, T item) => arrays.Any(x => x.Contains(item));

		[Obsolete("This method MAY be removed in the near future.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyTo(RecyclableList<T[]> arrays, long startingIndex, int blockSize, int lastBlockSize, T[] destinationArray, int destinationArrayIndex)
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
					maxToCopy = Math.Min(maxToCopy, lastBlockSize);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static int EnsureCapacity(RecyclableList<T> list, int requestedCapacity)
		{
			int newCapacity = list._capacity;
			while (newCapacity < requestedCapacity)
			{
				newCapacity <<= 1;
			}

			ResizeAndCopy(list, newCapacity);
			return list._capacity;
		}

		[Obsolete("This method WILL be removed in the near future. Please use the existing enumerators instead of it.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> Enumerate(RecyclableList<T[]> arrays, int chunkSize, long totalCount)
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

		public static void InsertionSort(IList<T> values, int startIndex, int endIndex, IComparer<T> comparer)
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

		public static void InsertionSort(IList<T> values, IComparer<T> comparer)
		{
			InsertionSort(values, 0, values.Count - 1, comparer);
		}

		public static void InsertionSort(IList<T> values)
		{
			InsertionSort(values, Comparer<T>.Default);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long LongIndexOf(RecyclableList<T[]> arrays, int blockSize, T item, IEqualityComparer<T> comparer)
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

		// The source code was taken from https://codereview.stackexchange.com/questions/205407/quicksort-without-recursion.
		// It was only beautified for .Net 6.0+.
		public static void QuickSort(IList<T> values, int startIndex, int endIndex, IComparer<T> comparer, IRandomNumberGenerator randomNumberGenerator)
		{
			var range = (startIndex, endIndex);
			var stack = new Stack<(int, int)>();

			do
			{
				startIndex = range.startIndex;
				endIndex = range.endIndex;

				if (endIndex - startIndex + 1 < 31)
				{
					InsertionSort(values, startIndex, endIndex, comparer);

					continue;
				}

				var pivot = SampleMedian(values, startIndex, endIndex, comparer, randomNumberGenerator);
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
						Swap(values, left++, right--);
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
		public static void QuickSort(IList<T> values, IComparer<T> comparer, IRandomNumberGenerator randomNumberGenerator)
			=> QuickSort(values, 0, values.Count - 1, comparer, randomNumberGenerator);

		public static void QuickSort(IList<T> values)
			=> QuickSort(values, Comparer<T>.Default, new SystemRandomNumberGenerator());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static T SampleMedian(IList<T> values, int startIndex, int endIndex, IComparer<T> comparer, IRandomNumberGenerator randomNumberGenerator)
		{
			var left = randomNumberGenerator.NextInt32(startIndex, endIndex);
			var middle = randomNumberGenerator.NextInt32(startIndex, endIndex);
			var right = randomNumberGenerator.NextInt32(startIndex, endIndex);

			if (0 > comparer.Compare(values[right], values[left]))
			{
				Swap(values, right, left);
			}

			if (0 > comparer.Compare(values[middle], values[left]))
			{
				Swap(values, middle, left);
			}

			if (0 > comparer.Compare(values[right], values[middle]))
			{
				Swap(values, right, middle);
			}

			return values[middle];
		}

		private static readonly bool NeedsClearing = !typeof(T).IsValueType;

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void ResizeAndCopy(RecyclableList<T> list)
		{
			// TODO: Measure performance
			// T[] newMemoryBlock = RecyclableListHelpers<T>._arrayPool.Rent(_capacity <<= 1);
			T[] oldMemoryBlock = list._memoryBlock;
			list._memoryBlock = (list._capacity <<= 1) < RecyclableDefaults.MinPooledArrayLength
				? new T[list._capacity]
				: RecyclableArrayPool<T>.Rent(list._capacity);

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
				RecyclableArrayPool<T>.Return(oldMemoryBlock, NeedsClearing);
			}

			// list._memoryBlock = newMemoryBlock;
			list._capacity = list._memoryBlock.Length;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void ResizeAndCopy(RecyclableList<T> sourceList, int newSize)
		{
			T[] newMemoryBlock = newSize >= RecyclableDefaults.MinPooledArrayLength
				? RecyclableArrayPool<T>.Rent(newSize)
				: new T[newSize];

			// & WAS SLOWER WITHOUT
			T[] oldMemoryBlock = sourceList._memoryBlock;

			// & WAS SLOWER AS ARRAY
			new Span<T>(oldMemoryBlock, 0, sourceList._count).CopyTo(newMemoryBlock);

			if (oldMemoryBlock.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				// TODO: Measure gain vs relying on arrayPool to clear
				//if (NeedsClearing)
				//{
				//	Array.Clear(source);
				//}

				// If anything, it has been already cleared above, so we don't need to repeat it.
				RecyclableArrayPool<T>.Return(oldMemoryBlock, NeedsClearing);
			}

			sourceList._memoryBlock = newMemoryBlock;
			sourceList._capacity = newMemoryBlock.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Swap(IList<T> values, int xIndex, int yIndex)
			=> (values[yIndex], values[xIndex]) = (values[xIndex], values[yIndex]);
	}
}
