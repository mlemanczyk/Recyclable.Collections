using System.Collections;

namespace Recyclable.Collections
{
	public partial class RecyclableLongList<T>
	{
		// & WAS SLOWER
		// [StructLayout(LayoutKind.Sequential, Pack = 1)]
		// [StructLayout(LayoutKind.Sequential)]
		public struct Enumerator : IEnumerator<T>
		{
			// & WAS SLOWER WITHOUT
			#nullable disable
						private static readonly T _default = default;
			#nullable restore

			private static readonly bool _isReferenceType = !typeof(T).IsValueType;

			private int _currentBlockIndex;
			private int _currentItemIndex;
			private readonly RecyclableLongList<T> _list;
			// & WAS SLOWER WITHOUT
			#nullable disable
						private T _current;
			#nullable restore
			// & WAS SLOWER
			// private readonly ulong _enumeratorVersion;
			// public T Current => _enumeratorVersion != _list._version ? throw new InvalidOperationException() : _current;

			public readonly T Current => _current;
				// & WAS SLOWER
				// get => _currentItemIndex > 0
				// 		? _list._memoryBlocks[_currentBlockIndex][_currentItemIndex - 1]
				// 		: _list._memoryBlocks[_currentBlockIndex - 1][_list._blockSize - 1];
				// & WAS SLOWER
				// get => _currentItemIndex != RecyclableDefaults.BlockIndexLimit
				// 		? _currentItemIndex > 0
				// 		? _list._memoryBlocks[_currentBlockIndex][_currentItemIndex - 1]
				// & WAS SLOWER
				// 		: _list._memoryBlocks[_currentBlockIndex - 1][_list._blockSize - 1]
				// 		? _current
				// 		: throw new IndexOutOfRangeException();
			// & WAS SLOWER
			// {
			// get
			// {
			// if (_enumeratorVersion != _list._version)
			// {
			// 	throw new InvalidOperationException();
			// }

			// if (_list._longCount == 0 || _currentItemIndex == RecyclableDefaults.BlockIndexLimit)
			// {
			// 	throw new IndexOutOfRangeException();
			// }

			// Contract.EndContractBlock();

			// return _currentItemIndex > 0
			// 	? _list._memoryBlocks[_currentBlockIndex][_currentItemIndex - 1]
			// 	: _list._memoryBlocks[_currentBlockIndex - 1][_list._blockSize - 1];
			// }
			// }

			// & WAS SLOWER
			// [MethodImpl(MethodImplOptions.AggressiveInlining)]
			// [MethodImpl(MethodImplOptions.NoInlining)]
			// [MethodImpl(MethodImplOptions.AggressiveOptimization)]
			// [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
			public bool MoveNext()
			{
				// & WAS SLOWER
				// if (_enumeratorVersion != _list._version)
				// {
				// 	throw new InvalidOperationException();
				// }
				//
				// Contract.EndContractBlock();

				// & WAS SLOWER WITHOUT
				var list = _list;
				// & WAS SLOWER
				// ref var currentBlockIndex = ref _currentBlockIndex;
				// & WAS SLOWER
				// ref var currentItemIndex = ref _currentItemIndex;

				if (_currentBlockIndex < list._lastBlockWithData)
				{
					// & WAS SLOWER WITHOUT
					_current = list._memoryBlocks[_currentBlockIndex][_currentItemIndex];
					if (_currentItemIndex + 1 < list._blockSize)
					{
						_currentItemIndex++;
					}
					else
					{
						_currentBlockIndex++;
						_currentItemIndex = 0;
					}

					return true;
				}
				// & WAS SLOWER
				// else if (list._longCount == 0)
				// {
				// 	return false;
				// }
				// Move to the next item if we're within block boundaries, otherwise report end of collection.
				else if (list._longCount > 0 && _currentItemIndex < (list._nextItemIndex > 0 ? list._nextItemIndex : list._blockSize))
				{
					// & WAS SLOWER WITHOUT
					_current = list._memoryBlocks[_currentBlockIndex][_currentItemIndex];
					// & WAS SLOWER WITHOUT
					_currentItemIndex++;
					return true;
				}
				// & WAS SLOWER
				// else if (_currentItemIndex != RecyclableDefaults.BlockIndexLimit)
				// {
				// 	_currentItemIndex = RecyclableDefaults.BlockIndexLimit;
				// _current = _default;
				// }

				return false;
			}

#nullable disable
			readonly object IEnumerator.Current => _current;
				// & WAS SLOWER
				// get => _currentItemIndex > 0
				// 		? _list._memoryBlocks[_currentBlockIndex][_currentItemIndex - 1]
				// 		: (object)_list._memoryBlocks[_currentBlockIndex - 1][_list._blockSize - 1];
				// & WAS SLOWER
				// get => _currentItemIndex != RecyclableDefaults.BlockIndexLimit
				// 		// ? _currentItemIndex > 0
				// 		// ? _list._memoryBlocks[_currentBlockIndex][_currentItemIndex - 1]
				// 		// : _list._memoryBlocks[_currentBlockIndex - 1][_list._blockSize - 1]
				// 		// & WAS SLOWER
				// 		? _current
				// 		: throw new IndexOutOfRangeException();
#nullable restore
			// & WAS SLOWER
			// readonly object? IEnumerator.Current => _enumeratorVersion != _list._version ? throw new InvalidOperationException() : _current;
			// {
			// 	get
			// 	{
			// 		// if (_enumeratorVersion != _list._version)
			// 		// {
			// 		// 	throw new InvalidOperationException();
			// 		// }

			// 		// if (_list._longCount == 0 || _currentItemIndex == RecyclableDefaults.BlockIndexLimit)
			// 		// {
			// 		// 	throw new IndexOutOfRangeException();
			// 		// }

			// 		// Contract.EndContractBlock();

			// 		return _currentItemIndex > 0
			// 			? _list._memoryBlocks[_currentBlockIndex][_currentItemIndex - 1]
			// 			: _list._memoryBlocks[_currentBlockIndex - 1][_list._blockSize - 1];
			// 	}
			// }

			public Enumerator(RecyclableLongList<T> list)
			{
				// & WAS SLOWER WITHOUT
				_current = _default;
				_currentBlockIndex = 0;
				// & WAS SLOWER
				//_currentBlockIndex = -1;
				_currentItemIndex = 0;
				_list = list;
				// & WAS SLOWER
				// _enumeratorVersion = list._version;
			}

			public void Reset()
			{
				// & WAS SLOWER WITHOUT
				_current = _default;
				_currentBlockIndex = 0;
				// & WAS SLOWER
				//_currentBlockIndex = -1;
				_currentItemIndex = 0;
			}

			public void Dispose()
			{
				if (_isReferenceType)
				{
					_current = _default;
				}
			}
		}
	}
}
