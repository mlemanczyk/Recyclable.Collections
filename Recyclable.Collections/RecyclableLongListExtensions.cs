﻿using System.Buffers;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	internal static class RecyclableLongListExtensions
	{
		private const int _minPooledArraySize = RecyclableDefaults.MinPooledArrayLength;

		public static void CopyTo<T>(this T[][] sourceMemoryBlocks, long startingIndex, int blockSize, long itemsCount, T[] destinationArray, int destinationArrayIndex)
		{
			if (itemsCount <= 0)
			{
				return;
			}

			Span<T> sourceBlockMemory;
			Span<T> destinationArrayMemory;
			int startingBlockIndex = (int)(startingIndex / blockSize);
			int lastBlockIndex = (int)((itemsCount / blockSize) + (itemsCount % blockSize > 0 ? 1 : 0)) - 1;
			Span<T[]> sourceMemoryBlocksSpan = new(sourceMemoryBlocks, startingBlockIndex, lastBlockIndex + 1);
			destinationArrayMemory = new Span<T>(destinationArray, destinationArrayIndex, (int)Math.Min(destinationArray.Length - destinationArrayIndex, itemsCount));
			int memoryBlockIndex;
			for (memoryBlockIndex = 0; memoryBlockIndex < lastBlockIndex; memoryBlockIndex++)
			{
				sourceBlockMemory = new(sourceMemoryBlocksSpan[memoryBlockIndex], 0, blockSize);
				sourceBlockMemory.CopyTo(destinationArrayMemory);
				destinationArrayMemory = destinationArrayMemory[blockSize..];
			}

			if (itemsCount - (lastBlockIndex * blockSize) > 0)
			{
				sourceBlockMemory = new(sourceMemoryBlocksSpan[lastBlockIndex], 0, (int)(itemsCount - (lastBlockIndex * blockSize)));
				sourceBlockMemory.CopyTo(destinationArrayMemory);
			}
		}

		public static void CopyTo<T>(this T[][] sourceMemoryBlocks, long startingIndex, int blockSize, long itemsCount, Array destinationArray, int destinationArrayIndex)
		{
			if (itemsCount <= 0)
			{
				return;
			}

			int startingBlockIndex = (int)(startingIndex / blockSize);
			int lastBlockIndex = (int)((itemsCount / blockSize) + (itemsCount % blockSize > 0 ? 1 : 0)) - 1;

			var destinationIndex = destinationArrayIndex;
			Span<T[]> sourceMemoryBlocksSpan = new(sourceMemoryBlocks, startingBlockIndex, lastBlockIndex + 1);
			for (int memoryBlockIndex = 0; memoryBlockIndex < lastBlockIndex; memoryBlockIndex++)
			{
				Array.ConstrainedCopy(sourceMemoryBlocksSpan[memoryBlockIndex], 0, destinationArray, destinationIndex, blockSize);
				destinationIndex += blockSize;
			}

			if (itemsCount - destinationIndex > 0)
			{
				Array.ConstrainedCopy(sourceMemoryBlocksSpan[lastBlockIndex], 0, destinationArray, destinationIndex, (int)(itemsCount - destinationIndex));
			}
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public static bool Contains<T>(this RecyclableLongList<T[]> arrays, T item) => arrays.Any(x => x.Contains(item));

		public static IEnumerable<T> Enumerate<T>(this T[][] arrays, int chunkSize, long totalCount)
		{
			long currentCount = 0;
			for (var arrayIdx = 0; arrayIdx < arrays.Length && currentCount < totalCount; arrayIdx++)
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

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public static T LimitTo<T>(this T value, T maxValue, IComparer<T> comparer)
		//	=> comparer.Compare(value, maxValue) > 0 ? maxValue : value;

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long LongIndexOf<T>(this RecyclableLongList<T> arrays, long itemsCount, T itemToFind, IEqualityComparer<T> comparer)
		{
			for (var itemIdx = 0L; itemIdx < itemsCount; itemIdx++)
			{
				var item = arrays[itemIdx];
				if (comparer.Equals(itemToFind, item))
				{
					return itemIdx;
				}

				itemIdx++;
			}

			return -1;
		}

		public static long LongIndexOf<T>(this T[][] arrays, long itemsCount, T itemToFind, IEqualityComparer<T> comparer)
		{
			var arraysSpan = new Span<T[]>(arrays);
			var arraysCount = arraysSpan.Length;
			for (int arrayIdx = 0, scannedCount = 0; arrayIdx < arraysCount; arrayIdx++)
			{
				var itemsSpan = new Span<T>(arraysSpan[arrayIdx]);
				var arrayItemsCount = itemsSpan.Length;
				for (var itemIdx = 0; itemIdx < arrayItemsCount; itemIdx++)
				{
					if (comparer.Equals(itemToFind, itemsSpan[itemIdx]))
					{
						return scannedCount;
					}

					scannedCount++;
					if (scannedCount == itemsCount)
					{
						return -1;
					}
				}
			}

			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this IList<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this List<T> values, int blockSize = RecyclableDefaults.BlockSize) => new(values, blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this IEnumerable<T> values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this T[] values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this List<T> values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableList<T> ToRecyclableList<T>(this RecyclableList<T> values) => new(values);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RecyclableLongList<T> ToRecyclableLongList<T>(this IEnumerable<T> values, int minBlockSize = RecyclableDefaults.BlockSize) => new(values, minBlockSize: minBlockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] RentArrayFromPool<T>(this int minSize, ArrayPool<T> arrayPool) => (minSize >= _minPooledArraySize)
			? arrayPool.Rent(minSize)
			: new T[minSize];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] RentArrayFromPool<T>(this long minSize, ArrayPool<T> arrayPool) => (minSize is >= _minPooledArraySize and <= int.MaxValue)
			? arrayPool.Rent((int)minSize)
			: new T[minSize];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReturnToPool<T>(this T[] array, ArrayPool<T> arrayPool, bool needsClearing)
		{
			if (array.LongLength is > _minPooledArraySize and <= int.MaxValue)
			{
				arrayPool.Return(array, needsClearing);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long LimitTo(this long value, int limit)
			=> (value <= limit) ? value : limit;

		public static int IndexOf<T>(this T[] memory, int itemCount, T? itemToFind, IEqualityComparer<T> equalityComparer)
		{
			for (var itemIdx = 0; itemIdx < itemCount; itemIdx++)
			{
				var item = memory[itemIdx];
				if (equalityComparer.Equals(item, itemToFind))
				{
					return itemIdx;
				}
			}

			return -1;
		}
	}
}
