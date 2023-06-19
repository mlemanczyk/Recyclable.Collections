using System.Collections;

namespace Recyclable.Collections
{
	public sealed class RecyclableLongListEnumerator<T> : IEnumerator<T>
	{
		public static void ThrowListModifiedException() => throw new InvalidOperationException("Can't move to the next item, because the collection was modified. You must restart the enumeration by calling Reset(), if you want to enumerate the collection again");

		private int _blockSize;
		private int _blockSizeMinus1;
		private int _currentBlockIndex = 0;
		private int _currentIndex = RecyclableDefaults.ItemNotFoundIndex;
		private int _lastBlockWithData;
		private RecyclableLongList<T>? _list;
		private RecyclableCollectionVersion? _listVersion;
		private T[][] _memoryBlocks;
		private int _nextItemBlockIndex;
		private int _nextItemIndex;
		//private ulong _version;
		private bool _isVersionChanged;
		private void VersionChanged() => _isVersionChanged = true;

		private T? _current;

		public RecyclableLongListEnumerator(RecyclableLongList<T> list)
		{
			_listVersion = list._version;
			list._version!.VersionChanged += VersionChanged;
			//_listVersion!.AddEnumerator();
			_blockSize = list._blockSize;
			_blockSizeMinus1 = list._blockSizeMinus1;
			_lastBlockWithData = list._lastBlockWithData;
			_list = list;
			//_version = _listVersion!.Version;
			_memoryBlocks = list._memoryBlocks;
			_nextItemBlockIndex = list._nextItemBlockIndex;
			_nextItemIndex = _lastBlockWithData == _nextItemBlockIndex ? _list._nextItemIndex : _blockSize;
		}

		public T Current => _current ?? throw new InvalidOperationException("The enumerator wasn't initialized. You need to call MoveNext() before getting the current iterator value");
		object? IEnumerator.Current => _current;

		public void Dispose()
		{
			if (_listVersion != null)
			{
				_listVersion.VersionChanged -= VersionChanged;
				//_listVersion.RemoveEnumerator();
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
				if (_isVersionChanged)
				{
					_current = default;
					ThrowListModifiedException();
					return false;
				}
				else if (_currentBlockIndex < _lastBlockWithData)
				{
					if (_currentIndex < _blockSizeMinus1)
					{
						_currentIndex++;
					}
					else
					{
						_currentIndex = 0;
						_currentBlockIndex++;
						if (_currentBlockIndex == _lastBlockWithData && _currentIndex >= _nextItemIndex)
						{
							_current = default;
							return false;
						}
					}

					_current = _memoryBlocks[_currentBlockIndex][_currentIndex];
					return true;
				}
				else if (_currentBlockIndex == _lastBlockWithData && !(_currentIndex >= _nextItemIndex - 1))
				{
					_currentIndex++;
					_current = _memoryBlocks[_currentBlockIndex][_currentIndex];
					return true;
				}
				else
				{
					_current = default;
					return false;
				}
			}
			else
			{
				_current = default;
				return false;
			}
		}

		public void Reset()
		{
			_currentBlockIndex = 0;
			_currentIndex = RecyclableDefaults.ItemNotFoundIndex;
			_current = default;
			if (_isVersionChanged)
			{
				//_blockSize = _list!._blockSize;
				//_blockSizeMinus1 = _list._blockSizeMinus1;
				_isVersionChanged = false;
				//_lastBlockWithData = _list!._lastBlockWithData;
				//_listVersion = _list._version;
				//_memoryBlocks = _list._memoryBlocks;
				//_nextItemBlockIndex = _list._nextItemBlockIndex;
				//_nextItemIndex = _lastBlockWithData == _nextItemBlockIndex ? _list._nextItemIndex : _blockSize;
				//_version = _listVersion!.Version;
			}
		}

		internal RecyclableLongListEnumerator<T> Init()
		{
			_currentBlockIndex = 0;
			_currentIndex = RecyclableDefaults.ItemNotFoundIndex;
			_current = default;
			if (_list != null)
			{
				_blockSize = _list._blockSize;
				_blockSizeMinus1 = _list._blockSizeMinus1;
				_isVersionChanged = false;
				_lastBlockWithData = _list._lastBlockWithData;
				_listVersion = _list._version;
				_memoryBlocks = _list._memoryBlocks;
				_nextItemBlockIndex = _list._nextItemBlockIndex;
				_nextItemIndex = _lastBlockWithData == _nextItemBlockIndex ? _list._nextItemIndex : _blockSize;
				//_version = _listVersion!.Version;
			}

			return this;
		}
	}
}
