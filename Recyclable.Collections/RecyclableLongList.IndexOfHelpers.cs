// #define MAX_BLOCKS_FOR_TASK_2
// #define MAX_BLOCKS_FOR_TASK_3
#define MAX_BLOCKS_FOR_TASK_4
// #define MAX_BLOCKS_FOR_TASK_5
// #define MAX_BLOCKS_FOR_TASK_6
// #define MAX_BLOCKS_FOR_TASK_7
// #define MAX_BLOCKS_FOR_TASK_8

using System.Runtime.CompilerServices;
using Recyclable.Collections.Parallel;

namespace Recyclable.Collections
{
	public partial class RecyclableLongList<T>
	{
		public static class IndexOfHelpers
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			private static void IndexOfInConsecutiveBlocks(IndexOfSynchronizationContext context, RecyclableLongList<T> list, int blockIndex, T itemToFind)
			{
				try
				{
					if (context._isItemFound && blockIndex >= context.FoundBlockIndex)
					{
						return;
					}

					long index = Array.IndexOf(list._memoryBlocks[blockIndex], itemToFind, 0, list._blockSize);
					if (index >= 0 && (!context._isItemFound || index < context.FoundItemIndex))
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex, ((long)blockIndex << list._blockSizePow2BitShift) + index);
						return;
					}
					else if (context._isItemFound)
					{
						return;
					}

#if MAX_BLOCKS_FOR_TASK_2 || MAX_BLOCKS_FOR_TASK_3 || MAX_BLOCKS_FOR_TASK_4 || MAX_BLOCKS_FOR_TASK_5 || MAX_BLOCKS_FOR_TASK_6 || MAX_BLOCKS_FOR_TASK_7 || MAX_BLOCKS_FOR_TASK_8

					index = Array.IndexOf(list._memoryBlocks[blockIndex + 1], itemToFind, 0, list._blockSize);
					if (index >= 0)
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex + 1, ((long)(blockIndex + 1) << list._blockSizePow2BitShift) + index);
						return;
					}
					else if (context._isItemFound)
					{
						return;
					}

#endif

#if MAX_BLOCKS_FOR_TASK_3 || MAX_BLOCKS_FOR_TASK_4 || MAX_BLOCKS_FOR_TASK_5 || MAX_BLOCKS_FOR_TASK_6 || MAX_BLOCKS_FOR_TASK_7 || MAX_BLOCKS_FOR_TASK_8

					index = Array.IndexOf(list._memoryBlocks[blockIndex + 2], itemToFind, 0, list._blockSize);
					if (index >= 0 && (!context._isItemFound || index < context.FoundItemIndex))
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex + 2, ((long)(blockIndex + 2) << list._blockSizePow2BitShift) + index);
						return;
					}
					else if (context._isItemFound)
					{
						return;
					}

#endif

#if MAX_BLOCKS_FOR_TASK_4 || MAX_BLOCKS_FOR_TASK_5 || MAX_BLOCKS_FOR_TASK_6 || MAX_BLOCKS_FOR_TASK_7 || MAX_BLOCKS_FOR_TASK_8

					index = Array.IndexOf(list._memoryBlocks[blockIndex + 3], itemToFind, 0, list._blockSize);
					if (!context._isItemFound && index >= 0)
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex + 3, ((long)(blockIndex + 3) << list._blockSizePow2BitShift) + index);
					}

#endif

#if MAX_BLOCKS_FOR_TASK_5 || MAX_BLOCKS_FOR_TASK_6 || MAX_BLOCKS_FOR_TASK_7 || MAX_BLOCKS_FOR_TASK_8

					index = Array.IndexOf(list._memoryBlocks[blockIndex + 4], itemToFind, 0, list._blockSize);
					if (!context._isItemFound && index >= 0 && (!context._isItemFound || index < context.FoundItemIndex))
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex + 4, ((long)(blockIndex + 4) << list._blockSizePow2BitShift) + index);
					}
					
#endif

#if MAX_BLOCKS_FOR_TASK_6 || MAX_BLOCKS_FOR_TASK_7 || MAX_BLOCKS_FOR_TASK_8

					index = Array.IndexOf(list._memoryBlocks[blockIndex + 5], itemToFind, 0, list._blockSize);
					if (!context._isItemFound && index >= 0 && (!context._isItemFound || index < context.FoundItemIndex))
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex + 5, ((long)(blockIndex + 5) << list._blockSizePow2BitShift) + index);
					}

