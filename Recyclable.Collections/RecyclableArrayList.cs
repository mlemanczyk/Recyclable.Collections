using System.Buffers;
using System.Collections;

namespace Recyclable.Collections
{
	public class RecyclableArrayList<T> : IEnumerable<T>, IDisposable
	{
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;

		protected T[] _memory;

		protected static T[] SetNewLength(in T[]? source, in int newSize)
		{
			var sourceLength = source?.Length ?? 0;
			T[] newArray;
			newArray = newSize.RentArrayFromPool<T>();
			switch (newSize >= sourceLength)
			{
				case true:
					if (sourceLength > 0)
					{
						Memory<T> sourceMemory = new(source);
						Memory<T> newArrayMemory = new(newArray);
						sourceMemory.CopyTo(newArrayMemory);
						source!.ReturnToPool();
					}

					return newArray;

				case false:
					Span<T> sourceSpan = source.AsSpan();
					Span<T> newArraySpan = newArray.AsSpan();
					sourceSpan = sourceSpan[..newSize];
					sourceSpan.CopyTo(newArraySpan);
					source!.ReturnToPool();
					return newArray;
			}
		}

		protected ref int EnsureCapacity(in int requestedCapacity)
		{
			int newCapacity;
			switch (_capacity > 0)
			{
				case true:
					newCapacity = _capacity;
					while (newCapacity < requestedCapacity)
					{
						newCapacity *= 2;
					}

					break;

				case false:
					newCapacity = requestedCapacity;
					break;
			}

			_memory = SetNewLength(_memory, newCapacity);
			newCapacity = _memory.Length;
			_capacity = newCapacity;
			return ref _capacity;
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
			// _memory will be initialized when the 1st item is added
			if (initialCapacity > 0)
			{
				_memory = SetNewLength(_memory, initialCapacity);
				_capacity = initialCapacity;
			}
		}

		public RecyclableArrayList(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.Capacity)
		{
			_memory = SetNewLength(_memory, initialCapacity);
			_capacity = initialCapacity;

			foreach (var item in source)
			{
				Add(item);
			}
		}

		public T this[int index]
		{
			get => _memory[index];
			set => _memory[index] = value;
		}

		internal int _capacity;
		public int Capacity 
		{
			get => _capacity;
			set
			{
				if (_capacity != value)
				{
					_memory = SetNewLength(_memory, value);
					_capacity = value;
				}
			}
		}

		public int Count { get; set; }
		public bool IsReadOnly { get; } = false;

		public void Add(in T item)
		{
			int requestedCapacity = Count + 1;
			if (_capacity < requestedCapacity)
			{
				_ = EnsureCapacity(requestedCapacity);
			}

			_memory[Count] = item;
			Count++;
		}

		public void AddRange(in T[] items)
		{
			int sourceItemsCount = items.Length;
			int targetCapacity = Count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			Span<T> itemsSpan = items.AsSpan();
			for (int sourceItemIdx = 0, targetItemIdx = Count; sourceItemIdx < sourceItemsCount;)
			{
				_memory[targetItemIdx++] = itemsSpan[sourceItemIdx++];
			}

			Count = targetCapacity;
		}

		public void AddRange(in List<T> items)
		{
			var sourceItemsCount = items.Count;
			var targetCapacity = Count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			for (int sourceItemIdx = 0, targetItemIdx = Count; sourceItemIdx < sourceItemsCount;)
			{
				_memory[targetItemIdx++] = items[sourceItemIdx++];
			}

			Count = targetCapacity;
		}

		public void Clear()
		{
			Count = 0;
		}

		public bool Contains(T item) => _memory.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => _memory.CopyTo(array, arrayIndex);

		protected static IEnumerable<T> Enumerate(RecyclableArrayList<T> list)
		{
			for (var i = 0; i < list.Count; i++)
			{
				yield return list._memory[i];
			}
		}

		public int IndexOf(T itemToFind)
		{
			var memory = _memory;
			int itemCount = Count;
			var equalityComparer = _equalityComparer;
			for (var itemIdx = 0; itemIdx < itemCount; itemIdx++)
			{
				var item = memory[itemIdx];
				if (equalityComparer.Equals(item, itemToFind))
				{
					return itemIdx;
				}
			}

			return -1;
		}

		public void Insert(int index, T item)
		{
			int requestedCapacity = Count + 1;
			if (_capacity < requestedCapacity)
			{
				_ = EnsureCapacity(requestedCapacity);
			}

			for (var toMoveIdx = Count - 1; toMoveIdx >= index; toMoveIdx--)
			{
				_memory[toMoveIdx + 1] = _memory[toMoveIdx];
			}

			_memory[index] = item;
			Count++;
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
			int oldCount = Count;
			int oldCountMinus1 = oldCount - 1;
			if (index == oldCountMinus1)
			{
				Count--;
				return;
			}

			if (index >= oldCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			_memory.CopyItems(index, oldCountMinus1, ref _memory);
			Count--;
		}

		public IEnumerator<T> GetEnumerator() => Enumerate(this).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Enumerate(this).GetEnumerator();

		public void Dispose()
		{
			if (Capacity > 0)
			{
				Clear();
				Capacity = 0;
				_memory.ReturnToPool();
				GC.SuppressFinalize(this);
			}
		}
	}
}
