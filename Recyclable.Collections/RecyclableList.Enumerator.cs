using System.Collections;

namespace Recyclable.Collections
{
	public class RecyclableListEnumerator<T> : IEnumerator<T>
	{
		private static void ThrowListModifiedException() => throw new InvalidOperationException("Can't move to the next item, because the collection was modified. You must restart the enumeration by calling Reset(), if you want to enumerate the collection again");

		private int _currentIndex = RecyclableDefaults.ItemNotFoundIndex;
		private RecyclableList<T>? _list;
		private RecyclableCollectionVersion? _listVersion;
		private ulong _version;

		private T? _current;

		public RecyclableListEnumerator(RecyclableList<T> list)
		{
			_list = list;
			_listVersion = list._version;
			_listVersion!.AddEnumerator();
			_version = _listVersion!.Version;
		}

		public T Current => _current ?? throw new InvalidOperationException("The enumerator wasn't initialized. You need to call MoveNext() before getting the current iterator value");
		object? IEnumerator.Current => _current;

		public void Dispose()
		{
			if (_listVersion != null)
			{
				_listVersion.RemoveEnumerator();
				_listVersion = null;
			}

			if (_list != null)
			{
				_list = null;
			}

			GC.SuppressFinalize(this);
		}

		public bool MoveNext()
		{
			if (_list != null)
			{
				if (_list._version!.Version != _version)
				{
					ThrowListModifiedException();
					return false;
				}
				else if (_currentIndex < _list.Count - 1)
				{
					_currentIndex++;
					_current = _list[_currentIndex];
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public void Reset()
		{
			_currentIndex = RecyclableDefaults.ItemNotFoundIndex;
			_current = default;
			if (_list != null)
			{
				_version = _list._version!.Version;
			}
		}
	}
}
