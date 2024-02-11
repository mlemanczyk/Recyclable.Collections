using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
#pragma warning disable IDE1006 // This is intentional naming for the class to show up at the end of IntelliSense list.
	public static class zRecyclableLongListInsertRange
#pragma warning restore IDE1006
	{
		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableLongList<T> list, long startingItemIndex, T[] items)
		{
			if (items.Length == 0)
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = (int)(startingItemIndex & list._blockSizeMinus1),
				sourceBlockIndex = (int)(startingItemIndex >> list._blockSizePow2BitShift);

			long newCapacity = list._longCount + items.LongLength;
			list._capacity = (int)BitOperations.RoundUpToPowerOf2((ulong)newCapacity);

			// This is "requiredCapacity". We're reusing blockSize var to save on the total no. of variables.
			int blockSize = (int)(list._capacity >> list._blockSizePow2BitShift) + 1;
			T[][] newMemoryBlocks = blockSize >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T[]>.RentShared(blockSize) : new T[blockSize][];

			blockSize = list._blockSize;

			// Copy references to memory block arrays which stay as-is
			Array.Copy(oldMemoryBlocks, newMemoryBlocks, sourceBlockIndex);

			// Prepare the first block for items in the new memory blocks
			bool usePool = blockSize >= RecyclableDefaults.MinPooledArrayLength;
			Span<T[]> newMemoryBlocksSpan = new(newMemoryBlocks);

			int newBlockIndex = sourceBlockIndex;// + (newItemIndex + 1 == 0 ? 1 : 0);
			newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];

			// Copy the starting items from the list in the current block, but to a new block
			long copiedCount;
			if (newItemIndex > 0)
			{
				Array.Copy(oldMemoryBlocksSpan[sourceBlockIndex], newMemoryBlocks[newBlockIndex], newItemIndex);
				copiedCount = newItemIndex;
			}
			else
			{
				copiedCount = 0;
			}

			// Add new items to the list
			ReadOnlySpan<T> itemsSpan = items;
			Span<T> newBlockArraySpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);

			//sourceItemIndex = 0;
			if (itemsSpan.Length >= newBlockArraySpan.Length)
			{
				newItemIndex = 0;
				if (usePool)
				{
					do
					{
						itemsSpan[..newBlockArraySpan.Length].CopyTo(newBlockArraySpan);
						itemsSpan = itemsSpan[newBlockArraySpan.Length..];
						newBlockIndex++;
						if (itemsSpan.Length == 0)
						{
							break;
						}

						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = RecyclableArrayPool<T>.RentShared(blockSize);
					} while (itemsSpan.Length >= blockSize);
				}
				else
				{
					do
					{
						itemsSpan[..newBlockArraySpan.Length].CopyTo(newBlockArraySpan);
						itemsSpan = itemsSpan[newBlockArraySpan.Length..];
						newBlockIndex++;
						if (itemsSpan.Length == 0)
						{
							break;
						}

						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = new T[blockSize];
					} while (itemsSpan.Length >= blockSize) ;
				}
			}

			// Copy the rest of the new items to the current block, if any
			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(newBlockArraySpan);
				newItemIndex += itemsSpan.Length;
			}
			else
			{
				newItemIndex = 0;
				newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
			}

			long itemsCount = list._longCount - startingItemIndex;
			if (itemsCount > 0)
			{
				// Copy the rest of items from the list after the new items
				int toCopy;
				itemsSpan = new(oldMemoryBlocksSpan[sourceBlockIndex], (int)copiedCount, (int)(blockSize - copiedCount));
				copiedCount = 0;
				newBlockArraySpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);

				while (copiedCount < itemsCount)
				{
					toCopy = (int)Math.Min(itemsCount - copiedCount, itemsSpan.Length);
					if (newBlockArraySpan.Length < toCopy)
					{
						toCopy = newBlockArraySpan.Length;
						itemsSpan[..toCopy].CopyTo(newBlockArraySpan);
						newBlockIndex++;
						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						itemsSpan = itemsSpan[toCopy..];
						copiedCount += toCopy;
					}
					else
					{
						itemsSpan[..toCopy].CopyTo(newBlockArraySpan);
						newBlockArraySpan = newBlockArraySpan[toCopy..];
						itemsSpan = itemsSpan[toCopy..];
						if (itemsSpan.Length == 0 && sourceBlockIndex + 1 < oldMemoryBlocksSpan.Length)
						{
							sourceBlockIndex++;
							itemsSpan = oldMemoryBlocksSpan[sourceBlockIndex];
						}

						copiedCount += toCopy;
						if (newBlockArraySpan.Length == 0 && copiedCount < itemsCount)
						{
							newBlockIndex++;
							newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						}
					}
				}
			}

			list._lastBlockWithData = newBlockIndex;
			if ((newCapacity & list._blockSizeMinus1) == 0)
			{
				list._nextItemIndex = 0;
				list._nextItemBlockIndex = newBlockIndex + 1;
			}
			else
			{
				list._nextItemIndex = (int)(newCapacity & list._blockSizeMinus1);
				list._nextItemBlockIndex = newBlockIndex;
			}

			list._memoryBlocks = newMemoryBlocks;
			list._longCount = newCapacity;
			list._version++;

			// Return unused memory block arrays to the pool
			if (usePool)
			{
				bool needsClearing = RecyclableLongList<T>._needsClearing;
				while (sourceBlockIndex < oldMemoryBlocksSpan.Length)
				{
					RecyclableArrayPool<T>.ReturnShared(oldMemoryBlocksSpan[sourceBlockIndex++], needsClearing);
				}
			}

			// Allocate unused memory block arrays up to the capacity
			newBlockIndex++;
			if (usePool)
			{
				while (newBlockIndex < newMemoryBlocksSpan.Length)
				{
					newMemoryBlocksSpan[newBlockIndex++] = RecyclableArrayPool<T>.RentShared(blockSize);
				}
			}
			else
			{
				while (newBlockIndex < newMemoryBlocksSpan.Length)
				{
					newMemoryBlocksSpan[newBlockIndex++] = new T[blockSize];
				}
			}

			if (oldMemoryBlocksSpan.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				RecyclableArrayPool<T[]>.ReturnShared(oldMemoryBlocks, true);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableLongList<T> list, long startingItemIndex, ICollection items)
		{
		}
	}
}
