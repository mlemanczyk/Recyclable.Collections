﻿using System.Collections;

namespace Recyclable.CollectionsTests
{
	public class CustomIList<T> : IList<T>
	{
		protected List<T> _list;

		public CustomIList()
		{
			_list = new List<T>();
		}

		public CustomIList(IEnumerable<T> items)
		{
			_list = new List<T>(items);
		}

		public T this[int index] { get => _list[index]; set => _list[index] = value; }
		public int Count => _list.Count;
		public bool IsReadOnly => false;
		public void Add(T item) => _list.Add(item);
		public void Clear() => _list.Clear();
		public bool Contains(T item) => _list.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(0, array, arrayIndex, _list.Count);
		public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
		public int IndexOf(T item) => _list.IndexOf(item);
		public void Insert(int index, T item) => _list.Insert(index, item);
		public bool Remove(T item) => _list.Remove(item);
		public void RemoveAt(int index) => _list.RemoveAt(index);
		IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
	}
}