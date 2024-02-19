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
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, Array items)
		{
			if (items.Length == 0)
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

			long newCapacity = list._longCount + items.LongLength;
			list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
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
			ReadOnlySpan<T> itemsSpan = new((T[])items);
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
					} while (itemsSpan.Length >= blockSize);
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
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, ICollection items)
		{
			long itemsCount = items.Count;
			if (itemsCount == 0)
			{
				return;
			}

			T[] tempArray = itemsCount >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T>.RentShared(BitOperations.IsPow2(itemsCount) ? (int)itemsCount : checked((int)BitOperations.RoundUpToPowerOf2((uint)itemsCount))) : new T[itemsCount];
			try
			{
				items.CopyTo(tempArray, 0);

				T[][] oldMemoryBlocks = list._memoryBlocks;
				Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
				int newItemIndex = startingItemIndex & list._blockSizeMinus1,
					sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

				long newCapacity = list._longCount + items.Count;
				list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

				// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
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
				ReadOnlySpan<T> itemsSpan = new(tempArray, 0, (int)itemsCount);

				itemsCount = list._longCount - startingItemIndex;
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
						} while (itemsSpan.Length >= blockSize);
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
			finally
			{
				RecyclableArrayPool<T>.ReturnShared(tempArray, RecyclableLongList<T>._needsClearing);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, ICollection<T> items)
		{
			long itemsCount = items.Count;
			if (itemsCount == 0)
			{
				return;
			}

			T[] tempArray = itemsCount >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T>.RentShared(BitOperations.IsPow2(itemsCount) ? (int)itemsCount : checked((int)BitOperations.RoundUpToPowerOf2((uint)itemsCount))) : new T[itemsCount];
			try
			{
				items.CopyTo(tempArray, 0);

				T[][] oldMemoryBlocks = list._memoryBlocks;
				Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
				int newItemIndex = startingItemIndex & list._blockSizeMinus1,
					sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

				long newCapacity = list._longCount + items.Count;
				list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

				// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
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
				ReadOnlySpan<T> itemsSpan = new(tempArray, 0, (int)itemsCount);

				itemsCount = list._longCount - startingItemIndex;
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
						} while (itemsSpan.Length >= blockSize);
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
			finally
			{
				RecyclableArrayPool<T>.ReturnShared(tempArray, RecyclableLongList<T>._needsClearing);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, IEnumerable items)
		{
			IEnumerator enumerator = items.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				newBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

			long newCapacity = Math.Max(list._longCount + 1, RecyclableDefaults.BlockSize);
			newCapacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
			int blockSize = Math.Max((int)(newCapacity >> list._blockSizePow2BitShift), 1);
			T[][] newMemoryBlocks = blockSize >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T[]>.RentShared(blockSize) : new T[blockSize][];

			blockSize = list._blockSize;

			// Copy references to memory block arrays which stay as-is
			Array.Copy(oldMemoryBlocks, newMemoryBlocks, newBlockIndex);

			// Prepare the first block for items in the new memory blocks
			bool usePool = blockSize >= RecyclableDefaults.MinPooledArrayLength;
			Span<T[]> newMemoryBlocksSpan = new(newMemoryBlocks);

			int oldBlockIndex = newBlockIndex;// + (newItemIndex + 1 == 0 ? 1 : 0);
			newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];

			// Copy the starting items from the list in the current block, but to a new block
			long copiedFromStartCount;
			if (newItemIndex > 0)
			{
				Array.Copy(oldMemoryBlocksSpan[oldBlockIndex], newMemoryBlocks[newBlockIndex], newItemIndex);
				copiedFromStartCount = newItemIndex;
			}
			else
			{
				copiedFromStartCount = 0;
			}

			// Add new items to the list
			long newCount = list._longCount,
				available = newCapacity - newCount,
				i;

			Span<T> newBlockSpan;
			while (true)
			{
				if (newCount + blockSize > newCapacity)
				{
					newCapacity = RecyclableLongList<T>.Helpers.Resize(ref newMemoryBlocks, blockSize, list._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)(newCapacity + blockSize))));
					newMemoryBlocksSpan = newMemoryBlocks;
					available = newCapacity - newCount;
				}

				newBlockSpan = newMemoryBlocksSpan[newBlockIndex];
				for (i = 1; i <= available; i++)
				{
					newBlockSpan[newItemIndex++] = (T)enumerator.Current;

					if (!enumerator.MoveNext())
					{
						newCount += i;
						if (newItemIndex >= blockSize)
						{
							newBlockIndex++;
							newItemIndex = 0;
							newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						}

						goto postIteration;
					}

					if (newItemIndex == blockSize)
					{
						newBlockIndex++;
						newItemIndex = 0;
						if (i == available)
						{
							break;
						}

						newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
					}
				}

				newCount += available;
			}

			// Copy the rest of the new items to the current block, if any
			postIteration:
			long toCopy = list._longCount - startingItemIndex;
			if (toCopy > 0)
			{
				// Copy the rest of items from the list after the new items
				newCapacity = RecyclableLongList<T>.Helpers.Resize(ref newMemoryBlocks, blockSize, list._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCount)));
				newMemoryBlocksSpan = newMemoryBlocks;

				Span<T> itemsSpan = new(oldMemoryBlocksSpan[oldBlockIndex], (int)copiedFromStartCount, (int)(blockSize - copiedFromStartCount));
				Span<T> newBlockArraySpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);
				copiedFromStartCount = 0;

				while (copiedFromStartCount < toCopy)
				{
					// "blockSize" will now become "toCopy". This is to save on 1 less variable.
					blockSize = (int)Math.Min(toCopy - copiedFromStartCount, itemsSpan.Length);
					if (newBlockArraySpan.Length < blockSize)
					{
						blockSize = newBlockArraySpan.Length;
						itemsSpan[..blockSize].CopyTo(newBlockArraySpan);
						newBlockIndex++;
						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex];
						itemsSpan = itemsSpan[blockSize..];
						copiedFromStartCount += blockSize;
					}
					else
					{
						itemsSpan[..blockSize].CopyTo(newBlockArraySpan);
						newBlockArraySpan = newBlockArraySpan[blockSize..];
						itemsSpan = itemsSpan[blockSize..];
						if (itemsSpan.Length == 0 && oldBlockIndex + 1 < oldMemoryBlocksSpan.Length)
						{
							oldBlockIndex++;
							itemsSpan = oldMemoryBlocksSpan[oldBlockIndex];
						}

						copiedFromStartCount += blockSize;
						if (newBlockArraySpan.Length == 0 && copiedFromStartCount < toCopy)
						{
							newBlockIndex++;
							newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex];
						}
					}
				}
			}

			list._lastBlockWithData = newBlockIndex;
			if ((newCount & list._blockSizeMinus1) == 0)
			{
				list._nextItemIndex = 0;
				list._nextItemBlockIndex = newBlockIndex + 1;
			}
			else
			{
				list._nextItemIndex = (int)(newCount & list._blockSizeMinus1);
				list._nextItemBlockIndex = newBlockIndex;
			}

			list._memoryBlocks = newMemoryBlocks;
			list._longCount = newCount;
			list._capacity = newCapacity;
			list._version++;

			// Return unused memory block arrays to the pool
			if (usePool)
			{
				// We're reusing variable to save on 1 less variable.
				usePool = RecyclableLongList<T>._needsClearing;
				while (oldBlockIndex < oldMemoryBlocksSpan.Length)
				{
					RecyclableArrayPool<T>.ReturnShared(oldMemoryBlocksSpan[oldBlockIndex++], usePool);
				}
			}

			if (oldMemoryBlocksSpan.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				RecyclableArrayPool<T[]>.ReturnShared(oldMemoryBlocks, true);
			}
		}

		public static void InsertRange<T>(this RecyclableLongList<T> targetList, int startingItemIndex, IEnumerable<T> items)
		{
			switch (items)
			{
				case RecyclableList<T> sourceRecyclableList:
					targetList.InsertRange(startingItemIndex, sourceRecyclableList);
					return;

				case RecyclableLongList<T> sourceRecyclableLongList:
					targetList.InsertRange(startingItemIndex, sourceRecyclableLongList);
					return;

				case T[] sourceArray:
					targetList.InsertRange(startingItemIndex, sourceArray);
					return;

				case Array sourceArrayWithObjects:
					targetList.InsertRange(startingItemIndex, sourceArrayWithObjects);
					return;

				case List<T> sourceList:
					targetList.InsertRange(startingItemIndex, sourceList);
					return;

				case ICollection<T> sourceICollection:
					targetList.InsertRange(startingItemIndex, sourceICollection);
					return;

				case ICollection sourceICollectionWithObjects:
					targetList.InsertRange(startingItemIndex, sourceICollectionWithObjects);
					return;

				case IReadOnlyList<T> sourceIReadOnlyList:
					targetList.InsertRange(startingItemIndex, sourceIReadOnlyList);
					return;

				default:
					targetList.InsertRangeEnumerated(startingItemIndex, items);
					return;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, IList<T> items)
		{
			long itemsCount = items.Count;
			if (itemsCount == 0)
			{
				return;
			}

			T[] tempArray = itemsCount >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T>.RentShared(BitOperations.IsPow2(itemsCount) ? (int)itemsCount : checked((int)BitOperations.RoundUpToPowerOf2((uint)itemsCount))) : new T[itemsCount];
			try
			{
				items.CopyTo(tempArray, 0);

				T[][] oldMemoryBlocks = list._memoryBlocks;
				Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
				int newItemIndex = startingItemIndex & list._blockSizeMinus1,
					sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

				long newCapacity = list._longCount + items.Count;
				list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

				// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
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
				ReadOnlySpan<T> itemsSpan = new(tempArray, 0, (int)itemsCount);

				itemsCount = list._longCount - startingItemIndex;
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
						} while (itemsSpan.Length >= blockSize);
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
			finally
			{
				RecyclableArrayPool<T>.ReturnShared(tempArray, RecyclableLongList<T>._needsClearing);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, IReadOnlyList<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				newBlockIndex = startingItemIndex >> list._blockSizePow2BitShift,
				oldBlockIndex = newBlockIndex + (newItemIndex + 1 == 0 ? 1 : 0);

			long newCapacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)(list._longCount + items.Count)));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
			int blockSize = Math.Max((int)(newCapacity >> list._blockSizePow2BitShift), 1);
			T[][] newMemoryBlocks = blockSize >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T[]>.RentShared(blockSize) : new T[blockSize][];

			blockSize = list._blockSize;

			// Copy references to memory block arrays which stay as-is
			Array.Copy(oldMemoryBlocks, newMemoryBlocks, newBlockIndex);

			// Prepare the first block for items in the new memory blocks
			bool usePool = blockSize >= RecyclableDefaults.MinPooledArrayLength;
			Span<T[]> newMemoryBlocksSpan = new(newMemoryBlocks);

			newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];

			// Copy the starting items from the list in the current block, but to a new block
			long copiedFromStartCount;
			if (newItemIndex > 0)
			{
				Array.Copy(oldMemoryBlocksSpan[newBlockIndex], newMemoryBlocks[newBlockIndex], newItemIndex);
				copiedFromStartCount = newItemIndex;
			}
			else
			{
				copiedFromStartCount = 0;
			}

			// Add new items to the list
			int sourceBlockIndex = newBlockIndex;
			long additionalCount = items.Count;
			
			int i = 0;

			Span<T> newBlockSpan = newMemoryBlocksSpan[newBlockIndex];
			while (i < additionalCount)
			{
				newBlockSpan[newItemIndex++] = items[i++];
				if (i == additionalCount)
				{
					if (newItemIndex >= blockSize)
					{
						newBlockIndex++;
						newItemIndex = 0;
						newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
					}

					break;
				}

				if (newItemIndex == blockSize)
				{
					newBlockIndex++;
					newItemIndex = 0;
					newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
				}
			}

			long itemsCount = list._longCount - startingItemIndex;
			if (itemsCount > 0)
			{
				// Copy the rest of items from the list after the new items
				int toCopy;
				Span<T> itemsSpan = new(oldMemoryBlocksSpan[sourceBlockIndex], (int)copiedFromStartCount, (int)(blockSize - copiedFromStartCount));
				copiedFromStartCount = 0;
				newBlockSpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);

				while (copiedFromStartCount < itemsCount)
				{
					toCopy = (int)Math.Min(itemsCount - copiedFromStartCount, itemsSpan.Length);
					if (newBlockSpan.Length < toCopy)
					{
						toCopy = newBlockSpan.Length;
						itemsSpan[..toCopy].CopyTo(newBlockSpan);
						newBlockIndex++;
						newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						itemsSpan = itemsSpan[toCopy..];
						copiedFromStartCount += toCopy;
					}
					else
					{
						itemsSpan[..toCopy].CopyTo(newBlockSpan);
						newBlockSpan = newBlockSpan[toCopy..];
						itemsSpan = itemsSpan[toCopy..];
						if (itemsSpan.Length == 0 && sourceBlockIndex + 1 < oldMemoryBlocksSpan.Length)
						{
							sourceBlockIndex++;
							itemsSpan = oldMemoryBlocksSpan[sourceBlockIndex];
						}

						copiedFromStartCount += toCopy;
						if (newBlockSpan.Length == 0 && copiedFromStartCount < itemsCount)
						{
							newBlockIndex++;
							newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						}
					}
				}
			}

			list._lastBlockWithData = newBlockIndex;
			additionalCount = list._longCount + items.Count;
			if ((additionalCount & list._blockSizeMinus1) == 0)
			{
				list._nextItemIndex = 0;
				list._nextItemBlockIndex = newBlockIndex + 1;
			}
			else
			{
				list._nextItemIndex = (int)(additionalCount & list._blockSizeMinus1);
				list._nextItemBlockIndex = newBlockIndex;
			}

			list._memoryBlocks = newMemoryBlocks;
			list._longCount = additionalCount;
			list._capacity = newCapacity;
			list._version++;

			// Return unused memory block arrays to the pool
			if (usePool)
			{
				// We're reusing variable to save on 1 less variable.
				usePool = RecyclableLongList<T>._needsClearing;
				while (oldBlockIndex < oldMemoryBlocksSpan.Length)
				{
					RecyclableArrayPool<T>.ReturnShared(oldMemoryBlocksSpan[oldBlockIndex++], usePool);
				}
			}

			if (oldMemoryBlocksSpan.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				RecyclableArrayPool<T[]>.ReturnShared(oldMemoryBlocks, true);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, List<T> items)
		{
			int sourceItemsCount = items.Count;
			if (sourceItemsCount == 0)
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

			long newCapacity = list._longCount + sourceItemsCount;
			list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
			int blockSize = Math.Max((int)(list._capacity >> list._blockSizePow2BitShift), 1);
			T[][] newMemoryBlocks = blockSize >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T[]>.RentShared(blockSize) : new T[blockSize][];
			blockSize = list._blockSize;

			// Copy references to memory block arrays which stay as-is
			if (sourceBlockIndex > 0)
			{
				Array.Copy(oldMemoryBlocks, newMemoryBlocks, sourceBlockIndex);
			}

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
			//Span<T> newBlockArraySpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);
			T[] newBlockArray = newMemoryBlocksSpan[newBlockIndex];
			int newBlockArrayLength = blockSize - newItemIndex;
			int sourceItemIndex = 0;
			if (sourceItemsCount >= newBlockArrayLength + sourceItemIndex)
			{
				if (usePool)
				{
					do
					{
						items.CopyTo(sourceItemIndex, newBlockArray, newItemIndex, newBlockArrayLength);
						sourceItemIndex += newBlockArrayLength;
						newBlockIndex++;
						if (newItemIndex != 0)
						{
							newItemIndex = 0;
							newBlockArrayLength = blockSize;
						}

						if (sourceItemIndex == sourceItemsCount)
						{
							break;
						}

						newBlockArray = newMemoryBlocksSpan[newBlockIndex] = RecyclableArrayPool<T>.RentShared(blockSize);
					} while (sourceItemsCount >= blockSize + sourceItemIndex);
				}
				else
				{
					do
					{
						items.CopyTo(sourceItemIndex, newBlockArray, newItemIndex, newBlockArrayLength);
						sourceItemIndex += newBlockArrayLength;
						newBlockIndex++;
						if (newItemIndex != 0)
						{
							newItemIndex = 0;
							newBlockArrayLength = blockSize;
						}

						if (sourceItemIndex == sourceItemsCount)
						{
							break;
						}

						newBlockArray = newMemoryBlocksSpan[newBlockIndex] = new T[blockSize];
					} while (sourceItemsCount >= blockSize + sourceItemIndex);
				}
			}

			// Copy the rest of the new items to the current block, if any
			if (sourceItemsCount > sourceItemIndex)
			{
				items.CopyTo(sourceItemIndex, newBlockArray, newItemIndex, sourceItemsCount - sourceItemIndex);
				newItemIndex += sourceItemsCount - sourceItemIndex;
			}
			else
			{
				//newItemIndex = 0;
				newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
			}

			long itemsCount = list._longCount - startingItemIndex;
			if (itemsCount > 0)
			{
				// Copy the rest of items from the list after the new items
				int toCopy;
				Span<T> itemsSpan = new(oldMemoryBlocksSpan[sourceBlockIndex], (int)copiedCount, (int)(blockSize - copiedCount));
				copiedCount = 0;
				var newBlockSpan = new Span<T>(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);

				while (copiedCount < itemsCount)
				{
					toCopy = (int)Math.Min(itemsCount - copiedCount, itemsSpan.Length);
					if (newBlockSpan.Length < toCopy)
					{
						toCopy = newBlockSpan.Length;
						itemsSpan[..toCopy].CopyTo(newBlockSpan);
						newBlockIndex++;
						newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						itemsSpan = itemsSpan[toCopy..];
						copiedCount += toCopy;
					}
					else
					{
						itemsSpan[..toCopy].CopyTo(newBlockSpan);
						newBlockSpan = newBlockSpan[toCopy..];
						itemsSpan = itemsSpan[toCopy..];
						if (itemsSpan.Length == 0 && sourceBlockIndex + 1 < oldMemoryBlocksSpan.Length)
						{
							sourceBlockIndex++;
							itemsSpan = oldMemoryBlocksSpan[sourceBlockIndex];
						}

						copiedCount += toCopy;
						if (newBlockSpan.Length == 0 && copiedCount < itemsCount)
						{
							newBlockIndex++;
							newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
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
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, ReadOnlySpan<T> items)
		{
			if (items.Length == 0)
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

			long newCapacity = list._longCount + items.Length;
			list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
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
			Span<T> newBlockArraySpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);

			//sourceItemIndex = 0;
			if (items.Length >= newBlockArraySpan.Length)
			{
				newItemIndex = 0;
				if (usePool)
				{
					do
					{
						items[..newBlockArraySpan.Length].CopyTo(newBlockArraySpan);
						items = items[newBlockArraySpan.Length..];
						newBlockIndex++;
						if (items.Length == 0)
						{
							break;
						}

						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = RecyclableArrayPool<T>.RentShared(blockSize);
					} while (items.Length >= blockSize);
				}
				else
				{
					do
					{
						items[..newBlockArraySpan.Length].CopyTo(newBlockArraySpan);
						items = items[newBlockArraySpan.Length..];
						newBlockIndex++;
						if (items.Length == 0)
						{
							break;
						}

						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = new T[blockSize];
					} while (items.Length >= blockSize);
				}
			}

			// Copy the rest of the new items to the current block, if any
			if (items.Length > 0)
			{
				items.CopyTo(newBlockArraySpan);
				newItemIndex += items.Length;
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
				items = new(oldMemoryBlocksSpan[sourceBlockIndex], (int)copiedCount, (int)(blockSize - copiedCount));
				copiedCount = 0;
				newBlockArraySpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);

				while (copiedCount < itemsCount)
				{
					toCopy = (int)Math.Min(itemsCount - copiedCount, items.Length);
					if (newBlockArraySpan.Length < toCopy)
					{
						toCopy = newBlockArraySpan.Length;
						items[..toCopy].CopyTo(newBlockArraySpan);
						newBlockIndex++;
						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						items = items[toCopy..];
						copiedCount += toCopy;
					}
					else
					{
						items[..toCopy].CopyTo(newBlockArraySpan);
						newBlockArraySpan = newBlockArraySpan[toCopy..];
						items = items[toCopy..];
						if (items.Length == 0 && sourceBlockIndex + 1 < oldMemoryBlocksSpan.Length)
						{
							sourceBlockIndex++;
							items = oldMemoryBlocksSpan[sourceBlockIndex];
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
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, RecyclableList<T> items)
		{
			if (items._count == 0)
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

			long newCapacity = list._longCount + items._count;
			list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
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
			ReadOnlySpan<T> itemsSpan = new(items._memoryBlock, 0, items._count);
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
					} while (itemsSpan.Length >= blockSize);
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
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, RecyclableLongList<T> items)
		{
			if (items._longCount == 0)
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

			long newCapacity = list._longCount + items._longCount;
			list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
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
			int sourceBlockIndex2 = 0,
				sourceBlockSize = items._blockSize,
				toCopy;

			long copiedCount2 = 0,
				itemsCount2 = items._longCount;

			ReadOnlySpan<T[]> sourceMemoryBlocksSpan = items._memoryBlocks;
			ReadOnlySpan<T> itemsSpan = new(sourceMemoryBlocksSpan[sourceBlockIndex2], 0, (int)Math.Min(sourceBlockSize, itemsCount2));
			Span<T> newBlockSpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);
			
			while (copiedCount2 < itemsCount2)
			{
				toCopy = (int)Math.Min(itemsCount2 - copiedCount2, itemsSpan.Length);
				if (newBlockSpan.Length < toCopy)
				{
					toCopy = newBlockSpan.Length;
					itemsSpan[..toCopy].CopyTo(newBlockSpan);
					newBlockIndex++;
					newItemIndex = 0;
					newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
					itemsSpan = itemsSpan[toCopy..];
					copiedCount2 += toCopy;
				}
				else
				{
					itemsSpan[..toCopy].CopyTo(newBlockSpan);
					itemsSpan = itemsSpan[toCopy..];
					copiedCount2 += toCopy;
					newItemIndex += toCopy;
					if (itemsSpan.Length == 0 && sourceBlockIndex2 + 1 < sourceMemoryBlocksSpan.Length)
					{
						sourceBlockIndex2++;
						itemsSpan = new(sourceMemoryBlocksSpan[sourceBlockIndex2], 0, sourceBlockSize);
					}

					if (newItemIndex == blockSize)
					{
						newBlockIndex++;
						newItemIndex = 0;
						if (copiedCount2 < itemsCount2)
						{
							newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						}
					}
					else
					{
						newBlockSpan = newBlockSpan[toCopy..];
					}
				}
			}

			// Copy the rest of the new items to the current block, if any
			//if (itemsSpan.Length > 0)
			//{
			//	itemsSpan.CopyTo(newBlockSpan);
			//	newItemIndex += itemsSpan.Length;
			//}
			//else
			//{
			//	newItemIndex = 0;
			//	newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
			//}

			long itemsCount = list._longCount - startingItemIndex;
			if (itemsCount > 0)
			{
				if (newMemoryBlocksSpan[newBlockIndex] == null)
				{
					newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
				}
				// Copy the rest of items from the list after the new items
				itemsSpan = new(oldMemoryBlocksSpan[sourceBlockIndex], (int)copiedCount, (int)(blockSize - copiedCount));
				copiedCount = 0;
				newBlockSpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);

				while (copiedCount < itemsCount)
				{
					toCopy = (int)Math.Min(itemsCount - copiedCount, itemsSpan.Length);
					if (newBlockSpan.Length < toCopy)
					{
						toCopy = newBlockSpan.Length;
						itemsSpan[..toCopy].CopyTo(newBlockSpan);
						newBlockIndex++;
						newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						itemsSpan = itemsSpan[toCopy..];
						copiedCount += toCopy;
					}
					else
					{
						itemsSpan[..toCopy].CopyTo(newBlockSpan);
						newBlockSpan = newBlockSpan[toCopy..];
						itemsSpan = itemsSpan[toCopy..];
						if (itemsSpan.Length == 0 && sourceBlockIndex + 1 < oldMemoryBlocksSpan.Length)
						{
							sourceBlockIndex++;
							itemsSpan = oldMemoryBlocksSpan[sourceBlockIndex];
						}

						copiedCount += toCopy;
						if (newBlockSpan.Length == 0 && copiedCount < itemsCount)
						{
							newBlockIndex++;
							newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
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
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, Span<T> items)
		{
			if (items.Length == 0)
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

			long newCapacity = list._longCount + items.Length;
			list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
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
			Span<T> newBlockArraySpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);

			//sourceItemIndex = 0;
			if (items.Length >= newBlockArraySpan.Length)
			{
				newItemIndex = 0;
				if (usePool)
				{
					do
					{
						items[..newBlockArraySpan.Length].CopyTo(newBlockArraySpan);
						items = items[newBlockArraySpan.Length..];
						newBlockIndex++;
						if (items.Length == 0)
						{
							break;
						}

						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = RecyclableArrayPool<T>.RentShared(blockSize);
					} while (items.Length >= blockSize);
				}
				else
				{
					do
					{
						items[..newBlockArraySpan.Length].CopyTo(newBlockArraySpan);
						items = items[newBlockArraySpan.Length..];
						newBlockIndex++;
						if (items.Length == 0)
						{
							break;
						}

						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = new T[blockSize];
					} while (items.Length >= blockSize);
				}
			}

			// Copy the rest of the new items to the current block, if any
			if (items.Length > 0)
			{
				items.CopyTo(newBlockArraySpan);
				newItemIndex += items.Length;
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
				items = new(oldMemoryBlocksSpan[sourceBlockIndex], (int)copiedCount, (int)(blockSize - copiedCount));
				copiedCount = 0;
				newBlockArraySpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);

				while (copiedCount < itemsCount)
				{
					toCopy = (int)Math.Min(itemsCount - copiedCount, items.Length);
					if (newBlockArraySpan.Length < toCopy)
					{
						toCopy = newBlockArraySpan.Length;
						items[..toCopy].CopyTo(newBlockArraySpan);
						newBlockIndex++;
						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						items = items[toCopy..];
						copiedCount += toCopy;
					}
					else
					{
						items[..toCopy].CopyTo(newBlockArraySpan);
						newBlockArraySpan = newBlockArraySpan[toCopy..];
						items = items[toCopy..];
						if (items.Length == 0 && sourceBlockIndex + 1 < oldMemoryBlocksSpan.Length)
						{
							sourceBlockIndex++;
							items = oldMemoryBlocksSpan[sourceBlockIndex];
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
		public static void InsertRange<T>(this RecyclableLongList<T> list, int startingItemIndex, T[] items)
		{
			if (items.Length == 0)
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				sourceBlockIndex = startingItemIndex >> list._blockSizePow2BitShift;

			long newCapacity = list._longCount + items.LongLength;
			list._capacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCapacity));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
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
					} while (itemsSpan.Length >= blockSize);
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
		public static void InsertRangeEnumerated<T>(this RecyclableLongList<T> list, int startingItemIndex, IEnumerable<T> items)
		{
			using IEnumerator<T> enumerator = items.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return;
			}

			T[][] oldMemoryBlocks = list._memoryBlocks;
			Span<T[]> oldMemoryBlocksSpan = new(oldMemoryBlocks);
			int newItemIndex = startingItemIndex & list._blockSizeMinus1,
				newBlockIndex = startingItemIndex >> list._blockSizePow2BitShift,
				oldBlockIndex = newBlockIndex;// + (newItemIndex + 1 == 0 ? 1 : 0);

			long newCapacity = checked((long)BitOperations.RoundUpToPowerOf2((ulong)Math.Max(list._longCount + 1, RecyclableDefaults.BlockSize)));

			// This is "requiredBlockCount". We're reusing blockSize var to save on the total no. of variables.
			int blockSize = Math.Max((int)(newCapacity >> list._blockSizePow2BitShift), 1);
			T[][] newMemoryBlocks = blockSize >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T[]>.RentShared(blockSize) : new T[blockSize][];

			blockSize = list._blockSize;

			// Copy references to memory block arrays which stay as-is
			Array.Copy(oldMemoryBlocks, newMemoryBlocks, newBlockIndex);

			// Prepare the first block for items in the new memory blocks
			bool usePool = blockSize >= RecyclableDefaults.MinPooledArrayLength;
			Span<T[]> newMemoryBlocksSpan = new(newMemoryBlocks);

			newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];

			// Copy the starting items from the list in the current block, but to a new block
			long copiedFromStartCount;
			if (newItemIndex > 0)
			{
				Array.Copy(oldMemoryBlocksSpan[oldBlockIndex], newMemoryBlocks[newBlockIndex], newItemIndex);
				copiedFromStartCount = newItemIndex;
			}
			else
			{
				copiedFromStartCount = 0;
			}

			// Add new items to the list
			long newCount = list._longCount,
				available = newCapacity - newCount,
				i;

			Span<T> newBlockSpan;
			while (true)
			{
				if (newCount + blockSize > newCapacity)
				{
					newCapacity = RecyclableLongList<T>.Helpers.Resize(ref newMemoryBlocks, blockSize, list._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)(newCapacity + blockSize))));
					newMemoryBlocksSpan = newMemoryBlocks;
					available = newCapacity - newCount;
				}

				newBlockSpan = newMemoryBlocksSpan[newBlockIndex];
				for (i = 1; i <= available; i++)
				{
					newBlockSpan[newItemIndex++] = enumerator.Current;

					if (!enumerator.MoveNext())
					{
						newCount += i;
						if (newItemIndex >= blockSize)
						{
							newBlockIndex++;
							newItemIndex = 0;
							newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
						}

						goto postIteration;
					}

					if (newItemIndex == blockSize)
					{
						newBlockIndex++;
						newItemIndex = 0;
						if (i == available)
						{
							break;
						}

						newBlockSpan = newMemoryBlocksSpan[newBlockIndex] = usePool ? RecyclableArrayPool<T>.RentShared(blockSize) : new T[blockSize];
					}
				}

				newCount += available;
			}

			// Copy the rest of the new items to the current block, if any
			postIteration:
			long toCopy = list._longCount - startingItemIndex;
			if (toCopy > 0)
			{
				// Copy the rest of items from the list after the new items
				newCapacity = RecyclableLongList<T>.Helpers.Resize(ref newMemoryBlocks, blockSize, list._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)newCount)));
				newMemoryBlocksSpan = newMemoryBlocks;

				Span<T> itemsSpan = new(oldMemoryBlocksSpan[oldBlockIndex], (int)copiedFromStartCount, (int)(blockSize - copiedFromStartCount));
				Span<T> newBlockArraySpan = new(newMemoryBlocksSpan[newBlockIndex], newItemIndex, blockSize - newItemIndex);
				copiedFromStartCount = 0;

				while (copiedFromStartCount < toCopy)
				{
					// "blockSize" will now become "toCopy". This is to save on 1 less variable.
					blockSize = (int)Math.Min(toCopy - copiedFromStartCount, itemsSpan.Length);
					if (newBlockArraySpan.Length < blockSize)
					{
						blockSize = newBlockArraySpan.Length;
						itemsSpan[..blockSize].CopyTo(newBlockArraySpan);
						newBlockIndex++;
						newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex];
						itemsSpan = itemsSpan[blockSize..];
						copiedFromStartCount += blockSize;
					}
					else
					{
						itemsSpan[..blockSize].CopyTo(newBlockArraySpan);
						newBlockArraySpan = newBlockArraySpan[blockSize..];
						itemsSpan = itemsSpan[blockSize..];
						if (itemsSpan.Length == 0 && oldBlockIndex + 1 < oldMemoryBlocksSpan.Length)
						{
							oldBlockIndex++;
							itemsSpan = oldMemoryBlocksSpan[oldBlockIndex];
						}

						copiedFromStartCount += blockSize;
						if (newBlockArraySpan.Length == 0 && copiedFromStartCount < toCopy)
						{
							newBlockIndex++;
							newBlockArraySpan = newMemoryBlocksSpan[newBlockIndex];
						}
					}
				}
			}

			list._lastBlockWithData = newBlockIndex;
			if ((newCount & list._blockSizeMinus1) == 0)
			{
				list._nextItemIndex = 0;
				list._nextItemBlockIndex = newBlockIndex + 1;
			}
			else
			{
				list._nextItemIndex = (int)(newCount & list._blockSizeMinus1);
				list._nextItemBlockIndex = newBlockIndex;
			}

			list._memoryBlocks = newMemoryBlocks;
			list._longCount = newCount;
			list._capacity = newCapacity;
			list._version++;

			// Return unused memory block arrays to the pool
			if (usePool)
			{
				// We're reusing variable to save on 1 less variable.
				usePool = RecyclableLongList<T>._needsClearing;
				while (oldBlockIndex < oldMemoryBlocksSpan.Length)
				{
					RecyclableArrayPool<T>.ReturnShared(oldMemoryBlocksSpan[oldBlockIndex++], usePool);
				}
			}

			if (oldMemoryBlocksSpan.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				RecyclableArrayPool<T[]>.ReturnShared(oldMemoryBlocks, true);
			}
		}
	}
}
