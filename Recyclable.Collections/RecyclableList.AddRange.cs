using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public static class zRecyclableListAddRange
    {
		private static void AddRangeEnumerated<T>(this RecyclableList<T> list, IEnumerable<T> items, int growByCount)
		{
			int targetItemIndex = list._count;
			Span<T> memorySpan;

			int i;
			var enumerator = items.GetEnumerator();

			int capacity = list._capacity;
			memorySpan = new((T[])list._memoryBlock);
			if (enumerator.MoveNext())
			{
				int available = capacity - targetItemIndex;
				do
				{
					if (targetItemIndex + growByCount > capacity)
					{
						capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, targetItemIndex, checked((int)BitOperations.RoundUpToPowerOf2((uint)(targetItemIndex + growByCount))));
						memorySpan = new((T[])list._memoryBlock);
						available = capacity - targetItemIndex;
					}

					for (i = 0; i < available; i++)
					{
						memorySpan[targetItemIndex++] = enumerator.Current;
						if (!enumerator.MoveNext())
						{
							break;
						}
					}
				}
				while (i >= available);
			}

			list._capacity = capacity;
			list._count = targetItemIndex;
		}

		private static void AddRangeWithKnownCount<T>(this RecyclableList<T> list, IEnumerable<T> items, int currentItemsCount, int requiredAdditionalCapacity)
		{
			list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, currentItemsCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)(currentItemsCount + requiredAdditionalCapacity))));

			Span<T> memorySpan = new((T[])list._memoryBlock);
			foreach (var item in items)
			{
				memorySpan[currentItemsCount++] = item;
			}

			list._count = currentItemsCount;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, in Array items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			Array.Copy(items, items.GetLowerBound(0), list._memoryBlock, list._count, items.LongLength);
			list._count += items.Length;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, in T[] items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			new Span<T>(items).CopyTo(new Span<T>((T[])list._memoryBlock, list._count, items.Length));
			list._count += items.Length;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, ReadOnlySpan<T> items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			items.CopyTo(new Span<T>((T[])list._memoryBlock, list._count, items.Length));
			list._count += items.Length;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, Span<T> items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			items.CopyTo(new Span<T>((T[])list._memoryBlock, list._count, items.Length));
			list._count += items.Length;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, List<T> items)
		{
			if (list._capacity < list._count + items.Count)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Count))));
			}

			items.CopyTo((T[])list._memoryBlock, list._count);
			list._count += items.Count;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, ICollection items)
		{
			if (list._capacity < list._count + items.Count)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Count))));
			}

			items.CopyTo(list._memoryBlock, list._count);
			list._count += items.Count;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, ICollection<T> items)
		{
			if (list._capacity < list._count + items.Count)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Count))));
			}

			items.CopyTo((T[])list._memoryBlock, list._count);
			list._count += items.Count;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, RecyclableList<T> items)
		{
			if (list._capacity < list._count + items._count)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items._count))));
			}

			new Span<T>((T[])items._memoryBlock, 0, items._count).CopyTo(new Span<T>((T[])list._memoryBlock, list._count, items._count));
			list._count += items._count;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, RecyclableLongList<T> items)
		{
			if (items._longCount == 0)
			{
				return;
			}

			int oldCount = list._count;
			long sourceItemsCount = items._longCount;
			if (oldCount + sourceItemsCount > int.MaxValue)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items in source and target table exceeds the maximum capacity of {nameof(RecyclableList<T>)}, equal {int.MaxValue}. Please consider using {nameof(RecyclableLongList<T>)}, instead");
			}

			int targetCapacity = oldCount + (int)sourceItemsCount;
			if (list._capacity < targetCapacity)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
			}

			// TODO: Avoid unnecessary range operator - pass as arguments instead.
			Span<T> targetSpan = new Span<T>((T[])list._memoryBlock)[oldCount..],
				itemsSpan;

			int blockIndex,
				sourceBlockSize = items._blockSize,
				lastBlockIndex = (int)(sourceItemsCount >> items._blockSizePow2BitShift) - 1;

			for (blockIndex = 0; blockIndex <= lastBlockIndex; blockIndex++)
			{
				itemsSpan = new(items._memoryBlocks[blockIndex], 0, sourceBlockSize);
				itemsSpan.CopyTo(targetSpan);
				targetSpan = targetSpan[sourceBlockSize..];
			}

			if (blockIndex == 0)
			{
				itemsSpan = new(items._memoryBlocks[blockIndex], 0, (int)sourceItemsCount);
				itemsSpan.CopyTo(targetSpan);
			}
			else if (blockIndex <= items._lastBlockWithData)
			{
				itemsSpan = new(items._memoryBlocks[blockIndex], 0, items._nextItemIndex);
				itemsSpan.CopyTo(targetSpan);
			}

			list._count = targetCapacity;
			list._version++;
		}

		public static IEnumerator AddRange<T>(this RecyclableList<T> list, IEnumerable source, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			int targetItemIndex = list._count;
			Span<T> memorySpan;

			int i;
			var enumerator = source.GetEnumerator();

			int capacity = list._capacity;
			memorySpan = new((T[])list._memoryBlock);
			if (enumerator.MoveNext())
			{
				int available = capacity - targetItemIndex;
				do
				{
					if (targetItemIndex + growByCount > capacity)
					{
						capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, targetItemIndex, checked((int)BitOperations.RoundUpToPowerOf2((uint)(targetItemIndex + growByCount))));
						memorySpan = new((T[])list._memoryBlock);
						available = capacity - targetItemIndex;
					}

					for (i = 0; i < available; i++)
					{
						memorySpan[targetItemIndex++] = (T)enumerator.Current;
						if (!enumerator.MoveNext())
						{
							break;
						}
					}
				}
				while (i >= available);
			}

			list._capacity = capacity;
			list._count = targetItemIndex;
			return enumerator;
		}

		public static void AddRange<T>(this RecyclableList<T> list, IReadOnlyList<T> items)
		{
			int sourceItemsCount = items.Count;
			if (list._capacity < list._count + sourceItemsCount)
			{
				list._capacity = RecyclableListHelpers<T>.ResizeAndCopy(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + sourceItemsCount))));
			}

			Span<T> memorySpan = new((T[])list._memoryBlock, list._count, sourceItemsCount);
			for (var sourceItemIndex = 0; sourceItemIndex < sourceItemsCount; sourceItemIndex++)
			{
				memorySpan[sourceItemIndex] = items[sourceItemIndex];
			}

			list._count += sourceItemsCount;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, IEnumerable<T> items, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			if (items is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(list, sourceRecyclableList);
			}
			else if (items is RecyclableLongList<T> sourceRecyclableLongList)
			{
				AddRange(list, sourceRecyclableLongList);
			}
			else if (items is T[] sourceArray)
			{
				AddRange(list, sourceArray);
			}
			else if (items is Array sourceArrayWithObjects)
			{
				AddRange(list, sourceArrayWithObjects);
			}
			else if (items is List<T> sourceList)
			{
				AddRange(list, sourceList);
			}
			else if (items is ICollection<T> sourceICollection)
			{
				AddRange(list, sourceICollection);
			}
			else if (items is ICollection sourceICollectionWithObjects)
			{
				AddRange(list, sourceICollectionWithObjects);
			}
			else if (items is IReadOnlyList<T> sourceIReadOnlyList)
			{
				AddRange(list, sourceIReadOnlyList);
			}
			else if (items.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity) && requiredAdditionalCapacity != 0)
			{
				AddRangeWithKnownCount(list, items, list._count, requiredAdditionalCapacity);
				list._version++;
			}
			else
			{
				AddRangeEnumerated(list, items, growByCount);
				list._version++;
			}
		}
    }
}