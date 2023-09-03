using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
	[Serializable]
	public sealed partial class RecyclableList<T> : IList<T>, IReadOnlyList<T>, IDisposable
	{
		private static readonly bool _needsClearing = !typeof(T).IsValueType;

		public static explicit operator ReadOnlySpan<T>(RecyclableList<T> source) => new(source._memoryBlock, 0, source._count);

#nullable disable
		internal T[] _memoryBlock;
#nullable restore

		internal ulong _version;
		public ulong Version => _version;

		private IEnumerator<T> AddRangeEnumerated(IEnumerable<T> source, int growByCount)
		{
			int targetItemIndex = _count;
			Span<T> memorySpan;

			int i;
			var enumerator = source.GetEnumerator();

			int capacity = _capacity;
			memorySpan = new(_memoryBlock);
			if (enumerator.MoveNext())
			{
				int available = capacity - targetItemIndex;
				do
				{
					if (targetItemIndex + growByCount > capacity)
					{
						capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, targetItemIndex, checked((int)BitOperations.RoundUpToPowerOf2((uint)(targetItemIndex + growByCount))));
						memorySpan = new(_memoryBlock);
						available = capacity - targetItemIndex;
					}

					for (i = 0; i < available; i++)
					{
						memorySpan[targetItemIndex++] = enumerator.Current;
						if (!enumerator.MoveNext())
						{
							break;
						}
					}
				}
				while (i >= available);
			}

			_capacity = capacity;
			_count = targetItemIndex;
			return enumerator;
		}

		private void AddRangeWithKnownCount(IEnumerable<T> source, int currentItemsCount, int requiredAdditionalCapacity)
		{
			_capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, currentItemsCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)(currentItemsCount + requiredAdditionalCapacity))));

			Span<T> memorySpan = new(_memoryBlock);
			foreach (var item in source)
			{
				memorySpan[currentItemsCount++] = item;
			}

			_count = currentItemsCount;
		}

		public RecyclableList()
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
		}

		public RecyclableList(int initialCapacity)
		{
			if (initialCapacity >= RecyclableDefaults.InitialCapacity)
			{
				_memoryBlock = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
					? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity)))
					: new T[initialCapacity];
				_capacity = _memoryBlock.Length;
			}
			else
			{
				_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
				_capacity = RecyclableDefaults.InitialCapacity;
			}
		}

		public RecyclableList(RecyclableList<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			AddRange(source);
		}

		public RecyclableList(RecyclableLongList<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			AddRange(source);
		}

		public RecyclableList(ReadOnlySpan<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			AddRange(source);
		}

		public RecyclableList(in T[] source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			AddRange(source);
		}

		public RecyclableList(List<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			AddRange(source);
		}

		public RecyclableList(IList<T> source)
		{
			_capacity = RecyclableDefaults.InitialCapacity;
			_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			AddRange(source);
		}

		public RecyclableList(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.InitialCapacity)
		{
			if (initialCapacity >= RecyclableDefaults.InitialCapacity)
			{
				_memoryBlock = initialCapacity >= RecyclableDefaults.MinPooledArrayLength
					? RecyclableArrayPool<T>.RentShared(checked((int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity)))
					: new T[initialCapacity];
				_capacity = _memoryBlock.Length;
			}
			else
			{
				_capacity = RecyclableDefaults.InitialCapacity;
				_memoryBlock = new T[RecyclableDefaults.InitialCapacity];
			}

			AddRange(source);
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _memoryBlock[index];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				_memoryBlock[index] = value;
				_version++;
			}
		}

		internal int _capacity;
		public int Capacity
		{
			get => _capacity;
			set
			{
				if (_capacity != value)
				{
					_capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, _count, checked((int)BitOperations.RoundUpToPowerOf2((uint)value)));
					_version++;
				}
			}
		}

		internal int _count;
		public int Count => _count;

		public bool IsReadOnly => false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			if (_count >= _capacity)
			{
				RecyclableListHelpers<T>.ResizeAndCopy(this);
			}

			_memoryBlock[_count++] = item;
			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(in T[] items)
		{
			if (_capacity < _count + items.Length)
			{
				_capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, _count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(_count + items.Length))));
			}

			new Span<T>(items).CopyTo(new Span<T>(_memoryBlock, _count, items.Length));
			_count += items.Length;
			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(ReadOnlySpan<T> items)
		{
			if (_capacity < _count + items.Length)
			{
				_capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, _count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(_count + items.Length))));
			}

			items.CopyTo(new Span<T>(_memoryBlock, _count, items.Length));
			_count += items.Length;
			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(List<T> items)
		{
			if (_capacity < _count + items.Count)
			{
				_capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, _count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(_count + items.Count))));
			}

			items.CopyTo(_memoryBlock, _count);
			_count += items.Count;
			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(IList<T> items)
		{
			if (_capacity < _count + items.Count)
			{
				_capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, _count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(_count + items.Count))));
			}

			items.CopyTo(_memoryBlock, _count);
			_count += items.Count;
			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(RecyclableList<T> items)
		{
			if (_capacity < _count + items._count)
			{
				_capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, _count, checked((int)BitOperations.RoundUpToPowerOf2((uint)(_count + items._count))));
			}

			new Span<T>(items._memoryBlock, 0, items._count).CopyTo(new Span<T>(_memoryBlock, _count, items._count));
			_count += items._count;
			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(RecyclableLongList<T> items)
		{
			if (items._longCount == 0)
			{
				return;
			}

			int oldCount = _count;
			long sourceItemsCount = items._longCount;
			if (sourceItemsCount > int.MaxValue)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The number of items exceeds the maximum capacity of {nameof(RecyclableList<T>)}, equal {int.MaxValue}, equal {int.MaxValue}. Please consider using {nameof(RecyclableLongList<T>)}, instead");
			}

			if (oldCount + sourceItemsCount > int.MaxValue)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(items), $"The total number of items in source and target table exceeds the maximum capacity of {nameof(RecyclableList<T>)}, equal {int.MaxValue}. Please consider using {nameof(RecyclableLongList<T>)}, instead");
			}

			int targetCapacity = oldCount + (int)sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)targetCapacity)));
			}

			// TODO: Avoid unnecessary range operator - pass as arguments instead.
			Span<T> targetSpan = new Span<T>(_memoryBlock)[oldCount..],
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

			_count = targetCapacity;
			_version++;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public void AddRange(IEnumerable<T> items, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			if (items is T[] sourceArray)
			{
				AddRange(sourceArray);
			}
			else if (items is List<T> sourceList)
			{
				AddRange(sourceList);
			}
			else if (items is RecyclableList<T> sourceRecyclableList)
			{
				AddRange(sourceRecyclableList);
			}
			else if (items is IList<T> sourceIList)
			{
				AddRange(sourceIList);
			}
			else if (items.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
			{
				AddRangeWithKnownCount(items, _count, requiredAdditionalCapacity);
				_version++;
			}
			else
			{
				AddRangeEnumerated(items, growByCount);
				_version++;
			}
		}

		public T[] AsArray => _memoryBlock;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Clear()
		{
			if (_count == 0)
			{
				return;
			}

			if (_needsClearing)
			{
				Array.Clear(_memoryBlock, 0, _count);
			}

			_count = 0;
			_version++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item) => _count > 0 && Array.IndexOf(_memoryBlock, item, 0, _count) >= 0;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void CopyTo(T[] array, int index) => Array.Copy(_memoryBlock, 0, array, index, _count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T itemToFind) => _count > 0 ? Array.IndexOf(_memoryBlock, itemToFind, 0, _count) : RecyclableDefaults.ItemNotFoundIndex;

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void Insert(int index, T item)
		{
			int oldCount = _count;
			if (_capacity < oldCount + 1)
			{
				_capacity = RecyclableListHelpers<T>.ResizeAndCopy(this, oldCount, checked((int)BitOperations.RoundUpToPowerOf2((uint)oldCount + 1)));
			}

			if (index < oldCount)
			{
				new Span<T>(_memoryBlock, index, oldCount)
					.CopyTo(new Span<T>(_memoryBlock, index + 1, oldCount));
			}

			_memoryBlock[index] = item;
			_count++;
			_version++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public bool Remove(T itemToRemove)
		{
			var index = IndexOf(itemToRemove);
			if (index >= 0)
			{
				_count--;
				if (index < _count)
				{
					Array.Copy(_memoryBlock, index + 1, _memoryBlock, index, _count - index);
				}

				if (_needsClearing)
				{
					_memoryBlock[_count] = default;
				}

				_version++;

				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void RemoveAt(int index)
		{
			if (index >= _count)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index), $"Provided value {index} exceeds the no. of items on the list {_count}");
			}

			_count--;
			if (index < _count)
			{
				Array.Copy(_memoryBlock, index + 1, _memoryBlock, index, _count - index);
			}

			if (_needsClearing)
			{
				_memoryBlock[_count] = default;
			}

			_version++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator GetEnumerator() => new(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

		public void Dispose()
		{
			_version++;
			if (_count != 0)
			{
				if (_needsClearing)
				{
					Array.Clear(_memoryBlock, 0, _count);
				}

				_count = 0;
			}

			if (_capacity >= RecyclableDefaults.MinPooledArrayLength)
			{
				_capacity = 0;
				// If anything, it has been already cleared by .Clear(), Remove() or RemoveAt() methods, as the list was modified / disposed.
				RecyclableArrayPool<T>.ReturnShared(_memoryBlock, false);
			}

			GC.SuppressFinalize(this);
		}
	}
}
