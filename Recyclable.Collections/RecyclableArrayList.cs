using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableArrayList<T> : IEnumerable<T>, IDisposable
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
			var sourceLength = source?.Length ?? 0;
			T[] newArray;
			newArray = newSize.RentArrayFromPool(_arrayPool);
			switch (newSize >= sourceLength)
			{
				case true:
					if (sourceLength > 0)
					{
						Memory<T> sourceMemory = new(source);
						Memory<T> newArrayMemory = new(newArray);
						sourceMemory.CopyTo(newArrayMemory);
						source!.ReturnToPool(_arrayPool);
					}

					return newArray;

				case false:
					Span<T> sourceSpan = new(source);
					Span<T> newArraySpan = new(newArray);
					sourceSpan = sourceSpan[..newSize];
					sourceSpan.CopyTo(newArraySpan);
					source!.ReturnToPool(_arrayPool);
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
			// _memory will be initialized when the 1st item is added
			if (initialCapacity > 0)
			{
				_memory = SetNewLength(_memory, initialCapacity);
				_capacity = _memory.Length;
			}
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
					_capacity = _memory.Length;
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

			_memory[Count++] = item;
		}

		public void AddRange(in T[] items)
		{
			int sourceItemsCount = items.Length;
			int targetCapacity = Count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			Span<T> itemsSpan = new(items);
			Span<T> targetSpan = new(_memory);
			for (int sourceItemIdx = 0, targetItemIdx = Count; sourceItemIdx < sourceItemsCount;)
			{
				targetSpan[targetItemIdx++] = itemsSpan[sourceItemIdx++];
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

			Span<T> targetSpan = new(_memory);
			for (int sourceItemIdx = 0, targetItemIdx = Count; sourceItemIdx < sourceItemsCount;)
			{
				targetSpan[targetItemIdx++] = items[sourceItemIdx++];
			}

			Count = targetCapacity;
		}

		public void AddRange(IEnumerable<T> source)
		{
			foreach (var item in source)
			{
				Add(item);
			}
		}

		public void Clear()
		{
			Count = 0;
		}

		public bool Contains(T item) => _memory.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => _memory.CopyTo(array, arrayIndex);

		protected static IEnumerable<T> Enumerate(RecyclableArrayList<T> list)
		{
			int count = list.Count;
			var memory = list._memory;
			for (var i = 0; i < count; i++)
			{
				yield return memory[i];
			}
		}

		public int IndexOf(T itemToFind)
		{
			Span<T> memorySpan = _memory;
			int itemCount = Count;
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
			int requestedCapacity = Count + 1;
			if (_capacity < requestedCapacity)
			{
				_ = EnsureCapacity(requestedCapacity);
			}

			Span<T> memorySpan = new(_memory);
			for (var toMoveIdx = Count - 1; toMoveIdx >= index; toMoveIdx--)
			{
				memorySpan[toMoveIdx + 1] = memorySpan[toMoveIdx];
			}

			memorySpan[index] = item;
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
				ThrowArgumentOutOfRangeException();
				return;
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
				_memory.ReturnToPool(_arrayPool);
				GC.SuppressFinalize(this);
			}
		}
	}
}
