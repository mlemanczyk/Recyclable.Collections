using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Reflection;

namespace Recyclable.Collections
{
        public static class zRecyclableListAddRange
        {

                private static class AddRangeHelper<T>
                {
                        private static readonly Action<RecyclableList<T>, object>? _dictionaryAdder;
                        private static readonly Action<RecyclableList<T>, object>? _sortedListAdder;
                        private static readonly Type? _dictionaryType;
                        private static readonly Type? _sortedListType;

                        static AddRangeHelper()
                        {
                                Type elementType = typeof(T);
                                if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                                {
                                        Type[] args = elementType.GetGenericArguments();
                                        _dictionaryType = typeof(RecyclableDictionary<,>).MakeGenericType(args);
                                        MethodInfo? method = typeof(zRecyclableListAddRange).GetMethod(
                                                nameof(AddRange),
                                                BindingFlags.NonPublic | BindingFlags.Static,
                                                binder: null,
                                                types: new[] { typeof(RecyclableList<>).MakeGenericType(elementType), _dictionaryType },
                                                modifiers: null);
                                        if (method != null)
                                        {
                                                var listParam = Expression.Parameter(typeof(RecyclableList<T>));
                                                var objParam = Expression.Parameter(typeof(object));
                                                var call = Expression.Call(method, listParam, Expression.Convert(objParam, _dictionaryType));
                                                _dictionaryAdder = Expression.Lambda<Action<RecyclableList<T>, object>>(call, listParam, objParam).Compile();
                                        }
                                }
                                else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                                {
                                        Type[] args = elementType.GetGenericArguments();
                                        _sortedListType = typeof(RecyclableSortedList<,>).MakeGenericType(args);
                                        MethodInfo? method = typeof(zRecyclableListAddRange).GetMethod(
                                                nameof(AddRange),
                                                BindingFlags.NonPublic | BindingFlags.Static,
                                                binder: null,
                                                types: new[] { typeof(RecyclableList<>).MakeGenericType(elementType), _sortedListType },
                                                modifiers: null);
                                        if (method != null)
                                        {
                                                var listParam = Expression.Parameter(typeof(RecyclableList<T>));
                                                var objParam = Expression.Parameter(typeof(object));
                                                var call = Expression.Call(method, listParam, Expression.Convert(objParam, _sortedListType));
                                                _sortedListAdder = Expression.Lambda<Action<RecyclableList<T>, object>>(call, listParam, objParam).Compile();
                                        }
                                }
                        }

                        internal static bool TryAddRange(RecyclableList<T> list, IEnumerable<T> items)
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

                private static void AddRangeEnumerated<T>(this RecyclableList<T> list, IEnumerable<T> items, int growByCount)
                {
			var enumerator = items.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return;
			}

			int capacity = list._capacity,
				targetItemIndex = list._count,
				available = capacity - targetItemIndex,
				i;

			Span<T> targetSpan = list._memoryBlock;
			do
			{
				if (targetItemIndex + growByCount > capacity)
				{
					capacity = RecyclableListHelpers<T>.EnsureCapacity(list, targetItemIndex, checked((int)BitOperations.RoundUpToPowerOf2((uint)(targetItemIndex + growByCount))));
					targetSpan = list._memoryBlock;
					available = capacity - targetItemIndex;
				}

				for (i = 0; i < available; i++)
				{
					targetSpan[targetItemIndex++] = enumerator.Current;
					if (!enumerator.MoveNext())
					{
						break;
					}
				}
			}
			while (i >= available);

			list._capacity = capacity;
			list._count = targetItemIndex;
		}

