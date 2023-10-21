namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_GetItem()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		public void List_GetItem()
		{
			var data = TestObjectsAsList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		public void PooledList_GetItem()
		{
			var data = TestObjectsAsPooledList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		public void RecyclableList_GetItem()
		{
			var data = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		public void RecyclableLongList_GetItem()
		{
			var data = TestObjectsAsRecyclableLongList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		public void RecyclableLongList_GetItem_AsSpan()
		{
			var data = TestObjectsAsRecyclableLongList;
			int blockSize = data.BlockSize;
			int nextItemBlockIndex = data.NextItemBlockIndex;

			var memoryBlocks = new Span<int[]>(data.AsArray, 0, data.LastBlockWithData + 1);
			for (var blockIndex = 0; blockIndex < nextItemBlockIndex; blockIndex++)
			{
				var memoryBlock = new Span<int>(memoryBlocks[blockIndex], 0, blockSize);
				for (var itemIndex = 0; itemIndex < blockSize; itemIndex++)
				{
					DoNothing(memoryBlock[itemIndex]);
				}
			}

			if (nextItemBlockIndex == data.LastBlockWithData)
			{
				var memoryBlock = new Span<int>(memoryBlocks[nextItemBlockIndex], 0, data.NextItemIndex);
				for (var itemIndex = 0; itemIndex < memoryBlock.Length; itemIndex++)
				{
					DoNothing(memoryBlock[itemIndex]);
				}
			}
		}

		public void RecyclableLongList_GetItem_AsArray()
		{
			var data = TestObjectsAsRecyclableLongList;
			int blockSize = data.BlockSize;
			int nextItemBlockIndex = data.NextItemBlockIndex;

			var memoryBlocks = data.AsArray;
			for (var blockIndex = 0; blockIndex < nextItemBlockIndex; blockIndex++)
			{
				var memoryBlock = memoryBlocks[blockIndex];
				for (var itemIndex = 0; itemIndex < blockSize; itemIndex++)
				{
					DoNothing(memoryBlock[itemIndex]);
				}
			}

			if (nextItemBlockIndex == data.LastBlockWithData)
			{
				var memoryBlock = memoryBlocks[nextItemBlockIndex];
				blockSize = data.NextItemIndex;
				for (var itemIndex = 0; itemIndex < blockSize; itemIndex++)
				{
					DoNothing(memoryBlock[itemIndex]);
				}
			}
		}

		public void RecyclableLongList_GetItem_AsForEach()
		{
			var data = TestObjectsAsRecyclableLongList;
			foreach (var item in data)
			{
				DoNothing(item);
			}
		}

		public void RecyclableLongList_GetItem_AsWhile()
		{
			var data = TestObjectsAsRecyclableLongList;
			int dataCount = TestObjectCount;
			if (dataCount == 0)
			{
				return;
			}

			int blockSize = data.BlockSize;
			int nextItemBlockIndex = data.NextItemBlockIndex;
			int itemIndex = 0;
			int blockIndex = 0;
			var memoryBlocks = data.AsArray;
			var memoryBlock = memoryBlocks[0];
			while (blockIndex < nextItemBlockIndex)
			{
				DoNothing(memoryBlock[itemIndex]);
				if (itemIndex + 1 < blockSize)
				{
					itemIndex++;
				}
				else
				{
					blockIndex++;
					memoryBlock = memoryBlocks[blockIndex];
					itemIndex = 0;
				}
			}

			if (nextItemBlockIndex == data.LastBlockWithData)
			{
				memoryBlock = memoryBlocks[nextItemBlockIndex];
				blockSize = data.NextItemIndex;
				while (itemIndex < blockSize)
				{
					DoNothing(memoryBlock[itemIndex++]);
				}
			}
		}
	}
}
