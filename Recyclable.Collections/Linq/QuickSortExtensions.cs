// Ignore Spelling: Linq
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

// The source code was taken from
// https://codereview.stackexchange.com/questions/205407/quicksort-without-recursion.
//
// It was only beautified for .Net 6.0+.

namespace Recyclable.Collections.Linq
{
	internal static class QuickSortExtensions<T>
	{

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Swap(IList<T> values, int xIndex, int yIndex)
			=> (values[yIndex], values[xIndex]) = (values[xIndex], values[yIndex]);
	}
}
