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

		protected int _count;
		public int Count
		{
			get => _count;
			set => _count = value;
		}

		public bool IsReadOnly { get; } = false;

		public void Add(in T item)
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

			//for (int sourceItemIdx = 0, targetItemIdx = _count; sourceItemIdx < sourceItemsCount;)
			//{
			//	targetSpan[targetItemIdx++] = itemsSpan[sourceItemIdx++];
			//}

			_count = targetCapacity;
		}

		public void AddRange(in List<T> items)
		{
			var sourceItemsCount = items.Count;
			var targetCapacity = _count + sourceItemsCount;
			if (_capacity < targetCapacity)
			{
				_ = EnsureCapacity(targetCapacity);
			}

			items.CopyTo(_memory, _count);
			//Span<T> targetSpan = new(_memory);
			//for (int sourceItemIdx = 0, targetItemIdx = _count; sourceItemIdx < sourceItemsCount;)
			//{
			//	targetSpan[targetItemIdx++] = items[sourceItemIdx++];
			//}

			_count = targetCapacity;
		}

		public void AddRange(in RecyclableArrayList<T> items)
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
			//for (int sourceItemIdx = 0, targetItemIdx = _count; sourceItemIdx < sourceItemsCount;)
			//{
			//	targetSpan[targetItemIdx++] = itemsSpan[sourceItemIdx++];
			//}
			//
			_count = targetCapacity;
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
			Span<T> memorySpan = _memory;
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
			int requestedCapacity = _count + 1;
			if (_capacity < requestedCapacity)
			{
				_ = EnsureCapacity(requestedCapacity);
			}

			Span<T> memorySpan = new(_memory);
			for (var toMoveIdx = _count - 1; toMoveIdx >= index; toMoveIdx--)
			{
				memorySpan[toMoveIdx + 1] = memorySpan[toMoveIdx];
			}

			memorySpan[index] = item;
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
