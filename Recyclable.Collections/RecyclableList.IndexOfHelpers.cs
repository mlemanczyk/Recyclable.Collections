using System.Runtime.CompilerServices;
using Recyclable.Collections.Parallel;

namespace Recyclable.Collections
{
	public partial class RecyclableList<T>
	{
		public static class IndexOfHelpers
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			// [MethodImpl(MethodImplOptions.AggressiveInlining)]
			// [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
			private static void ScheduleIndexOfTask(IndexOfSynchronizationContext context, RecyclableList<T> list, int blockIndex, T itemToFind)
			{
				_ = context.AllDoneSignal.AddParticipant();
				// & WAS SLOWER
				// _ = ThreadPool.QueueUserWorkItem<object?>((_) =>
				_ = Task.Factory.StartNew(() =>
				{
					try
					{
						if (context._isItemFound)
						{
							return;
						}

						long index = Array.IndexOf(list._memoryBlocks[blockIndex], itemToFind, 0, list._blockSize);
						if (context._isItemFound)
						{
							return;
						}
						else if (index >= 0)
						{
							context._isItemFound = true;
							_ = Interlocked.Exchange(ref context.FoundItemIndex, (blockIndex << list._blockSizePow2BitShift) + index);
							return;
						}

						index = Array.IndexOf(list._memoryBlocks[blockIndex + 1], itemToFind, 0, list._blockSize);
						if (context._isItemFound)
						{
							return;
						}
						else if (index >= 0)
						{
							context._isItemFound = true;
							_ = Interlocked.Exchange(ref context.FoundItemIndex, ((blockIndex + 1) << list._blockSizePow2BitShift) + index);
							return;
						}

						index = Array.IndexOf(list._memoryBlocks[blockIndex + 2], itemToFind, 0, list._blockSize);
						if (context._isItemFound)
						{
							return;
						}
						else if (index >= 0)
						{
							context._isItemFound = true;
							_ = Interlocked.Exchange(ref context.FoundItemIndex, ((blockIndex + 2) << list._blockSizePow2BitShift) + index);
							return;
						}

						index = Array.IndexOf(list._memoryBlocks[blockIndex + 3], itemToFind, 0, list._blockSize);
						if (!context._isItemFound && index >= 0)
						{
							context._isItemFound = true;
							_ = Interlocked.Exchange(ref context.FoundItemIndex, ((blockIndex + 3) << list._blockSizePow2BitShift) + index);
						}

						//itemsToSearch -= blockSize;
						//if (itemsToSearch > 0)
						//{
						//	if (itemsToSearch < blockSize)
						//	{
						//		blockSize = (int)itemsToSearch;
						//	}

						//	index = Array.IndexOf(list._memoryBlocks[blockIndex + 2], itemToFind, 0, blockSize);
						//	if (index >= 0)
						//	{
						//		if (!context.ItemFoundSignal.IsSet)
						//		{
						//			context.ItemFoundSignal.Set();
						//			_ = Interlocked.Exchange(ref context.FoundItemIndex, ((blockIndex + 2) << list._blockSizePow2BitShift) + index);
						//		}

						//		return;
						//	}
						//}

						//itemsToSearch -= blockSize;
						//if (itemsToSearch > 0)
						//{
						//	if (itemsToSearch < blockSize)
						//	{
						//		blockSize = (int)itemsToSearch;
						//	}

						//	index = Array.IndexOf(list._memoryBlocks[blockIndex + 3], itemToFind, 0, blockSize);
						//	if (!context.ItemFoundSignal.IsSet && index >= 0)
						//	{
						//		context.ItemFoundSignal.Set();
						//		_ = Interlocked.Exchange(ref context.FoundItemIndex, ((blockIndex + 3) << list._blockSizePow2BitShift) + index);

						//		return;
						//	}
						//}
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
						context.AllDoneSignal.RemoveParticipant();
					}
				});
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			// [MethodImpl(MethodImplOptions.AggressiveInlining)]
			// [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
			private static void ScheduleIndexOfTaskSingle(IndexOfSynchronizationContext context, RecyclableList<T> list, int blockIndex, T itemToFind)
			{
				_ = context.AllDoneSignal.AddParticipant();
				// & WAS SLOWER
				// _ = ThreadPool.QueueUserWorkItem<object?>((_) =>
				_ = Task.Factory.StartNew(() =>
				{
					try
					{
						if (context._isItemFound)
						{
							return;
						}

						long index = Array.IndexOf(list._memoryBlocks[blockIndex], itemToFind, 0, list._blockSize);
						if (context._isItemFound)
						{
							return;
						}
						else if (index >= 0)
						{
							context._isItemFound = true;
							_ = Interlocked.Exchange(ref context.FoundItemIndex, (blockIndex << list._blockSizePow2BitShift) + index);
						}
					}
					catch (Exception e)
					{
						_ = Interlocked.Exchange(ref context.Exception, e);
					}
					finally
					{
						context.AllDoneSignal.RemoveParticipant();
					}
				});
			}

			private const int BlocksForEachTask = 4;

			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
			public static long IndexOfParallel(RecyclableList<T> list, in T item)
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

				// TODO: Switch to use _blockSize instead of list._longCountIndeexOfStep for simplifications & better performance.

				// & WAS SLOWER without
				int blockIndex = 2;
				// & WAS SLOWER
				//long itemsCount = list._longCount - list._blockSize;
				// & WAS SLOWER
				// int blockSize = _blockSize;
				// & WAS SLOWER
				// while ((blockIndex << list._blockSizePow2BitShift) + itemIndex > step)
				//long blockSize = list._blockSize << 1;
				int lastBlockWithData = list._lastBlockWithData - BlocksForEachTask;
				while (blockIndex < lastBlockWithData)
				{
					// At this point itemIndex is limited to blockSize range - int.
					ScheduleIndexOfTask(context, list, blockIndex, item);
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
						ScheduleIndexOfTaskSingle(context, list, blockIndex, item);
						if (context._isItemFound)
						{
							break;
						}

						blockIndex++;
					}
				}

