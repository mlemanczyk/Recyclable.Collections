using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableArrayList<T> : IEnumerable<T>, IList<T>, IDisposable
	{
		private static readonly ArrayPool<T> _arrayPool = ArrayPool<T>.Create();
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;

		protected T[] _memory;

		private static void ThrowArgumentOutOfRangeException()
		{
			throw new ArgumentOutOfRangeException("index");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static T[] SetNewLength(in T[]? source, in int newSize)
		{
			ArrayPool<T> arrayPool = _arrayPool;
			var sourceLength = source?.Length ?? 0;
			T[] newArray = newSize.RentArrayFromPool(arrayPool);
			switch (newSize >= sourceLength)
			{
				case true:
					if (sourceLength > 0)
					{
						Memory<T> sourceMemory = new(source);
						Memory<T> newArrayMemory = new(newArray);
						sourceMemory.CopyTo(newArrayMemory);
						source!.ReturnToPool(arrayPool);
					}

					return newArray;

				case false:
					var sourceSpan = new Span<T>(source)[..newSize];
					Span<T> newArraySpan = new(newArray);
					sourceSpan.CopyTo(newArraySpan);
					source!.ReturnToPool(arrayPool);
					return newArray;
			}
		}

		protected int EnsureCapacity(in int requestedCapacity)
		{
			int oldCapacity = _capacity;
			ref T[] memory = ref _memory;

			int newCapacity;
			switch (oldCapacity > 0)
			{
				case true:
					newCapacity = oldCapacity;
					while (newCapacity < requestedCapacity)
					{
						newCapacity *= 2;
					}

					break;

				case false:
					newCapacity = requestedCapacity;
					break;
			}

			memory = SetNewLength(memory, newCapacity);
			newCapacity = memory.Length;
			_capacity = newCapacity;
			return newCapacity;
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList()
#pragma warning restore CS8618
		{
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(int initialCapacity)
#pragma warning restore CS8618
		{
			if (initialCapacity > 0)
			{
				_memory = SetNewLength(_memory, initialCapacity);
				_capacity = _memory.Length;
			}
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(RecyclableArrayList<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(in T[] source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(List<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

#pragma warning disable CS8618 // _memory will be initialized when the 1st item is added
		public RecyclableArrayList(IList<T> source)
#pragma warning restore CS8618
		{
			AddRange(source);
		}

		public RecyclableArrayList(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.Capacity)
		{
			_memory = SetNewLength(_memory, initialCapacity);
			_capacity = _memory.Length;
			AddRange(source);
		}

		public T this[int index]
		{
			get => _memory[index];
			set => new Span<T>(_memory)[index] = value;
		}

		protected int _capacity;
		public int Capacity 
		{
			get => _capacity;
			set
			{
				if (_capacity != value)
				{
					_memory = SetNewLength(_memory, value);
					_capacity = _memory.Length;
				}
			}
		}

		protected int _count;
		public int Count
		{
			get => _count;
			set => _count = value;
		}

		public bool IsReadOnly { get; } = false;

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			int requestedCapacity = _count + 1;
			if (_capacity < requestedCapacity)
			{
				_ = EnsureCapacity(requestedCapacity);
			}

			_memory[_count++] = item;
		}

		public void AddRange(in T[] items)
		{
			int sourceItemsCount = items.Length;
			int targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			var targetSpan = new Span<T>(_memory)[_count..];
			Span<T> itemsSpan = new(items);
			itemsSpan.CopyTo(targetSpan);
			_count = targetCapacity;
		}

		public void AddRange(List<T> items)
		{
			var sourceItemsCount = items.Count;
			var targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			items.CopyTo(_memory, _count);
			_count = targetCapacity;
		}

		public void AddRange(IList<T> items)
		{
			var sourceItemsCount = items.Count;
			var targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			items.CopyTo(_memory, _count);
			_count = targetCapacity;
		}

		public void AddRange(RecyclableArrayList<T> items)
		{
			var sourceItemsCount = items._count;
			var targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			var targetSpan = new Span<T>(_memory)[_count..];
			Span<T> itemsSpan = new(items._memory);
			itemsSpan.CopyTo(targetSpan);
			_count = targetCapacity;
		}

		public void AddRange(IEnumerable<T> source, int growByCount = RecyclableDefaults.MinPooledArrayLength)
		{
			if (source is T[] sourceArray)
			{
				AddRange(sourceArray);
				return;
			}

			if (source is List<T> sourceList)
			{
				AddRange(sourceList);
				return;
			}

			if (source is RecyclableArrayList<T> sourceRecyclableArrayList)
			{
				AddRange(sourceRecyclableArrayList);
				return;
			}

			if (source is IList<T> sourceIList)
			{
				AddRange(sourceIList);
				return;
			}

			int capacity = _capacity;
			int targetIndex = _count;
			Span<T> memorySpan;
			if (source.TryGetNonEnumeratedCount(out var requiredAdditionalCapacity))
			{
				_ = EnsureCapacity(targetIndex + requiredAdditionalCapacity);
				memorySpan = new(_memory);
				foreach (var item in source)
				{
					memorySpan[targetIndex++] = item;
				}

				_count = targetIndex;
				return;
			}

			int i;
			using var enumerator = source.GetEnumerator();

			memorySpan = new(_memory);
			if (enumerator.MoveNext())
			{
				int available = capacity - targetIndex;
				while (true)
				{
					if (targetIndex + growByCount > capacity)
					{
						capacity = EnsureCapacity(capacity + growByCount);
						memorySpan = new(_memory);
						available = capacity - targetIndex;
					}

					for (i = 0; i < available; i++)
					{
						memorySpan[targetIndex++] = enumerator.Current;
						if (!enumerator.MoveNext())
						{
							break;
						}
					}

					if (i < available)
					{
						break;
					}
				}
			}

			_count = targetIndex;
		}

		public void Clear()
		{
			_count = 0;
		}

		public bool Contains(T item) => _memory.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => _memory.CopyTo(array, arrayIndex);

		protected static IEnumerable<T> Enumerate(RecyclableArrayList<T> list)
		{
			int count = list._count;
			var memory = list._memory;
			for (var i = 0; i < count; i++)
			{
				yield return memory[i];
			}
		}

		public int IndexOf(T itemToFind)
		{
			Span<T> memorySpan = new(_memory);
			int itemCount = _count;
			var equalityComparer = _equalityComparer;
			for (var itemIdx = 0; itemIdx < itemCount; itemIdx++)
			{
				var item = memorySpan[itemIdx];
				if (equalityComparer.Equals(item, itemToFind))
				{
					return itemIdx;
				}
			}

			return -1;
		}

		public void Insert(int index, T item)
		{
			int oldCount = _count;
			int requestedCapacity = oldCount + 1;
			if (_capacity < requestedCapacity)
			{
				_ = EnsureCapacity(requestedCapacity);
			}

			if (oldCount > 0)
			{
				var sourceSpan = new Span<T>(_memory)[index..oldCount];
				var targetSpan = new Span<T>(_memory)[(index + 1)..];
				sourceSpan.CopyTo(targetSpan);
			}

			_memory[index] = item;
			_count++;
		}

		public bool Remove(T itemToRemove)
		{
			var itemIdx = IndexOf(itemToRemove);
			if (itemIdx >= 0)
			{
				RemoveAt(itemIdx);
				return true;
			}

			return false;
		}
		public void RemoveAt(int index)
		{
			int oldCount = _count;
			int oldCountMinus1 = oldCount - 1;
			if (index == oldCountMinus1)
			{
				_count--;
				return;
			}

			if (index >= oldCount)
			{
				ThrowArgumentOutOfRangeException();
				return;
			}

			_memory.CopyItems(index, oldCountMinus1, ref _memory);
			_count--;
		}

		public IEnumerator<T> GetEnumerator() => Enumerate(this).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Enumerate(this).GetEnumerator();

		public void Dispose()
		{
			if (_capacity > 0)
			{
				Clear();
				_capacity = 0;
				_memory.ReturnToPool(_arrayPool);
				GC.SuppressFinalize(this);
			}
		}
	}
}
