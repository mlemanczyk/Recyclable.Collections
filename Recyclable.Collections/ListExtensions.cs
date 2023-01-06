using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	// The source code was taken from https://codereview.stackexchange.com/questions/205407/quicksort-without-recursion.
	// It was only beautified for .Net 6.0+.
	public static class ListExtensions
	{
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
