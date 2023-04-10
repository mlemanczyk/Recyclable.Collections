using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		//[Benchmark]
		public void RecyclableList_Create()
		{
			using var list = new RecyclableList<object>();
			DoNothing(list);
		}

		//[Benchmark]
		public void RecyclableList_Create_WithCapacity()
		{
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			DoNothing(list);
		}

		//[Benchmark]
		public void RecyclableList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<object>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		//[Benchmark]
		public void RecyclableList_Add_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: dataCount);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		//[Benchmark]
		public void RecyclableList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<object>)TestObjectsAsRecyclableList;
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableList_AddRangeWhenSourceIsRecyclableArrayList()
		{
			var data = TestObjectsAsRecyclableArrayList;
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<object>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}

		//[Benchmark]
		public void RecyclableList_GetItem()
		{
			var data = _testRecyclableList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		//[Benchmark]
		public void RecyclableList_GetItem_AsSpan()
		{
			var data = _testRecyclableList;
			var dataCount = TestObjectCount;
			int blockSize = data.BlockSize;
			int nextItemBlockIndex = data.NextItemBlockIndex;
			
			var memoryBlocks = new Span<object[]>(data.MemoryBlocks, 0, data.LastTakenBlockIndex + 1);
			for (var blockIndex = 0; blockIndex < nextItemBlockIndex; blockIndex++)
			{
				var memoryBlock = new Span<object>(memoryBlocks[blockIndex], 0, blockSize);
				for (var itemIndex = 0; itemIndex < blockSize; itemIndex++)
				{
					DoNothing(memoryBlock[itemIndex]);
				}
			}

			if (nextItemBlockIndex == data.LastTakenBlockIndex)
			{

				var memoryBlock = new Span<object>(memoryBlocks[nextItemBlockIndex], 0, data.NextItemIndex);
				for (var itemIndex = 0; itemIndex < memoryBlock.Length; itemIndex++)
				{
					DoNothing(memoryBlock[itemIndex]);
				}
			}
		}

		//[Benchmark]
		public void RecyclableList_GetItem_AsArray()
		{
			var data = _testRecyclableList;
			var dataCount = TestObjectCount;
			int blockSize = data.BlockSize;
			int nextItemBlockIndex = data.NextItemBlockIndex;

			var memoryBlocks = data.MemoryBlocks;
			for (var blockIndex = 0; blockIndex < nextItemBlockIndex; blockIndex++)
			{
				var memoryBlock = memoryBlocks[blockIndex];
				for (var itemIndex = 0; itemIndex < blockSize; itemIndex++)
				{
					DoNothing(memoryBlock[itemIndex]);
				}
			}

			if (nextItemBlockIndex == data.LastTakenBlockIndex)
			{
				var memoryBlock = memoryBlocks[nextItemBlockIndex];
				blockSize = data.NextItemIndex;
				for (var itemIndex = 0; itemIndex < blockSize; itemIndex++)
				{
					DoNothing(memoryBlock[itemIndex]);
				}
			}
		}

		//[Benchmark]
		public void RecyclableList_GetItem_AsForEach()
		{
			var data = _testRecyclableList;
			var dataCount = TestObjectCount;

			foreach (var item in data)
			{
				DoNothing(item);
			}
		}

		//[Benchmark]
		public void RecyclableList_GetItem_AsWhile()
		{
			var data = _testRecyclableList;
			long dataCount = TestObjectCount;
			if (dataCount == 0)
			{
				return;
			}

			int blockSize = data.BlockSize;
			int nextItemBlockIndex = data.NextItemBlockIndex;
			int itemIndex = 0;
			int blockIndex = 0;
			var memoryBlocks = data.MemoryBlocks;
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

			if (nextItemBlockIndex == data.LastTakenBlockIndex)
			{
				memoryBlock = memoryBlocks[nextItemBlockIndex];
				blockSize = data.NextItemIndex;
				while (itemIndex < blockSize)
				{
					DoNothing(memoryBlock[itemIndex++]);
				}
			}
		}

		//[Benchmark]
		public void RecyclableList_SetItem()
		{
			var data = _testRecyclableList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		//[Benchmark]
		public void RecyclableList_Count()
		{
			var data = _testRecyclableList;
			DoNothing(data.Count);
		}

		//[Benchmark]
		public void RecyclableList_LongCount()
		{
			var data = _testRecyclableList;
			DoNothing(data.LongCount);
		}

		//[Benchmark]
		public void RecyclableList_Contains_FirstItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Contains(data[i]));
			}
		}

		//[Benchmark]
		public void RecyclableList_Contains_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Contains(data[^(i + 1)]));
			}
		}

		//[Benchmark]
		public void RecyclableList_IndexOf_FirstItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
			}
		}

		//[Benchmark]
		public void RecyclableList_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		//[Benchmark]
		public void RecyclableList_LongIndexOf_FirstItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.LongIndexOf(data[i]));
			}
		}

		//[Benchmark]
		public void RecyclableList_LongIndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.LongIndexOf(data[^(i + 1)]));
			}
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_Remove_FirstItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<object>(data, minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_Remove_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<object>(data, minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_RemoveAt_FirstItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<object>(data, minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				list.RemoveAt(i);
			}
		}

		[Benchmark(Baseline = false)]
		public void RecyclableList_RemoveAt_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<object>(data, minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				list.RemoveAt(i);
			}
		}
	}
}