#endif

#if MAX_BLOCKS_FOR_TASK_7 || MAX_BLOCKS_FOR_TASK_8

					index = Array.IndexOf(list._memoryBlocks[blockIndex + 6], itemToFind, 0, list._blockSize);
					if (!context._isItemFound && index >= 0 && (!context._isItemFound || index < context.FoundItemIndex))
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex + 6, ((long)(blockIndex + 6) << list._blockSizePow2BitShift) + index);
					}

#endif

#if MAX_BLOCKS_FOR_TASK_8

					index = Array.IndexOf(list._memoryBlocks[blockIndex + 7], itemToFind, 0, list._blockSize);
					if (!context._isItemFound && index >= 0 && (!context._isItemFound || index < context.FoundItemIndex))
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex + 7, ((long)(blockIndex + 7) << list._blockSizePow2BitShift) + index);
					}

#endif
				}
				catch (Exception e)
				{
					e.Data.Add("list", list);
					e.Data.Add("itemToFind", itemToFind);
					e.Data.Add("blockIndex", blockIndex);
					_ = Interlocked.Exchange(ref context.Exception, e);
				}
				finally
				{
					context.RemoveParticipant();
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void ExchangeFoundItemIndexIfLower(IndexOfSynchronizationContext context, int blockIndex, long newItemIndex)
			{
				context.Lock();

				if (newItemIndex < context.FoundItemIndex || context.FoundItemIndex == RecyclableDefaults.ItemNotFoundIndexLong)
				{
					context.FoundBlockIndex = blockIndex;
					context.FoundItemIndex = newItemIndex;
				}

				context._isItemFound = true;
				context.Unlock();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			private static void IndexOfIn1Block(IndexOfSynchronizationContext context, RecyclableLongList<T> list, int blockIndex, T itemToFind)
			{
				try
				{
					if (context._isItemFound)
					{
						return;
					}

					long index = Array.IndexOf(list._memoryBlocks[blockIndex], itemToFind, 0, list._blockSize);
					if (!context._isItemFound && index >= 0)
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex, ((long)blockIndex << list._blockSizePow2BitShift) + index);
					}
				}
				catch (Exception e)
				{
					_ = Interlocked.Exchange(ref context.Exception, e);
				}
				finally
				{
					context.RemoveParticipant();
				}
			}

#if MAX_BLOCKS_FOR_TASK_2
			private const int BlocksForEachTask = 2;
#endif
#if MAX_BLOCKS_FOR_TASK_3
			private const int BlocksForEachTask = 3;
#endif
#if MAX_BLOCKS_FOR_TASK_4
			private const int BlocksForEachTask = 4;
#endif
#if MAX_BLOCKS_FOR_TASK_5
			private const int BlocksForEachTask = 5;
#endif
#if MAX_BLOCKS_FOR_TASK_6
			private const int BlocksForEachTask = 6;
#endif
#if MAX_BLOCKS_FOR_TASK_7
			private const int BlocksForEachTask = 7;
#endif
#if MAX_BLOCKS_FOR_TASK_8
			private const int BlocksForEachTask = 8;
