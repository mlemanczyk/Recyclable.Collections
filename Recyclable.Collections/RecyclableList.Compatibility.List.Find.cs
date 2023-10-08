using System.Numerics;

namespace Recyclable.Collections
{
	[Obsolete("It'll be moved to Recyclable.Collections.Compatibility.List NuGet package in the next release.")]
	public static class zRecyclableListCompatibilityListFind
	{
		public static bool Exists<T>(this RecyclableList<T> list, Predicate<T> match)
		{
			int sourceItemsCount = list._count;
			if (sourceItemsCount == 0)
			{
				return false;
			}

			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			for (var itemIndex = 0; itemIndex < sourceItemsCount; itemIndex++)
			{
				if (match(sourceSpan[itemIndex]))
				{
					return true;
				}
			}

			return false;
		}

		public static T? Find<T>(this RecyclableList<T> list, Predicate<T> match)
		{
			int sourceItemsCount = list._count;
			if (sourceItemsCount == 0)
			{
				return default;
			}

			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			for (var itemIndex = 0; itemIndex < sourceItemsCount; itemIndex++)
			{
				if (match(sourceSpan[itemIndex]))
				{
					return sourceSpan[itemIndex];
				}
			}

			return default;
		}

		public static RecyclableList<T> FindAll<T>(this RecyclableList<T> list, Predicate<T> match)
		{
			int sourceItemsCount = list._count;
			if (sourceItemsCount == 0)
			{
				return new();
			}

			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			RecyclableList<T> result = new(sourceItemsCount >> 3);
			Span<T> resultSpan = result._memoryBlock;
			int capacity = result._capacity,
				resultItemsCount = 0;

			for (var itemIndex = 0; itemIndex < sourceItemsCount; itemIndex++)
			{
				if (match(sourceSpan[itemIndex]))
				{
					if (resultItemsCount + 1 > capacity)
					{
						capacity = checked((int)BitOperations.RoundUpToPowerOf2((uint)resultItemsCount + 1));
						_ = RecyclableListHelpers<T>.EnsureCapacity(result, resultItemsCount, capacity);
						resultSpan = result._memoryBlock;
					}

					resultSpan[resultItemsCount++] = sourceSpan[itemIndex];
				}
			}

			result._capacity = capacity;
			result._count = resultItemsCount;
			return result;
		}
		public static RecyclableList<int> FindAllIndexes<T>(this RecyclableList<T> list, Predicate<T> match)
		{
			int sourceItemsCount = list._count;
			if (sourceItemsCount == 0)
			{
				return new();
			}

			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			RecyclableList<int> result = new(sourceItemsCount >> 3);
			Span<int> resultSpan = result._memoryBlock;
			int capacity = result._capacity,
				resultItemsCount = 0;

			for (var itemIndex = 0; itemIndex < sourceItemsCount; itemIndex++)
			{
				if (match(sourceSpan[itemIndex]))
				{
					if (resultItemsCount + 1 > capacity)
					{
						capacity = checked((int)BitOperations.RoundUpToPowerOf2((uint)resultItemsCount + 1));
						_ = RecyclableListHelpers<int>.EnsureCapacity(result, resultItemsCount, capacity);
						resultSpan = result._memoryBlock;
					}

					resultSpan[resultItemsCount++] = itemIndex;
				}
			}

			result._capacity = capacity;
			result._count = resultItemsCount;
			return result;
		}

		public static int FindIndex<T>(this RecyclableList<T> list, int startIndex, int count, Predicate<T> match)
		{
			int sourceItemsCount = list._count;
			if (sourceItemsCount == 0 || startIndex >= sourceItemsCount)
			{
				return RecyclableDefaults.ItemNotFoundIndex;
			}

			sourceItemsCount = Math.Min(sourceItemsCount, startIndex + sourceItemsCount);
			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			for (var itemIndex = startIndex; itemIndex < sourceItemsCount; itemIndex++)
			{
				if (match(sourceSpan[itemIndex]))
				{
					return itemIndex;
				}
			}

			return RecyclableDefaults.ItemNotFoundIndex;
		}

		public static int FindIndex<T>(this RecyclableList<T> list, int startIndex, Predicate<T> match)
		{
			int sourceItemsCount = list._count;
			if (sourceItemsCount == 0 || startIndex >= sourceItemsCount)
			{
				return RecyclableDefaults.ItemNotFoundIndex;
			}

			sourceItemsCount = Math.Min(sourceItemsCount, startIndex + sourceItemsCount);
			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			for (var itemIndex = startIndex; itemIndex < sourceItemsCount; itemIndex++)
			{
				if (match(sourceSpan[itemIndex]))
				{
					return itemIndex;
				}
			}

			return RecyclableDefaults.ItemNotFoundIndex;
		}
		
		public static int FindIndex<T>(this RecyclableList<T> list, Predicate<T> match)
		{
			int sourceItemsCount = list._count;
			if (sourceItemsCount == 0)
			{
				return RecyclableDefaults.ItemNotFoundIndex;
			}

			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			for (var itemIndex = 0; itemIndex < sourceItemsCount; itemIndex++)
			{
				if (match(sourceSpan[itemIndex]))
				{
					return itemIndex;
				}
			}

			return RecyclableDefaults.ItemNotFoundIndex;
		}

		public static T? FindLast<T>(this RecyclableList<T> list, Predicate<T> match)
		{
			if (list._count == 0)
			{
				return default;
			}

			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			for (var itemIndex = list._count - 1; itemIndex >= 0; itemIndex--)
			{
				if (match(sourceSpan[itemIndex]))
				{
					return sourceSpan[itemIndex];
				}
			}

			return default;
		}

		public static int FindLastIndex<T>(this RecyclableList<T> list, int startIndex, int count, Predicate<T> match)
		{
			if (count == 0)
			{
				if (startIndex != -1)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException_Index();
				}
			}
			else  if (count < 0 || startIndex - count + 1 < 0)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException_Count();
			}

			if (startIndex < 0)
			{
				return RecyclableDefaults.ItemNotFoundIndex;
			}

			int sourceItemsCount = count; // startIndex - count + 1 >= 0 ? count : startIndex + 1;
			if (sourceItemsCount == 0)
			{
				return RecyclableDefaults.ItemNotFoundIndex;
			}

			int lastItemIndex = startIndex - sourceItemsCount;
			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			for (var itemIndex = startIndex; itemIndex > lastItemIndex; itemIndex--)
			{
				if (match(sourceSpan[itemIndex]))
				{
					return itemIndex;
				}
			}

			return RecyclableDefaults.ItemNotFoundIndex;
		}

		public static int FindLastIndex<T>(this RecyclableList<T> list, int startIndex, Predicate<T> match)
		{
			if (list._count == 0 || startIndex < 0)
			{
				return RecyclableDefaults.ItemNotFoundIndex;
			}

			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			for (var itemIndex = startIndex; itemIndex >= 0; itemIndex--)
			{
				if (match(sourceSpan[itemIndex]))
				{
					return itemIndex;
				}
			}

			return RecyclableDefaults.ItemNotFoundIndex;
		}

		public static int FindLastIndex<T>(this RecyclableList<T> list, Predicate<T> match)
		{
			if (list._count == 0)
			{
				return RecyclableDefaults.ItemNotFoundIndex;
			}

			ReadOnlySpan<T> sourceSpan = list._memoryBlock;
			for (var itemIndex = list._count - 1; itemIndex >= 0; itemIndex--)
			{
				if (match(sourceSpan[itemIndex]))
				{
					return itemIndex;
				}
			}

			return RecyclableDefaults.ItemNotFoundIndex;
		}
	}
}