				int itemIndex = Array.IndexOf(list._memoryBlocks[list._lastBlockWithData], item, 0, list._nextItemIndex > 0 ? list._nextItemIndex : list._blockSize);
				if (itemIndex >= 0)
				{
					context._isItemFound = true;
					_ = Interlocked.Exchange(ref context.FoundItemIndex, ((long)blockIndex << list._blockSizePow2BitShift) + itemIndex);
				}

				context.AllDoneSignal.SignalAndWait();
				if (context.Exception == null)
				{
					return context.FoundItemIndex;
				}

				context.Exception.CaptureAndRethrow();
				return RecyclableDefaults.ItemNotFoundIndexLong;
			}

			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
			public static long IndexOfSequential(RecyclableList<T> list, in T item) //, in T[][] memoryBlocks, int lastBlockIndex, int blockSize, int nextItemIndex)
			{
				int itemIndex;
				// & WAS SLOWER
				//int lastBlockWithData = list._lastBlockWithData;
				Span<T[]> memoryBlocksSpan = new(list._memoryBlocks, 0, list._lastBlockWithData + 1);
				int blockIndex;
				for (blockIndex = 1; blockIndex < list._lastBlockWithData; blockIndex += 4)
				{
					itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex], item, 0, list._blockSize);
					if (itemIndex >= 0)
					{
						return itemIndex + ((long)blockIndex << list._blockSizePow2BitShift);
					}

					if (blockIndex + 1 <  list._lastBlockWithData)
					{
						itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex + 1], item, 0, list._blockSize);
						if (itemIndex >= 0)
						{
							return itemIndex + ((long)(blockIndex + 1)<< list._blockSizePow2BitShift);
						}

						if (blockIndex + 2 <  list._lastBlockWithData)
						{
							itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex + 2], item, 0, list._blockSize);
							if (itemIndex >= 0)
							{
								return itemIndex + ((long)(blockIndex + 2)<< list._blockSizePow2BitShift);
							}

							if (blockIndex + 3 <  list._lastBlockWithData)
							{
								itemIndex = Array.IndexOf(memoryBlocksSpan[blockIndex + 3], item, 0, list._blockSize);
								if (itemIndex >= 0)
								{
									return itemIndex + ((long)(blockIndex + 3)<< list._blockSizePow2BitShift);
								}
							}
						}
					}
				}

				//if (blockIndex == list._lastBlockWithData)
				//{
					itemIndex = list._nextItemIndex != 0
						? Array.IndexOf(memoryBlocksSpan[list._lastBlockWithData], item, 0, list._nextItemIndex)
						: Array.IndexOf(memoryBlocksSpan[list._lastBlockWithData], item, 0, list._blockSize);
				//}
				//else
				//{
				//	return RecyclableDefaults.ItemNotFoundIndexLong;
				//}

				return itemIndex >= 0 ? itemIndex + ((long)list._lastBlockWithData << list._blockSizePow2BitShift) : RecyclableDefaults.ItemNotFoundIndexLong;
			}

			public static int Min(int a, int b, int c) => a < b ? a < c ? a : c : b < c ? b : c;
		}
	}
}