		private static void AddRangeWithKnownCount<T>(this RecyclableList<T> list, IEnumerable<T> items, int currentItemsCount, int requiredAdditionalCapacity)
		{
			list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, currentItemsCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)(currentItemsCount + requiredAdditionalCapacity))));

			Span<T> targetSpan = list._memoryBlock;
			foreach (var item in items)
			{
				targetSpan[currentItemsCount++] = item;
			}

			list._count = currentItemsCount;
		}

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                [VersionedCollectionsGenerator.RewriteForVersioned]
                public static void AddRange<T>(this RecyclableList<T> list, in Array items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			Array.Copy(items, items.GetLowerBound(0), list._memoryBlock, list._count, items.Length);
			list._count += items.Length;
			#if WITH_VERSIONING
			list._version++;
			#endif
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, in T[] items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			new ReadOnlySpan<T>(items).CopyTo(new Span<T>(list._memoryBlock, list._count, items.Length));
			list._count += items.Length;
			#if WITH_VERSIONING
			list._version++;
			#endif
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, ReadOnlySpan<T> items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			items.CopyTo(new Span<T>(list._memoryBlock, list._count, items.Length));
			list._count += items.Length;
			#if WITH_VERSIONING
			list._version++;
			#endif
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, Span<T> items)
		{
			if (list._capacity < list._count + items.Length)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Length))));
			}

			items.CopyTo(new Span<T>(list._memoryBlock, list._count, items.Length));
			list._count += items.Length;
			#if WITH_VERSIONING
			list._version++;
			#endif
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, List<T> items)
		{
			if (list._capacity < list._count + items.Count)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Count))));
			}

			items.CopyTo(list._memoryBlock, list._count);
			list._count += items.Count;
			#if WITH_VERSIONING
			list._version++;
			#endif
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, ICollection items)
		{
			if (list._capacity < list._count + items.Count)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Count))));
			}

			items.CopyTo(list._memoryBlock, list._count);
			list._count += items.Count;
			#if WITH_VERSIONING
			list._version++;
			#endif
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, ICollection<T> items)
		{
			if (list._capacity < list._count + items.Count)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items.Count))));
			}

			items.CopyTo(list._memoryBlock, list._count);
			list._count += items.Count;
			#if WITH_VERSIONING
			list._version++;
			#endif
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, RecyclableList<T> items)
		{
			if (list._capacity < list._count + items._count)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + items._count))));
			}

			new ReadOnlySpan<T>(items._memoryBlock, 0, items._count).CopyTo(new Span<T>(list._memoryBlock, list._count, items._count));
			list._count += items._count;
			#if WITH_VERSIONING
			list._version++;
			#endif
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                public static void AddRange<T>(this RecyclableList<T> list, RecyclableLongList<T> items)
                {
                        if (items._longCount == 0)
                        {
                                return;
                        }

			int oldCount = list._count;
			long sourceItemsCount = items._longCount;
			if (oldCount + sourceItemsCount > int.MaxValue)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items in source and target table exceeds the maximum capacity of {nameof(RecyclableList<T>)}, equal {int.MaxValue}. Please consider using {nameof(RecyclableLongList<T>)}, instead");
			}

			int targetCapacity = oldCount + (int)sourceItemsCount;
			if (list._capacity < targetCapacity)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
			}

			// TODO: Avoid unnecessary range operator - pass as arguments instead.
			Span<T> targetSpan = new Span<T>(list._memoryBlock)[oldCount..],
				itemsSpan;

			int blockIndex,
				sourceBlockSize = items._blockSize,
				lastBlockIndex = (int)(sourceItemsCount >> items._blockSizePow2BitShift) - 1;

			for (blockIndex = 0; blockIndex <= lastBlockIndex; blockIndex++)
			{
				itemsSpan = new(items._memoryBlocks[blockIndex], 0, sourceBlockSize);
				itemsSpan.CopyTo(targetSpan);
				targetSpan = targetSpan[sourceBlockSize..];
			}

			if (blockIndex == 0)
			{
				itemsSpan = new(items._memoryBlocks[blockIndex], 0, (int)sourceItemsCount);
				itemsSpan.CopyTo(targetSpan);
			}
			else if (blockIndex <= items._lastBlockWithData)
			{
				itemsSpan = new(items._memoryBlocks[blockIndex], 0, items._nextItemIndex);
				itemsSpan.CopyTo(targetSpan);
			}

                        list._count = targetCapacity;
                        #if WITH_VERSIONING
                        list._version++;
                        #endif
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableList<T> list, RecyclableHashSet<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        int oldCount = list._count,
                                sourceCount = items._count,
                                targetCapacity = oldCount + sourceCount;

                        if (list._capacity < targetCapacity)
                        {
                                list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
                        }

                        Span<T> targetSpan = new(list._memoryBlock, oldCount, sourceCount);
                        var entries = items._entries;
                        int shift = items._blockShift,
                                mask = items._blockSizeMinus1;

                        for (int i = 0; i < sourceCount; i++)
                        {
                                targetSpan[i] = entries[i >> shift][i & mask].Value;
                        }

                        list._count = targetCapacity;
                        #if WITH_VERSIONING
                        list._version++;
                        #endif
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableList<T> list, RecyclableStack<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        if (items._count > int.MaxValue - list._count)
                        {
                                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), "The total number of items exceeds the capacity of RecyclableList.");
                        }

                        int oldCount = list._count,
                                sourceCount = checked((int)items._count),
                                targetCapacity = oldCount + sourceCount;

                        if (list._capacity < targetCapacity)
                        {
                                list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
                        }

                        Span<T> targetSpan = new(list._memoryBlock, oldCount, sourceCount);
                        int index = 0;
                        var chunk = items._current;
                        while (chunk != null)
                        {
                                var array = chunk.Value;
                                for (int i = chunk.Index - 1; i >= 0; i--)
                                {
                                        targetSpan[index++] = array[i];
                                }

                                chunk = chunk.Previous;
                        }

                        list._count = targetCapacity;
                        #if WITH_VERSIONING
                        list._version++;
                        #endif
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableList<T> list, RecyclableSortedSet<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        int oldCount = list._count,
                                sourceCount = items._count,
                                targetCapacity = oldCount + sourceCount;

                        if (list._capacity < targetCapacity)
                        {
                                list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
                        }

                        new ReadOnlySpan<T>(items._items, 0, sourceCount).CopyTo(new Span<T>(list._memoryBlock, oldCount, sourceCount));

                        list._count = targetCapacity;
                        #if WITH_VERSIONING
                        list._version++;
                        #endif
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableList<T> list, RecyclableLinkedList<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        if (items._count > int.MaxValue - list._count)
                        {
                                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), "The total number of items exceeds the capacity of RecyclableList.");
                        }

                        int oldCount = list._count,
                                sourceCount = checked((int)items._count),
                                targetCapacity = oldCount + sourceCount;

                        if (list._capacity < targetCapacity)
                        {
                                list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
                        }

                        Span<T> targetSpan = new(list._memoryBlock, oldCount, sourceCount);
                        int index = 0;
                        var chunk = items._head;
                        while (chunk != null)
                        {
                                for (int i = chunk.Bottom; i < chunk.Top; i++)
                                {
                                        targetSpan[index++] = chunk.Value[i];
                                }

                                chunk = chunk.Next;
                        }

                        list._count = targetCapacity;
                        #if WITH_VERSIONING
                        list._version++;
                        #endif
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableList<T> list, RecyclablePriorityQueue<T> items)
                {
                        if (items._size == 0)
                        {
                                return;
                        }

                        int oldCount = list._count,
                                sourceCount = items._size,
                                targetCapacity = oldCount + sourceCount;

                        if (list._capacity < targetCapacity)
                        {
                                list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
                        }

                        new ReadOnlySpan<T>(items._heap, 0, sourceCount).CopyTo(new Span<T>(list._memoryBlock, oldCount, sourceCount));

                        list._count = targetCapacity;
                        #if WITH_VERSIONING
                        list._version++;
                        #endif
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<T>(this RecyclableList<T> list, RecyclableQueue<T> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        if (items._count > int.MaxValue - list._count)
                        {
                                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), "The total number of items exceeds the capacity of RecyclableList.");
                        }

                        int oldCount = list._count,
                                sourceCount = checked((int)items._count),
                                targetCapacity = oldCount + sourceCount;

                        if (list._capacity < targetCapacity)
                        {
                                list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
                        }

                        Span<T> targetSpan = new(list._memoryBlock, oldCount, sourceCount);
                        int index = 0;
                        var chunk = items._head;
                        while (chunk != null)
                        {
                                for (int i = chunk.Bottom; i < chunk.Top; i++)
                                {
                                        targetSpan[index++] = chunk.Value[i];
                                }

                                chunk = chunk.Next;
                        }

                        list._count = targetCapacity;
                        #if WITH_VERSIONING
                        list._version++;
                        #endif
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<TKey, TValue>(this RecyclableList<KeyValuePair<TKey, TValue>> list, RecyclableDictionary<TKey, TValue> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        int oldCount = list._count,
                                sourceCount = items._count,
                                targetCapacity = oldCount + sourceCount;

                        if (list._capacity < targetCapacity)
                        {
                                list._capacity = RecyclableListHelpers<KeyValuePair<TKey, TValue>>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
                        }

                        Span<KeyValuePair<TKey, TValue>> targetSpan = new(list._memoryBlock, oldCount, sourceCount);
                        var entries = items._entries;
                        int shift = items._blockShift,
                                mask = items._blockSizeMinus1;

                        for (int i = 0; i < sourceCount; i++)
                        {
                                ref var entry = ref entries[i >> shift][i & mask];
                                targetSpan[i] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                        }

                        list._count = targetCapacity;
                        #if WITH_VERSIONING
                        list._version++;
                        #endif
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
                internal static void AddRange<TKey, TValue>(this RecyclableList<(TKey Key, TValue Value)> list, RecyclableSortedList<TKey, TValue> items)
                {
                        if (items._count == 0)
                        {
                                return;
                        }

                        int oldCount = list._count,
                                sourceCount = items._count,
                                targetCapacity = oldCount + sourceCount;

                        if (list._capacity < targetCapacity)
                        {
                                list._capacity = RecyclableListHelpers<(TKey Key, TValue Value)>.EnsureCapacity(list, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
                        }

                        Span<(TKey Key, TValue Value)> targetSpan = new(list._memoryBlock, oldCount, sourceCount);
                        for (int i = 0; i < sourceCount; i++)
                        {
                                targetSpan[i] = (items._keys[i], items._values[i]);
                        }

                        list._count = targetCapacity;
                        #if WITH_VERSIONING
                        list._version++;
                        #endif
                }

		public static IEnumerator AddRange<T>(this RecyclableList<T> list, IEnumerable source, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			int targetItemIndex = list._count;
			Span<T> targetSpan;

			int i;
			var enumerator = source.GetEnumerator();

			int capacity = list._capacity;
			targetSpan = list._memoryBlock;
			if (enumerator.MoveNext())
			{
				int available = capacity - targetItemIndex;
				do
				{
					if (targetItemIndex + growByCount > capacity)
					{
						capacity = RecyclableListHelpers<T>.EnsureCapacity(list, targetItemIndex, checked((int)BitOperations.RoundUpToPowerOf2((uint)(targetItemIndex + growByCount))));
						targetSpan = list._memoryBlock;
						available = capacity - targetItemIndex;
					}

					for (i = 0; i < available; i++)
					{
						targetSpan[targetItemIndex++] = (T)enumerator.Current;
						if (!enumerator.MoveNext())
						{
							break;
						}
					}
				}
				while (i >= available);
			}

			list._capacity = capacity;
			list._count = targetItemIndex;
			return enumerator;
		}

		public static void AddRange<T>(this RecyclableList<T> list, IReadOnlyList<T> items)
		{
			int sourceItemsCount = items.Count;
			if (list._capacity < list._count + sourceItemsCount)
			{
				list._capacity = RecyclableListHelpers<T>.EnsureCapacity(list, list._count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(list._count + sourceItemsCount))));
			}

			Span<T> targetSpan = new(list._memoryBlock, list._count, sourceItemsCount);
			for (var sourceItemIndex = 0; sourceItemIndex < sourceItemsCount; sourceItemIndex++)
			{
				targetSpan[sourceItemIndex] = items[sourceItemIndex];
			}

			list._count += sourceItemsCount;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void AddRange<T>(this RecyclableList<T> list, IEnumerable<T> items, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			if (items is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(list, sourceRecyclableList);
			}
			else if (items is RecyclableLongList<T> sourceRecyclableLongList)
			{
				AddRange(list, sourceRecyclableLongList);
			}
			else if (items is T[] sourceArray)
			{
				AddRange(list, sourceArray);
			}
			else if (items is Array sourceArrayWithObjects)
			{
				AddRange(list, sourceArrayWithObjects);
			}
			else if (items is List<T> sourceList)
			{
				AddRange(list, sourceList);
			}
                        else if (items is ICollection<T> sourceICollection)
                        {
                                AddRange(list, sourceICollection);
                        }
                        else if (items is ICollection sourceICollectionWithObjects)
                        {
                                AddRange(list, sourceICollectionWithObjects);
                        }
                        else if (items is RecyclableHashSet<T> sourceHashSet)
                        {
                                AddRange(list, sourceHashSet);
                        }
                        else if (items is RecyclableStack<T> sourceStack)
                        {
                                AddRange(list, sourceStack);
                        }
                        else if (items is RecyclableSortedSet<T> sourceSortedSet)
                        {
                                AddRange(list, sourceSortedSet);
                        }
                        else if (items is RecyclableLinkedList<T> sourceLinkedList)
                        {
                                AddRange(list, sourceLinkedList);
                        }
                        else if (items is RecyclablePriorityQueue<T> sourcePriorityQueue)
                        {
                                AddRange(list, sourcePriorityQueue);
                        }
                        else if (items is RecyclableQueue<T> sourceQueue)
                        {
                                AddRange(list, sourceQueue);
                        }
                        else if (AddRangeHelper<T>.TryAddRange(list, items))
                        {
                        }
                        else if (items is IReadOnlyList<T> sourceIReadOnlyList)
                        {
                                AddRange(list, sourceIReadOnlyList);
                        }
			else if (items.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity) && requiredAdditionalCapacity != 0)
			{
				AddRangeWithKnownCount(list, items, list._count, requiredAdditionalCapacity);
				#if WITH_VERSIONING
				list._version++;
				#endif
			}
			else
			{
				AddRangeEnumerated(list, items, growByCount);
				#if WITH_VERSIONING
				list._version++;
				#endif
			}
		}
	}
}