#endif

			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
			public static long IndexOfParallel(RecyclableLongList<T> list,  T item)
			{
				// & WAS SLOWER
				//long itemsCount = Array.IndexOf(list._memoryBlocks[0], item, 0, (int)Math.Min(list._blockSize, list._longCount));
				//if (itemsCount >= 0)
				//{
				//	return itemsCount;
				//}

				//using var context = new ParallelSynchronizationContext(1);
				IndexOfSynchronizationContext context = IndexOfSynchronizationContextPool.GetWithOneParticipant();
				// (long)(_longCount * 0.329) => most efficient step, based on benchmarks
				//Iterate(this, context, item);

				// & WAS SLOWER without
				int blockIndex = 2;
				// & WAS SLOWER
				//long itemsCount = list._longCount - list._blockSize;
				// & WAS SLOWER
				// int blockSize = _blockSize;
				// & WAS SLOWER
				// while (((long)blockIndex << list._blockSizePow2BitShift) + itemIndex > step)
				//long blockSize = checked((long)list._blockSize << 1);
				int lastBlockWithData = list._lastBlockWithData - BlocksForEachTask;
				while (blockIndex < lastBlockWithData)
				{
					// At this point itemIndex is limited to blockSize range - int.

					// & WAS SLOWER
					// _ = ThreadPool.QueueUserWorkItem<object?>((_) =>
					// Task.Factory.StartNew(() =>
					context.AddParticipant();
					var startingBlock = blockIndex;
					_ = Task.Run(() => IndexOfInConsecutiveBlocks(context, list, startingBlock, item));

					// & WAS SLOWER
					// ScheduleIndexOfTask(context, list, blockIndex, item);
					if (context._isItemFound)
					{
						break;
					}

					blockIndex += BlocksForEachTask;
				}

				if (!context._isItemFound)
				{
					while (blockIndex < list._lastBlockWithData)
					{
						// & WAS SLOWER
						// _ = ThreadPool.QueueUserWorkItem<object?>((_) =>
						// Task.Factory.StartNew(() =>
						var startingBlock = blockIndex;
						context.AddParticipant();
						_ = Task.Run(() => IndexOfIn1Block(context, list, startingBlock, item));

						// & WAS SLOWER
						// ScheduleIndexOfTaskSingle(context, list, blockIndex, item);
						if (context._isItemFound)
						{
							break;
						}

						blockIndex++;
					}
				}

				if (!context._isItemFound)
				{
					int itemIndex = Array.IndexOf(list._memoryBlocks[list._lastBlockWithData], item, 0, list._nextItemIndex > 0 ? list._nextItemIndex : list._blockSize);
					if (!context._isItemFound && itemIndex >= 0)
					{
						ExchangeFoundItemIndexIfLower(context, blockIndex, ((long)blockIndex << list._blockSizePow2BitShift) + itemIndex);
					}
				}

				context.SignalAndWait();
				if (context.Exception == null)
				{
					try
					{
						return context.FoundItemIndex;
					}
					finally
					{
						context.Dispose();
					}
				}

				try
				{
					context.Exception.CaptureAndReThrow();
				}
				finally
				{
					context.Dispose();
				}

				return RecyclableDefaults.ItemNotFoundIndexLong;
			}

			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
			public static long IndexOfSequential(RecyclableLongList<T> list, in T item) //, in T[][] memoryBlocks, int lastBlockIndex, int blockSize, int nextItemIndex)
			{
				int itemIndex;
				// & WAS SLOWER
				//int lastBlockWithData = list._lastBlockWithData;
				ReadOnlySpan<T[]> memoryBlocksSpan = new(list._memoryBlocks, 0, list._lastBlockWithData + 1);
				int blockIndex;
				for (blockIndex = 1; blockIndex < list._lastBlockWithData; blockIndex += 4)
				{
					itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex], item, 0, list._blockSize);
					if (itemIndex >= 0)
					{
						return itemIndex + ((long)blockIndex << list._blockSizePow2BitShift);
					}

					if (blockIndex + 1 < list._lastBlockWithData)
					{
						itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex + 1], item, 0, list._blockSize);
						if (itemIndex >= 0)
						{
							return itemIndex + ((long)(blockIndex + 1) << list._blockSizePow2BitShift);
						}

						if (blockIndex + 2 < list._lastBlockWithData)
						{
							itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex + 2], item, 0, list._blockSize);
							if (itemIndex >= 0)
							{
								return itemIndex + ((long)(blockIndex + 2) << list._blockSizePow2BitShift);
							}

							if (blockIndex + 3 < list._lastBlockWithData)
							{
								itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex + 3], item, 0, list._blockSize);
								if (itemIndex >= 0)
								{
									return itemIndex + ((long)(blockIndex + 3) << list._blockSizePow2BitShift);
								}
							}
						}
					}
				}

				itemIndex = list._nextItemIndex != 0
					? Array.IndexOf(memoryBlocksSpan[list._lastBlockWithData], item, 0, list._nextItemIndex)
					: Array.IndexOf(memoryBlocksSpan[list._lastBlockWithData], item, 0, list._blockSize);

				return itemIndex >= 0 ? itemIndex + ((long)list._lastBlockWithData << list._blockSizePow2BitShift) : RecyclableDefaults.ItemNotFoundIndexLong;
			}
		}
	}
}
