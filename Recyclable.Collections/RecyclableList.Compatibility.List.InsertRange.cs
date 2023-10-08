using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public static class zRecyclableListCompatibilityListInsertRange
	{
		private static void InsertRangeEnumerated<T>(this RecyclableList<T> list, int index, IEnumerable<T> items, int growByCount)
		{
			var enumerator = items.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return;
			}

			int capacity = list._capacity,
				added = 0,
				available = capacity - list._count,
				i,
				movingBlockLength = list._count - index;

			T[] memoryBlock = list._memoryBlock;
			if (available > 0)
			{
				Array.Copy(memoryBlock, index, memoryBlock, capacity - movingBlockLength, movingBlockLength);
			}

			Span<T> memorySpan = memoryBlock;
			do
			{
				if (index + available + movingBlockLength > capacity)
				{
					var capacityBeforeIncrease = capacity;
					capacity = checked((int)BitOperations.RoundUpToPowerOf2((uint)(capacity + growByCount)));
					_ = RecyclableListHelpers<T>.EnsureCapacity(list, memoryBlock.Length, capacity);
					memorySpan = memoryBlock = list._memoryBlock;
					available = capacity - index - movingBlockLength;
					Array.Copy(memoryBlock, capacityBeforeIncrease - movingBlockLength, memoryBlock, capacity - movingBlockLength, movingBlockLength);
				}

				for (i = 0; i < available; i++)
				{
					memorySpan[index++] = enumerator.Current;
					if (!enumerator.MoveNext())
					{
						added++;
						break;
					}
				}

				added += i;
			}
			while (i == available);

			if (i != available)
			{
				Array.Copy(memoryBlock, capacity - movingBlockLength, memoryBlock, index, movingBlockLength);
				if (RecyclableList<T>._needsClearing)
				{
					Array.Clear(memoryBlock, capacity - movingBlockLength, movingBlockLength);
				}
			}

			list._count += added;
			list._capacity = capacity;
		}

		private static void InsertRangeWithKnownCount<T>(this RecyclableList<T> list, int index, IEnumerable<T> items, int currentItemsCount, int requiredAdditionalCapacity)
		{
			list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, currentItemsCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)(currentItemsCount + requiredAdditionalCapacity))));

			Array.Copy(list._memoryBlock, index, list._memoryBlock, index + requiredAdditionalCapacity, list._count - index);
			Span<T> memorySpan = list._memoryBlock;
			foreach (var item in items)
			{
				memorySpan[index++] = item;
			}

			list._count += requiredAdditionalCapacity;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, in Array items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			T[] memoryBlock = list._memoryBlock;
			Array.Copy(memoryBlock, index, memoryBlock, index + items.Length, list._count - index);
			Array.Copy(items, items.GetLowerBound(0), memoryBlock, index, items.Length);
			list._count += items.Length;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, in T[] items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			T[] memoryBlock = list._memoryBlock;
			Array.Copy(memoryBlock, index, memoryBlock, index + items.Length, list._count - index);
			new ReadOnlySpan<T>(items).CopyTo(new Span<T>(memoryBlock, index, items.Length));
			list._count += items.Length;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, ReadOnlySpan<T> items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			T[] memoryBlock = list._memoryBlock;
			Array.Copy(memoryBlock, index, memoryBlock, index + items.Length, list._count - index);
			items.CopyTo(new Span<T>(memoryBlock, index, items.Length));
			list._count += items.Length;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, Span<T> items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			T[] memoryBlock = list._memoryBlock;
			Array.Copy(memoryBlock, index, memoryBlock, index + items.Length, list._count - index);
			items.CopyTo(new Span<T>(memoryBlock, index, items.Length));
			list._count += items.Length;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, List<T> items)
		{
			if (list._capacity < list._count + items.Count)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Count))));
			}

			T[] memoryBlock = list._memoryBlock;
			Array.Copy(memoryBlock, index, memoryBlock, index + items.Count, list._count - index);
			items.CopyTo(memoryBlock, index);
			list._count += items.Count;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, ICollection items)
		{
			if (list._capacity < list._count + items.Count)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Count))));
			}

			T[] memoryBlock = list._memoryBlock;
			Array.Copy(memoryBlock, index, memoryBlock, index + items.Count, list._count - index);
			items.CopyTo(list._memoryBlock, index);
			list._count += items.Count;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, ICollection<T> items)
		{
			if (list._capacity < list._count + items.Count)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Count))));
			}

			T[] memoryBlock = list._memoryBlock;
			Array.Copy(memoryBlock, index, memoryBlock, index + items.Count, list._count - index);
			items.CopyTo(memoryBlock, index);
			list._count += items.Count;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, RecyclableList<T> items)
		{
			if (list._capacity < list._count + items._count)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items._count))));
			}

			T[] memoryBlock = list._memoryBlock;
			Array.Copy(memoryBlock, index, memoryBlock, index + items.Count, list._count - index);
			new ReadOnlySpan<T>(items._memoryBlock, 0, items._count).CopyTo(new(list._memoryBlock, index, items._count));
			list._count += items._count;
			list._version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, RecyclableLongList<T> items)
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
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
			}

			T[] memoryBlock = list._memoryBlock;
			Array.Copy(memoryBlock, index, memoryBlock, index + items.Count, oldCount - index);
			Span<T> targetSpan = new(list._memoryBlock, index, list._capacity - index),
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
				itemsSpan = new(items._memoryBlocks[blockIndex], 0, items._nextItemIndex > 0 ? items._nextItemIndex : sourceBlockSize);
				itemsSpan.CopyTo(targetSpan);
			}

			list._count = targetCapacity;
			list._version++;
		}

		public static IEnumerator InsertRange<T>(this RecyclableList<T> list, int index, IEnumerable source, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			int targetItemIndex = list._count;
			Span<T> memorySpan;

			int i;
			var enumerator = source.GetEnumerator();

			int capacity = list._capacity;
			memorySpan = list._memoryBlock;
			if (enumerator.MoveNext())
			{
				int available = capacity - targetItemIndex;
				do
				{
					if (targetItemIndex + growByCount > capacity)
					{
						capacity = RecyclableListHelpers<T>.EnsureCapacity(list, targetItemIndex, checked((int)BitOperations.RoundUpToPowerOf2((uint)(targetItemIndex + growByCount))));
						memorySpan = list._memoryBlock;
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

		public static void InsertRange<T>(this RecyclableList<T> list, int index, IReadOnlyList<T> items)
		{
			int sourceItemsCount = items.Count;
			if (list._capacity < list._count + sourceItemsCount)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + sourceItemsCount))));
			}

			Span<T> memorySpan = new(list._memoryBlock, list._count, sourceItemsCount);
			for (var sourceItemIndex = 0; sourceItemIndex < sourceItemsCount; sourceItemIndex++)
			{
				memorySpan[sourceItemIndex] = items[sourceItemIndex];
			}

			list._count += sourceItemsCount;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableList<T> list, int index, IEnumerable<T> items, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			if (items is RecyclableList<T> sourceRecyclableList)
			{
				InsertRange(list, index, sourceRecyclableList);
			}
			else if (items is RecyclableLongList<T> sourceRecyclableLongList)
			{
				InsertRange(list, index, sourceRecyclableLongList);
			}
			else if (items is T[] sourceArray)
			{
				InsertRange(list, index, sourceArray);
			}
			else if (items is Array sourceArrayWithObjects)
			{
				InsertRange(list, index, sourceArrayWithObjects);
			}
			else if (items is List<T> sourceList)
			{
				InsertRange(list, index, sourceList);
			}
			else if (items is ICollection<T> sourceICollection)
			{
				InsertRange(list, index, sourceICollection);
			}
			else if (items is ICollection sourceICollectionWithObjects)
			{
				InsertRange(list, index, sourceICollectionWithObjects);
			}
			else if (items is IReadOnlyList<T> sourceIReadOnlyList)
			{
				InsertRange(list, index, sourceIReadOnlyList);
			}
			else if (items.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity) && requiredAdditionalCapacity != 0)
			{
				InsertRangeWithKnownCount(list, index, items, list._count, requiredAdditionalCapacity);
			}
			else
			{
				InsertRangeEnumerated(list, index, items, growByCount);
			}
		}
	}
}