﻿using BenchmarkDotNet.Attributes;
using MoreLinq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Recyclable.Collections.Benchmarks.POC
{
	[MemoryDiagnoser]
	public class WhenParallelSearchBenchmarks : PocBenchmarkBase
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void DoNothing<T>(T index)
		{
			//Console.WriteLine(index);
		}

		//[Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 32)]
		//public int NumberOfThreads { get; set; } = 3;

		//[Params(10_000, 20_000, 35_000, 50_000, 75_000, 100_000, 125_000, 150_000, 175_000, 200_000, 225_000, 250_000, 275_000, 300_000, 325_000, 350_000, 375_000, 400_000, 500_000)]
		//[Params(320_000, 321_000, 322_000, 323_000, 324_000, 325_000, 326_000, 327_000, 328_000, 329_000, 330_000)]
		//[Params(329_000, 512_000, 750_000, 1_000_000, 3_290_000)]
		//public int Step { get; set; } = 329_000; // This was the fastest so far

		private static readonly int _numberOfProcessors = Environment.ProcessorCount;
		private RecyclableList<long>? _testData;

		public object? ItemToFind { get; set; }

		[GlobalSetup] 
		public void Setup()
		{
			_testData = Enumerable.Range(1, TestObjectCount).Cast<long>().ToRecyclableList(BlockSize);
			ItemToFind = _testData.Last();
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_testData = default;
		}

		
		[Benchmark(Baseline = true)]
		public void IndexOfSequentially()
		{
			long index = DoLongIndexOfSequentially();
			if (index >= 0)
			{
				DoNothing(index);
			}
			else
			{
				throw new Exception("Item not found");
			}
		}

		private long DoLongIndexOfSequentially()
		{
			// This won't work for TestObjectCount == 0
			int index;
			int blockSize = _testData!.BlockSize;
			int lastBlockWithData = _testData.LastBlockWithData;
			var memoryBlocks = _testData.AsArray;
			var itemToFind = ItemToFind;
			int blockIndex = 0;
			while (true)
			{
				index = Array.IndexOf(memoryBlocks[blockIndex], itemToFind, 0, blockSize);
				if (index >= 0)
				{
					return (blockIndex << _testData.BlockSizePow2BitShift) + index;
				}

				if (blockIndex + 1 < lastBlockWithData)
				{
					blockIndex++;
				}
				else
				{
					break;
				}
			}

			index = lastBlockWithData == _testData.NextItemBlockIndex
				? Array.IndexOf(memoryBlocks[lastBlockWithData], itemToFind, 0, _testData.NextItemIndex)
				: Array.IndexOf(memoryBlocks[lastBlockWithData], itemToFind, 0, blockSize);

			return index >= 0 ? (lastBlockWithData << _testData.BlockSizePow2BitShift) + index : -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static long DoLongIndexOfParallel(RecyclableList<long> list, in long itemToFind, ItemRange searchInfo, ManualResetEventSlim itemFoundSignal)
		{
			int index;
			int blockSize = list.BlockSize;
			int blockIndex = searchInfo.BlockIndex;
			long itemsToSearchCount = searchInfo.ItemsToSearchCount;
			long[][] memoryBlocks = list.AsArray;
			index = Array.IndexOf(memoryBlocks[searchInfo.BlockIndex], itemToFind, searchInfo.StartingItemIndex, blockSize - searchInfo.StartingItemIndex);
			if (itemFoundSignal.IsSet)
			{
				return -1;
			}
			else if (index >= 0)
			{
				return (blockIndex << list.BlockSizePow2BitShift) + index;
			}

			itemsToSearchCount -= blockSize - searchInfo.StartingItemIndex;
			if (itemsToSearchCount < 0)
			{
				return -1;
			}

			int lastBlockWithData = blockIndex + (int)(itemsToSearchCount >> list.BlockSizePow2BitShift) + ((itemsToSearchCount & (list.BlockSize - 1)) > 0 ? 1 : 0);
			blockIndex++;
			while (true)
			{
				index = Array.IndexOf(memoryBlocks[blockIndex], itemToFind, 0, blockSize);
				if (itemFoundSignal.IsSet)
				{
					return -1;
				}
				else if (index >= 0)
				{
					return ((long)blockIndex << list.BlockSizePow2BitShift) + index;
				}

				itemsToSearchCount -= blockSize;
				if (blockIndex + 1 < lastBlockWithData && itemsToSearchCount < 0)
				{
					blockIndex++;
				}
				else
				{
					break;
				}
			}

			index = itemsToSearchCount < 0 || itemFoundSignal.IsSet || lastBlockWithData > list.NextItemBlockIndex
				? -1
				: lastBlockWithData == list.NextItemBlockIndex
				? Array.IndexOf(memoryBlocks[lastBlockWithData], itemToFind, 0, checked((int)Math.Min(list.NextItemIndex, itemsToSearchCount)))
				: Array.IndexOf(memoryBlocks[lastBlockWithData], itemToFind, 0, checked((int)Math.Min(blockSize, itemsToSearchCount)));

			return itemFoundSignal.IsSet || index < 0 ? -1 : ((long)lastBlockWithData << list.BlockSizePow2BitShift) + index;
		}

		[Benchmark]
		public void IndexOfParallel()
		{
			var index = DoIndexOfParallel();
			if (index >= 0)
			{
				DoNothing(index);
			}
			else
			{
				throw new Exception("Item not found");
			}
		}

		private long DoIndexOfParallel()
		{
			using var itemFoundSignal = new ManualResetEventSlim(false);
			using var allDoneSignal = new Barrier(1);
			//long step = Step;// TestObjectCount / NumberOfThreads;
			int blockSize = _testData!.BlockSize;
			//object[][] memoryBlocks = _testData.AsArray();
			//long itemsRemaining = testObjectCount;
			var foundItemIndex = -1L;
			var itemToFind = ItemToFind;
			//ItemRangesIterator.Iterate(0, blockSize, BlockSizePow2BitShift, TestObjectCount, (long)(TestObjectCount * 0.329), (searchInfo) =>
			//{
			//	_ = allDoneSignal.AddParticipant();
			//	_ = Task.Factory.StartNew((state) =>
			//	{
			//		try
			//		{
			//			var index = DoLongIndexOfParallel(_testData, itemToFind, searchInfo, itemFoundSignal);
			//			if (index >= 0)
			//			{
			//				itemFoundSignal.Set();
			//				lock (itemFoundSignal)
			//				{
			//					foundItemIndex = index;
			//				}
			//			}
			//		}
			//		finally
			//		{
			//			lock (allDoneSignal)
			//			{
			//				allDoneSignal.RemoveParticipant();
			//			}
			//		}
			//	}, state: searchInfo);

			//	return !itemFoundSignal.IsSet;
			//});

			//while (true)
			//{
			//	var toGive = Math.Min(step, itemsRemaining);
			//	SearchInfo<object> searchInfo = new(
			//		blockIndex: blockIndex,
			//		startingItemIndex: itemIndex,
			//		itemsToSearchCount: toGive,
			//		itemToFind: ItemToFind,
			//		itemFoundSignal: itemFoundSignal);

			//	_ = Task.Factory.StartNew((state) =>
			//	{
			//		try
			//		{
			//			var index = DoLongIndexOfParallel(_testData, (SearchInfo<object>)state!);
			//			if (index >= 0)
			//			{
			//				itemFoundSignal.Set();
			//				lock (itemFoundSignal)
			//				{
			//					foundItemIndex = index;
			//				}
			//			}
			//		}
			//		finally
			//		{
			//			lock (allDoneSignal)
			//			{
			//				allDoneSignal.RemoveParticipant();
			//			}
			//		}
			//	}, state: searchInfo);

			//	//itemIndex = checked((int)((itemIndex + toGive) >> BlockSizePow2BitShift)) - 1;
			//	while (toGive > 0)
			//	{
			//		if (itemIndex + toGive < blockSize)
			//		{
			//			itemIndex += (int)toGive;
			//			break;
			//		}
			//		else
			//		{
			//			blockIndex++;
			//			//itemIndex += blockSize;
			//			//if (itemIndex > blockSize)
			//			//{
			//			//	itemIndex 
			//			//}
			//			//if (itemIndex + blockSize < )
			//			//itemIndex = (int)(toGive - blockSize + itemIndex);
			//			toGive -= blockSize;
			//		}

			//		//if (blockIndex == _testData.LastBlockWithData)
			//		//{
			//		//	itemIndex = checked((int)toGive);
			//		//	break;
			//		//}

			//	}


			//	if (threadIndex + 1 < numberOfRanges && blockIndex <= _testData.LastBlockWithData)
			//	{
			//		threadIndex++;
			//	}
			//	else
			//	{
			//		break;
			//	}
			//}

			allDoneSignal.SignalAndWait();
			return foundItemIndex;
		}
	}
}
