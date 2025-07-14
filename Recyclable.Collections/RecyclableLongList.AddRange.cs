using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Reflection;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
        public static class zRecyclableLongListAddRange
        {

                private static class AddRangeHelper<T>
                {
                        private static readonly Action<RecyclableLongList<T>, object>? _dictionaryAdder;
                        private static readonly Action<RecyclableLongList<T>, object>? _sortedListAdder;
                        private static readonly Type? _dictionaryType;
                        private static readonly Type? _sortedListType;

                        static AddRangeHelper()
                        {
                                Type elementType = typeof(T);
                                if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                                {
                                        Type[] args = elementType.GetGenericArguments();
                                        _dictionaryType = typeof(RecyclableDictionary<,>).MakeGenericType(args);
                                        MethodInfo? method = typeof(zRecyclableLongListAddRange).GetMethod(
                                                nameof(AddRange),
                                                BindingFlags.NonPublic | BindingFlags.Static,
                                                binder: null,
                                                types: new[] { typeof(RecyclableLongList<>).MakeGenericType(elementType), _dictionaryType },
                                                modifiers: null);
                                        if (method != null)
                                        {
                                                var listParam = Expression.Parameter(typeof(RecyclableLongList<T>));
                                                var objParam = Expression.Parameter(typeof(object));
                                                var call = Expression.Call(method, listParam, Expression.Convert(objParam, _dictionaryType));
                                                _dictionaryAdder = Expression.Lambda<Action<RecyclableLongList<T>, object>>(call, listParam, objParam).Compile();
                                        }
                                }
                                else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                                {
                                        Type[] args = elementType.GetGenericArguments();
                                        _sortedListType = typeof(RecyclableSortedList<,>).MakeGenericType(args);
                                        MethodInfo? method = typeof(zRecyclableLongListAddRange).GetMethod(
                                                nameof(AddRange),
                                                BindingFlags.NonPublic | BindingFlags.Static,
                                                binder: null,
                                                types: new[] { typeof(RecyclableLongList<>).MakeGenericType(elementType), _sortedListType },
                                                modifiers: null);
                                        if (method != null)
                                        {
                                                var listParam = Expression.Parameter(typeof(RecyclableLongList<T>));
                                                var objParam = Expression.Parameter(typeof(object));
                                                var call = Expression.Call(method, listParam, Expression.Convert(objParam, _sortedListType));
                                                _sortedListAdder = Expression.Lambda<Action<RecyclableLongList<T>, object>>(call, listParam, objParam).Compile();
                                        }
                                }
                        }

                        internal static bool TryAddRange(RecyclableLongList<T> list, IEnumerable<T> items)
                        {
                                if (_dictionaryAdder != null && _dictionaryType!.IsInstanceOfType(items))
                                {
                                        _dictionaryAdder(list, items);
                                        return true;
                                }

                                if (_sortedListAdder != null && _sortedListType!.IsInstanceOfType(items))
                                {
                                        _sortedListAdder(list, items);
                                        return true;
                                }

                                return false;
                        }
                }

                private static void AddRangeEnumerated<T>(this RecyclableLongList<T> targetList, IEnumerable<T> items)
                {
			using IEnumerator<T> enumerator = items.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return;
			}

			int blockSize = targetList._blockSize,
				targetBlockIdx = targetList._nextItemBlockIndex,
				targetItemIdx = targetList._nextItemIndex;

			long capacity = targetList._capacity,
				copied = targetList._longCount,
				available = capacity - copied,
				i;

			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Span<T> blockArraySpan;

			while (true)
			{
				if (copied + blockSize > capacity)
				{
					capacity = RecyclableLongList<T>.Helpers.Resize(targetList, blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)(capacity + blockSize))));
					memoryBlocksSpan = targetList._memoryBlocks;
					available = capacity - copied;
				}

				blockArraySpan = memoryBlocksSpan[targetBlockIdx];
				for (i = 1; i <= available; i++)
				{
					blockArraySpan[targetItemIdx++] = enumerator.Current;

					if (!enumerator.MoveNext())
					{
						targetList._lastBlockWithData = targetBlockIdx;
						targetList._longCount += copied + i - targetList._longCount;
						if (targetItemIdx < blockSize)
						{
							targetList._nextItemBlockIndex = targetBlockIdx;
							targetList._nextItemIndex = targetItemIdx;
						}
						else
						{
							targetList._nextItemBlockIndex = targetBlockIdx + 1;
							targetList._nextItemIndex = 0;
						}

						targetList._capacity = capacity;
						targetList._version++;
						return;
					}

					if (targetItemIdx == blockSize)
					{
						targetBlockIdx++;
						targetItemIdx = 0;
						if (i == available)
						{
							break;
						}

						blockArraySpan = memoryBlocksSpan[targetBlockIdx];
					}
				}

				copied += available;
			}
		}

		private static void AddRangeWithKnownCount<T>(this RecyclableLongList<T> targetList, IEnumerable<T> items, int requiredAdditionalCapacity)
		{
			long requiredCapacity = targetList._longCount + requiredAdditionalCapacity;
			if (requiredCapacity == 0)
			{
				return;
			}

			int blockSize = targetList._blockSize,
				targetBlockIdx = targetList._nextItemBlockIndex,
				targetItemIdx = targetList._nextItemIndex;

			if (targetList._capacity < requiredCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)requiredCapacity)));
			}

			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Span<T> blockArraySpan = memoryBlocksSpan[targetBlockIdx];
			foreach (var item in items)
			{
				blockArraySpan[targetItemIdx++] = item;
				if (targetItemIdx == blockSize)
				{
					targetBlockIdx++;
					targetItemIdx = 0;
					if (targetBlockIdx == memoryBlocksSpan.Length)
					{
						break;
					}

					blockArraySpan = memoryBlocksSpan[targetBlockIdx];
				}
			}

			targetList._longCount = requiredCapacity;
			targetList._nextItemBlockIndex = targetBlockIdx;
			targetList._nextItemIndex = targetItemIdx;
			targetList._lastBlockWithData = targetBlockIdx - (targetItemIdx > 0 ? 0 : 1);
			targetList._version++;
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, in Array items)
		{
			if (items.LongLength == 0)
			{
				return;
			}

			int blockSize = targetList._blockSize,
				targetBlockIndex = targetList._nextItemBlockIndex;

			long fullBlockItemsCount = items.LongLength - blockSize,
				sourceItemIndex = Math.Min(blockSize - targetList._nextItemIndex, items.LongLength),
				targetCapacity = targetList._longCount + items.LongLength;

			if (targetList._capacity < targetCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Array.Copy(items, 0, memoryBlocksSpan[targetBlockIndex++], targetList._nextItemIndex, sourceItemIndex);
			while (sourceItemIndex < fullBlockItemsCount)
			{
				Array.Copy(items, sourceItemIndex, memoryBlocksSpan[targetBlockIndex++], 0, blockSize);
				sourceItemIndex += blockSize;
			}

			// We're reusing a variable which is no longer needed. That's to avoid unnecessary
			// allocation.
			if ((blockSize = (int)(items.LongLength - sourceItemIndex)) > 0)
			{
				Array.Copy(items, sourceItemIndex, memoryBlocksSpan[targetBlockIndex], 0, blockSize);
				targetList._lastBlockWithData = targetBlockIndex;
				targetList._nextItemBlockIndex = targetBlockIndex;
				targetList._nextItemIndex = blockSize;
			}
			else
			{
				targetList._lastBlockWithData = targetBlockIndex - 1;
				if (targetList._nextItemIndex + sourceItemIndex >= targetList._blockSize)
				{
					targetList._nextItemBlockIndex = targetBlockIndex;
					targetList._nextItemIndex = 0;
				}
				else
				{
					targetList._nextItemIndex += checked((int)sourceItemIndex);
				}
			}

			targetList._longCount = targetCapacity;
			targetList._version++;
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, ICollection items)
		{
			if (items.Count == 0)
			{
				return;
			}

			int targetBlockIndex = targetList._nextItemBlockIndex;
			long targetCapacity = targetList._longCount + items.Count;
			if (targetList._capacity < targetCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			// We're better off to temporarily copy it to a fixed array,
			// than copying them item by item. We may run into OOMs, though.
			T[] itemsBuffer = items.Count >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)items.Count))) : new T[items.Count];
			items.CopyTo(itemsBuffer, 0);

			ReadOnlySpan<T> itemsSpan = new(itemsBuffer, 0, items.Count);
			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], targetList._nextItemIndex, targetList._blockSize - targetList._nextItemIndex);
			while (targetBlockArraySpan.Length <= itemsSpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				targetBlockIndex++;
				if (itemsSpan.Length == 0)
				{
					break;
				}

				targetBlockArraySpan = memoryBlocksSpan[targetBlockIndex];
			}

			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(targetBlockArraySpan);
				if (targetList._nextItemBlockIndex != targetBlockIndex)
				{
					targetList._nextItemIndex = itemsSpan.Length;
					targetList._nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					targetList._nextItemIndex += itemsSpan.Length;
				}
			}
			else
			{
				targetList._nextItemIndex = 0;
			}

			targetList._longCount = targetCapacity;
			targetList._nextItemBlockIndex = targetBlockIndex;
			targetList._lastBlockWithData = targetBlockIndex - (itemsSpan.Length > 0 ? 0 : 1);

			if (items.Count >= RecyclableDefaults.MinPooledArrayLength)
			{
				RecyclableArrayPool<T>.ReturnShared(itemsBuffer, RecyclableLongList<T>._needsClearing);
			}

			targetList._version++;
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, ICollection<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			int targetBlockIndex = targetList._nextItemBlockIndex;

			long targetCapacity = targetList._longCount + items.Count;
			if (targetList._capacity < targetCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			// We're better off to temporarily copy it to a fixed array,
			// than copying them item by item. We may run into OOMs, though.
			T[] itemsBuffer = items.Count >= RecyclableDefaults.MinPooledArrayLength ? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)items.Count))) : new T[items.Count];

			items.CopyTo(itemsBuffer, 0);

			ReadOnlySpan<T> itemsSpan = new(itemsBuffer, 0, items.Count);
			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], targetList._nextItemIndex, targetList._blockSize - targetList._nextItemIndex);
				
			while (targetBlockArraySpan.Length <= itemsSpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				targetBlockIndex++;
				if (itemsSpan.Length == 0)
				{
					break;
				}

				targetBlockArraySpan = memoryBlocksSpan[targetBlockIndex];
			}

			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(targetBlockArraySpan);
				if (targetList._nextItemBlockIndex != targetBlockIndex)
				{
					targetList._nextItemIndex = itemsSpan.Length;
					targetList._nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					targetList._nextItemIndex += itemsSpan.Length;
				}
			}
			else
			{
				targetList._nextItemIndex = 0;
			}

			targetList._longCount = targetCapacity;
			targetList._nextItemBlockIndex = targetBlockIndex;
			targetList._lastBlockWithData = targetBlockIndex - (itemsSpan.Length > 0 ? 0 : 1);

			if (items.Count >= RecyclableDefaults.MinPooledArrayLength)
			{
				RecyclableArrayPool<T>.ReturnShared(itemsBuffer, RecyclableLongList<T>._needsClearing);
			}

			targetList._version++;
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, IEnumerable source)
		{
			IEnumerator enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return;
			}

			int blockSize = targetList._blockSize,
				targetBlockIdx = targetList._nextItemBlockIndex,
				targetItemIdx = targetList._nextItemIndex;

			long capacity = targetList._capacity,
				copied = targetList._longCount,
				available = capacity - copied,
				i;

			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Span<T> blockArraySpan;

			while (true)
			{
				if (copied + blockSize > capacity)
				{
					capacity = RecyclableLongList<T>.Helpers.Resize(targetList, blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)(capacity + blockSize))));
					memoryBlocksSpan = targetList._memoryBlocks;
					available = capacity - copied;
				}

				blockArraySpan = memoryBlocksSpan[targetBlockIdx];
				for (i = 1; i <= available; i++)
				{
					blockArraySpan[targetItemIdx++] = (T)enumerator.Current;

					if (!enumerator.MoveNext())
					{
						targetList._lastBlockWithData = targetBlockIdx;
						targetList._longCount += copied + i - targetList._longCount;
						if (targetItemIdx < blockSize)
						{
							targetList._nextItemBlockIndex = targetBlockIdx;
							targetList._nextItemIndex = targetItemIdx;
						}
						else
						{
							targetList._nextItemBlockIndex = targetBlockIdx + 1;
							targetList._nextItemIndex = 0;
						}

						targetList._capacity = capacity;
						return;
					}

					if (targetItemIdx == blockSize)
					{
						targetBlockIdx++;
						targetItemIdx = 0;

						if (i == available)
						{
							break;
						}

						blockArraySpan = memoryBlocksSpan[targetBlockIdx];
					}
				}

				copied += available;
			}
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, IEnumerable<T> items)
		{
			switch (items)
			{
				case RecyclableList<T> sourceRecyclableList:
					targetList.AddRange(sourceRecyclableList);
					return;

				case RecyclableLongList<T> sourceRecyclableLongList:
					targetList.AddRange(sourceRecyclableLongList);
					return;

				case T[] sourceArray:
					targetList.AddRange(sourceArray);
					return;

				case Array sourceArrayWithObjects:
					targetList.AddRange(sourceArrayWithObjects);
					return;

				case List<T> sourceList:
					targetList.AddRange(sourceList);
					return;

				case ICollection<T> sourceICollection:
					targetList.AddRange(sourceICollection);
					return;

                                case ICollection sourceICollectionWithObjects:
                                        targetList.AddRange(sourceICollectionWithObjects);
                                        return;

                                case IReadOnlyList<T> sourceIReadOnlyList:
                                        targetList.AddRange(sourceIReadOnlyList);
                                        return;

                                case RecyclableHashSet<T> sourceHashSet:
                                        targetList.AddRange(sourceHashSet);
                                        return;

                                case RecyclableStack<T> sourceStack:
                                        targetList.AddRange(sourceStack);
                                        return;

                                case RecyclableSortedSet<T> sourceSortedSet:
                                        targetList.AddRange(sourceSortedSet);
                                        return;

                                case RecyclableLinkedList<T> sourceLinkedList:
                                        targetList.AddRange(sourceLinkedList);
                                        return;

                                case RecyclablePriorityQueue<T> sourcePriorityQueue:
                                        targetList.AddRange(sourcePriorityQueue);
                                        return;

                                case RecyclableQueue<T> sourceQueue:
                                        targetList.AddRange(sourceQueue);
                                        return;

                                default:
                                        if (AddRangeHelper<T>.TryAddRange(targetList, items))
                                        {
                                                return;
                                        }

                                        if (items.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
                                        {
                                                targetList.AddRangeWithKnownCount(items, requiredAdditionalCapacity);
                                        }
                                        else
                                        {
                                                targetList.AddRangeEnumerated(items);
                                        }

                                        return;
                        }
                }

		public static void AddRange<T>(this RecyclableLongList<T> targetList, IReadOnlyList<T> items)
		{
			long sourceItemsCount = items.Count,
				requiredCapacity = targetList._longCount + sourceItemsCount;

			if (requiredCapacity == 0)
			{
				return;
			}

			int blockSize = targetList._blockSize,
				targetBlockIdx = targetList._nextItemBlockIndex,
				targetItemIdx = targetList._nextItemIndex;

			if (targetList._capacity < requiredCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)requiredCapacity)));
			}

			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Span<T> blockArraySpan = memoryBlocksSpan[targetBlockIdx];

			for (var itemIndex = 0; itemIndex < sourceItemsCount; itemIndex++)
			{
				blockArraySpan[targetItemIdx++] = items[itemIndex];
				if (targetItemIdx == blockSize)
				{
					targetItemIdx = 0;
					targetBlockIdx++;
					if (itemIndex + 1 == sourceItemsCount)
					{
						break;
					}

					blockArraySpan = memoryBlocksSpan[targetBlockIdx];
				}
			}

			targetList._longCount = requiredCapacity;
			targetList._nextItemBlockIndex = targetBlockIdx;
			targetList._nextItemIndex = targetItemIdx;
			targetList._lastBlockWithData = targetBlockIdx - (targetItemIdx > 0 ? 0 : 1);
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, List<T> items)
		{
			if (items.Count == 0)
			{
				return;
			}

			int blockSize = targetList._blockSize,
				itemsCount = items.Count,
				copied = Math.Min(blockSize - targetList._nextItemIndex, itemsCount),
				targetBlockIndex = targetList._nextItemBlockIndex;

			long targetCapacity = targetList._longCount + items.Count;
			if (targetList._capacity < targetCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			items.CopyTo(0, memoryBlocksSpan[targetBlockIndex], targetList._nextItemIndex, copied);

			if (targetList._nextItemIndex + copied == blockSize)
			{
				targetBlockIndex++;
				targetList._nextItemIndex = 0;
			}
			else
			{
				targetList._nextItemIndex += copied;
			}

			while (blockSize <= itemsCount - copied)
			{
				items.CopyTo(copied, memoryBlocksSpan[targetBlockIndex++], 0, blockSize);
				copied += blockSize;
			}

			if ((itemsCount - copied < blockSize) && (itemsCount - copied != 0))
			{
				targetList._nextItemIndex = itemsCount - copied;
				items.CopyTo(copied, memoryBlocksSpan[targetBlockIndex], 0, targetList._nextItemIndex);
			}

			targetList._nextItemBlockIndex = targetBlockIndex;
			targetList._longCount = targetCapacity;
			targetList._lastBlockWithData = targetBlockIndex - (targetList._nextItemIndex > 0 ? 0 : 1);
			targetList._version++;
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, ReadOnlySpan<T> items)
		{
			if (items.Length == 0)
			{
				return;
			}

			int targetBlockIndex = targetList._nextItemBlockIndex;

			long targetCapacity = targetList._longCount + items.Length;
			if (targetList._capacity < targetCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], targetList._nextItemIndex, targetList._blockSize - targetList._nextItemIndex);
			
			while (items.Length >= targetBlockArraySpan.Length)
			{
				items[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				items = items[targetBlockArraySpan.Length..];
				if (items.Length == 0)
				{
					break;
				}

				targetBlockIndex++;
				targetBlockArraySpan = memoryBlocksSpan[targetBlockIndex];
			}

			if (items.Length > 0)
			{
				items.CopyTo(targetBlockArraySpan);
				if (targetList._nextItemBlockIndex != targetBlockIndex)
				{
					targetList._nextItemIndex = items.Length;
					targetList._nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					targetList._nextItemIndex += items.Length;
				}
			}
			else
			{
				targetList._nextItemBlockIndex = targetBlockIndex + 1;
				targetList._nextItemIndex = 0;
			}

			targetList._longCount = targetCapacity;
			targetList._lastBlockWithData = targetBlockIndex;
			targetList._version++;
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, RecyclableList<T> items)
		{
			if (items._count == 0)
			{
				return;
			}

			int targetBlockIndex = targetList._nextItemBlockIndex;

			long targetCapacity = targetList._longCount + items._count;
			if (targetList._capacity < targetCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			ReadOnlySpan<T> itemsSpan = new(items._memoryBlock, 0, items._count);
			ReadOnlySpan<T[]> targetMemoryBlocksSpan = targetList._memoryBlocks;
			Span<T> targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex], targetList._nextItemIndex, targetList._blockSize - targetList._nextItemIndex);
			
			while (itemsSpan.Length >= targetBlockArraySpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				targetBlockIndex++;
				if (itemsSpan.Length == 0)
				{
					break;
				}

				targetBlockArraySpan = targetMemoryBlocksSpan[targetBlockIndex];
			}

			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(targetBlockArraySpan);
				targetList._lastBlockWithData = targetBlockIndex;
				if (targetList._nextItemBlockIndex != targetBlockIndex)
				{
					targetList._nextItemIndex = itemsSpan.Length;
					targetList._nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					targetList._nextItemIndex += itemsSpan.Length;
				}
			}
			else
			{
				targetList._lastBlockWithData = targetBlockIndex - 1;
				targetList._nextItemBlockIndex = targetBlockIndex;
				targetList._nextItemIndex = 0;
			}

			targetList._longCount = targetCapacity;
			targetList._version++;
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, RecyclableLongList<T> items)
		{
			if (items._longCount == 0)
			{
				return;
			}

			int sourceBlockIndex = 0,
				targetBlockIndex = targetList._nextItemBlockIndex,
				toCopy;

			long copiedCount = 0,
				itemsCount = items._longCount,
				targetCapacity = targetList._longCount + itemsCount;

			if (targetList._capacity < targetCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			ReadOnlySpan<T[]> sourceMemoryBlocksSpan = items._memoryBlocks;
			ReadOnlySpan<T> itemsSpan = sourceMemoryBlocksSpan[sourceBlockIndex];
			ReadOnlySpan<T[]> targetMemoryBlocksSpan = targetList._memoryBlocks;
			Span<T> targetBlockArraySpan = new(targetMemoryBlocksSpan[targetBlockIndex], targetList._nextItemIndex, targetList._blockSize - targetList._nextItemIndex);
			
			while (copiedCount < itemsCount)
			{
				toCopy = (int)Math.Min(itemsCount - copiedCount, itemsSpan.Length);
				if (targetBlockArraySpan.Length < toCopy)
				{
					toCopy = targetBlockArraySpan.Length;
					itemsSpan[..toCopy].CopyTo(targetBlockArraySpan);
					targetBlockIndex++;
					targetBlockArraySpan = targetMemoryBlocksSpan[targetBlockIndex];
					itemsSpan = itemsSpan[toCopy..];
					copiedCount += toCopy;
				}
				else
				{
					itemsSpan[..toCopy].CopyTo(targetBlockArraySpan);
					targetBlockArraySpan = targetBlockArraySpan[toCopy..];
					itemsSpan = itemsSpan[toCopy..];
					if (itemsSpan.IsEmpty && sourceBlockIndex + 1 < sourceMemoryBlocksSpan.Length)
					{
						sourceBlockIndex++;
						itemsSpan = sourceMemoryBlocksSpan[sourceBlockIndex];
					}

					copiedCount += toCopy;
					if (targetBlockArraySpan.IsEmpty && copiedCount < itemsCount)
					{
						targetBlockIndex++;
						targetBlockArraySpan = targetMemoryBlocksSpan[targetBlockIndex];
					}
				}
			}

			targetList._lastBlockWithData = targetBlockIndex;
			if ((targetCapacity & targetList._blockSizeMinus1) == 0)
			{
				targetList._nextItemIndex = 0;
				targetList._nextItemBlockIndex = targetBlockIndex + 1;
			}
			else
			{
				targetList._nextItemIndex = (int)(targetCapacity & targetList._blockSizeMinus1);
				targetList._nextItemBlockIndex = targetBlockIndex;
			}

			targetList._longCount = targetCapacity;
			targetList._version++;
		}

		public static void AddRange<T>(this RecyclableLongList<T> targetList, Span<T> items)
		{
			if (items.Length == 0)
			{
				return;
			}

			int targetBlockIndex = targetList._nextItemBlockIndex;			
			long targetCapacity = targetList._longCount + items.Length;

			if (targetList._capacity < targetCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], targetList._nextItemIndex, targetList._blockSize - targetList._nextItemIndex);

			while (items.Length >= targetBlockArraySpan.Length)
			{
				items[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				items = items[targetBlockArraySpan.Length..];
				if (items.Length == 0)
				{
					break;
				}

				targetBlockIndex++;
				targetBlockArraySpan = memoryBlocksSpan[targetBlockIndex];
			}

			if (items.Length > 0)
			{
				items.CopyTo(targetBlockArraySpan);
				if (targetList._nextItemBlockIndex != targetBlockIndex)
				{
					targetList._nextItemIndex = items.Length;
					targetList._nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					targetList._nextItemIndex += items.Length;
				}
			}
			else
			{
				targetList._nextItemBlockIndex = targetBlockIndex + 1;
				targetList._nextItemIndex = 0;
			}

			targetList._longCount = targetCapacity;
			targetList._lastBlockWithData = targetBlockIndex;
			targetList._version++;
		}

                public static void AddRange<T>(this RecyclableLongList<T> targetList, in T[] items)
                {
                        if (items.LongLength == 0)
                        {
                                return;
                        }

			int targetBlockIndex = targetList._nextItemBlockIndex;

			long targetCapacity = targetList._longCount + items.LongLength;
			if (targetList._capacity < targetCapacity)
			{
				targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
			}

			ReadOnlySpan<T> itemsSpan = items;
			ReadOnlySpan<T[]> memoryBlocksSpan = targetList._memoryBlocks;
			Span<T> targetBlockArraySpan = new(memoryBlocksSpan[targetBlockIndex], targetList._nextItemIndex, targetList._blockSize - targetList._nextItemIndex);

			while (itemsSpan.Length >= targetBlockArraySpan.Length)
			{
				itemsSpan[..targetBlockArraySpan.Length].CopyTo(targetBlockArraySpan);
				itemsSpan = itemsSpan[targetBlockArraySpan.Length..];
				if (itemsSpan.Length == 0)
				{
					break;
				}

				targetBlockIndex++;
				targetBlockArraySpan = memoryBlocksSpan[targetBlockIndex];
			}

			if (itemsSpan.Length > 0)
			{
				itemsSpan.CopyTo(targetBlockArraySpan);
				if (targetList._nextItemBlockIndex != targetBlockIndex)
				{
					targetList._nextItemIndex = itemsSpan.Length;
					targetList._nextItemBlockIndex = targetBlockIndex;
				}
				else
				{
					targetList._nextItemIndex += itemsSpan.Length;
				}
			}
			else
			{
				targetList._nextItemBlockIndex = targetBlockIndex + 1;
				targetList._nextItemIndex = 0;
			}

			targetList._longCount = targetCapacity;
                        targetList._lastBlockWithData = targetBlockIndex;
                        targetList._version++;
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableLongList<T> targetList, RecyclableHashSet<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        long sourceCount = items._count,
                                targetCapacity = targetList._longCount + sourceCount;

                        if (targetList._capacity < targetCapacity)
                        {
                                targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
                        }

                        var blocks = targetList._memoryBlocks;
                        int blockIndex = targetList._nextItemBlockIndex,
                                itemIndex = targetList._nextItemIndex,
                                blockSize = targetList._blockSize;

                        var entries = items._entries;
                        int shift = items._blockShift,
                                mask = items._blockSizeMinus1;

                        for (long i = 0; i < sourceCount; i++)
                        {
                                blocks[blockIndex][itemIndex++] = entries[i >> shift][(int)(i & mask)].Value;
                                if (itemIndex == blockSize)
                                {
                                        itemIndex = 0;
                                        blockIndex++;
                                }
                        }

                        targetList._longCount = targetCapacity;
                        targetList._nextItemBlockIndex = blockIndex;
                        targetList._nextItemIndex = itemIndex;
                        targetList._lastBlockWithData = blockIndex - (itemIndex > 0 ? 0 : 1);
                        targetList._version++;
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableLongList<T> targetList, RecyclableStack<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        long sourceCount = items._count,
                                targetCapacity = targetList._longCount + sourceCount;

                        if (targetList._capacity < targetCapacity)
                        {
                                targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
                        }

                        var blocks = targetList._memoryBlocks;
                        int blockIndex = targetList._nextItemBlockIndex,
                                itemIndex = targetList._nextItemIndex,
                                blockSize = targetList._blockSize;

                        var chunk = items._current;
                        while (chunk != null)
                        {
                                var array = chunk.Value;
                                for (int i = chunk.Index - 1; i >= 0; i--)
                                {
                                        blocks[blockIndex][itemIndex++] = array[i];
                                        if (itemIndex == blockSize)
                                        {
                                                itemIndex = 0;
                                                blockIndex++;
                                        }
                                }

                                chunk = chunk.Previous;
                        }

                        targetList._longCount = targetCapacity;
                        targetList._nextItemBlockIndex = blockIndex;
                        targetList._nextItemIndex = itemIndex;
                        targetList._lastBlockWithData = blockIndex - (itemIndex > 0 ? 0 : 1);
                        targetList._version++;
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableLongList<T> targetList, RecyclableSortedSet<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        long sourceCount = items._count,
                                targetCapacity = targetList._longCount + sourceCount;

                        if (targetList._capacity < targetCapacity)
                        {
                                targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
                        }

                        var blocks = targetList._memoryBlocks;
                        int blockIndex = targetList._nextItemBlockIndex,
                                itemIndex = targetList._nextItemIndex,
                                blockSize = targetList._blockSize;

                        int idx = 0;
                        while (idx < items._count)
                        {
                                blocks[blockIndex][itemIndex++] = items._items[idx++];
                                if (itemIndex == blockSize)
                                {
                                        itemIndex = 0;
                                        blockIndex++;
                                }
                        }

                        targetList._longCount = targetCapacity;
                        targetList._nextItemBlockIndex = blockIndex;
                        targetList._nextItemIndex = itemIndex;
                        targetList._lastBlockWithData = blockIndex - (itemIndex > 0 ? 0 : 1);
                        targetList._version++;
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableLongList<T> targetList, RecyclableLinkedList<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        long sourceCount = items._count,
                                targetCapacity = targetList._longCount + sourceCount;

                        if (targetList._capacity < targetCapacity)
                        {
                                targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
                        }

                        var blocks = targetList._memoryBlocks;
                        int blockIndex = targetList._nextItemBlockIndex,
                                itemIndex = targetList._nextItemIndex,
                                blockSize = targetList._blockSize;

                        var chunk = items._head;
                        while (chunk != null)
                        {
                                for (int i = chunk.Bottom; i < chunk.Top; i++)
                                {
                                        blocks[blockIndex][itemIndex++] = chunk.Value[i];
                                        if (itemIndex == blockSize)
                                        {
                                                itemIndex = 0;
                                                blockIndex++;
                                        }
                                }

                                chunk = chunk.Next;
                        }

                        targetList._longCount = targetCapacity;
                        targetList._nextItemBlockIndex = blockIndex;
                        targetList._nextItemIndex = itemIndex;
                        targetList._lastBlockWithData = blockIndex - (itemIndex > 0 ? 0 : 1);
                        targetList._version++;
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableLongList<T> targetList, RecyclablePriorityQueue<T> items)
                {
                        if (items._size == 0)
                        {
                                return;
                        }

                        long sourceCount = items._size,
                                targetCapacity = targetList._longCount + sourceCount;

                        if (targetList._capacity < targetCapacity)
                        {
                                targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
                        }

                        var blocks = targetList._memoryBlocks;
                        int blockIndex = targetList._nextItemBlockIndex,
                                itemIndex = targetList._nextItemIndex,
                                blockSize = targetList._blockSize;

                        int idx = 0;
                        while (idx < items._size)
                        {
                                blocks[blockIndex][itemIndex++] = items._heap[idx++];
                                if (itemIndex == blockSize)
                                {
                                        itemIndex = 0;
                                        blockIndex++;
                                }
                        }

                        targetList._longCount = targetCapacity;
                        targetList._nextItemBlockIndex = blockIndex;
                        targetList._nextItemIndex = itemIndex;
                        targetList._lastBlockWithData = blockIndex - (itemIndex > 0 ? 0 : 1);
                        targetList._version++;
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableLongList<T> targetList, RecyclableQueue<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        long sourceCount = items._count,
                                targetCapacity = targetList._longCount + sourceCount;

                        if (targetList._capacity < targetCapacity)
                        {
                                targetList._capacity = RecyclableLongList<T>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
                        }

                        var blocks = targetList._memoryBlocks;
                        int blockIndex = targetList._nextItemBlockIndex,
                                itemIndex = targetList._nextItemIndex,
                                blockSize = targetList._blockSize;

                        var chunkQ = items._head;
                        while (chunkQ != null)
                        {
                                for (int i = chunkQ.Bottom; i < chunkQ.Top; i++)
                                {
                                        blocks[blockIndex][itemIndex++] = chunkQ.Value[i];
                                        if (itemIndex == blockSize)
                                        {
                                                itemIndex = 0;
                                                blockIndex++;
                                        }
                                }

                                chunkQ = chunkQ.Next;
                        }

                        targetList._longCount = targetCapacity;
                        targetList._nextItemBlockIndex = blockIndex;
                        targetList._nextItemIndex = itemIndex;
                        targetList._lastBlockWithData = blockIndex - (itemIndex > 0 ? 0 : 1);
                        targetList._version++;
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<TKey, TValue>(this RecyclableLongList<KeyValuePair<TKey, TValue>> targetList, RecyclableDictionary<TKey, TValue> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        long sourceCount = items._count,
                                targetCapacity = targetList._longCount + sourceCount;

                        if (targetList._capacity < targetCapacity)
                        {
                                targetList._capacity = RecyclableLongList<KeyValuePair<TKey, TValue>>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
                        }

                        var blocks = targetList._memoryBlocks;
                        int blockIndex = targetList._nextItemBlockIndex,
                                itemIndex = targetList._nextItemIndex,
                                blockSize = targetList._blockSize;

                        var entries = items._entries;
                        int shift = items._blockShift,
                                mask = items._blockSizeMinus1;

                        for (long i = 0; i < sourceCount; i++)
                        {
                                ref var entry = ref entries[i >> shift][(int)(i & mask)];
                                blocks[blockIndex][itemIndex++] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                                if (itemIndex == blockSize)
                                {
                                        itemIndex = 0;
                                        blockIndex++;
                                }
                        }

                        targetList._longCount = targetCapacity;
                        targetList._nextItemBlockIndex = blockIndex;
                        targetList._nextItemIndex = itemIndex;
                        targetList._lastBlockWithData = blockIndex - (itemIndex > 0 ? 0 : 1);
                        targetList._version++;
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<TKey, TValue>(this RecyclableLongList<(TKey Key, TValue Value)> targetList, RecyclableSortedList<TKey, TValue> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        long sourceCount = items._count,
                                targetCapacity = targetList._longCount + sourceCount;

                        if (targetList._capacity < targetCapacity)
                        {
                                targetList._capacity = RecyclableLongList<(TKey Key, TValue Value)>.Helpers.Resize(targetList, targetList._blockSize, targetList._blockSizePow2BitShift, checked((long)BitOperations.RoundUpToPowerOf2((ulong)targetCapacity)));
                        }

                        var blocks = targetList._memoryBlocks;
                        int blockIndex = targetList._nextItemBlockIndex,
                                itemIndex = targetList._nextItemIndex,
                                blockSize = targetList._blockSize;

                        int idx2 = 0;
                        while (idx2 < items._count)
                        {
                                blocks[blockIndex][itemIndex++] = (items._keys[idx2], items._values[idx2]);
                                idx2++;
                                if (itemIndex == blockSize)
                                {
                                        itemIndex = 0;
                                        blockIndex++;
                                }
                        }

                        targetList._longCount = targetCapacity;
                        targetList._nextItemBlockIndex = blockIndex;
                        targetList._nextItemIndex = itemIndex;
                        targetList._lastBlockWithData = blockIndex - (itemIndex > 0 ? 0 : 1);
                        targetList._version++;
                }
	}
}
