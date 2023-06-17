namespace Recyclable.Collections
{
	public partial class RecyclableLongList<T>
	{
		internal class RecyclableLongListHelpers
		{
			public static void CopyTo(T[][] sourceMemoryBlocks, long startingIndex, int blockSize, long itemsCount, T[] destinationArray, int destinationArrayIndex)
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

			public static void CopyTo(T[][] sourceMemoryBlocks, long startingIndex, int blockSize, long itemsCount, Array destinationArray, int destinationArrayIndex)
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

			public static IEnumerable<T> Enumerate(T[][] arrays, int chunkSize, long totalCount)
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
		}
	}
}
