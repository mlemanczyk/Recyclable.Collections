using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Recyclable.Collections
{
	public partial class RecyclableList<T>
	{
		[StructLayout(LayoutKind.Auto)]
		public struct Enumerator : IEnumerator<T>
		{
			private int _currentIndex;
			private readonly ulong _enumeratorVersion;
			private readonly RecyclableList<T> _list;

			// & WAS SLOWER
			// private bool _isInValidState = false;
			// private bool _endOfCollectionReached;
			// private T _current;

#nullable disable
			// [Pure]
			public readonly T Current
			{
				get
				{
					if (_enumeratorVersion != _list._version)
					{
						throw new InvalidOperationException();
						// & WAS SLOWER
						// ThrowHelper.ThrowListModifiedException();
					}

					if (_currentIndex == 0 || _currentIndex > _list._count)
					{
						throw new IndexOutOfRangeException();
						// & WAS SLOWER
						// ThrowHelper.ThrowCollectionNotInitialized();// new IndexOutOfRangeException();// _current;
					}

					Contract.EndContractBlock();

					return _list._memoryBlock[_currentIndex - 1];// _current;
				}
			}

			// [Pure]
			object IEnumerator.Current
			{
				get
				{
					if (_enumeratorVersion != _list._version)
					{
						throw new InvalidOperationException();
						// & WAS SLOWER
						// ThrowHelper.ThrowListModifiedException();
					}

					if (_currentIndex == 0 || _currentIndex > _list._count)
					{
						throw new IndexOutOfRangeException();
						// & WAS SLOWER
						// ThrowHelper.ThrowCollectionNotInitialized();
					}

					Contract.EndContractBlock();

					return _list._memoryBlock[_currentIndex - 1];

					// & WAS SLOWER
					// return _current;
				}
			}

			// & WAS SLOWER
			// private readonly int _listCount;
			// private RecyclableCollectionVersion _listVersion;
			// private readonly T[] _memoryBlock;
			// private bool _isVersionChanged;
			// private void VersionChanged() => _isVersionChanged = true;
			// internal RecyclableListEnumeratorPool? _pool;
			// internal bool _returned;
			// private readonly int _listCountMinus1;

			// & WAS SLOWER
			// [MethodImpl(MethodImplOptions.AggressiveInlining)]
			// [MethodImpl(MethodImplOptions.NoInlining)]
			public bool MoveNext()
			{
				// & WAS SLOWER
				// var list = _list;
				if (_enumeratorVersion != _list._version)
				{
					throw new InvalidOperationException();
					// & WAS SLOWER
					// ThrowHelper.ThrowListModifiedException();
				}

				Contract.EndContractBlock();

				// & WAS SLOWER
				// if (_currentIndex == 0)
				// {
				// 	_isInValidState = true;
				// 	_currentIndex++;
				// 	return true;
				// }
				// else
				if (_currentIndex < _list._count)
				{
					// & WAS SLOWER
					// _current = _list._memoryBlock[_currentIndex];
					_currentIndex++;
					return true;
				}

				// & WAS SLOWER
				// else if (_currentIndex == _list._count)
				// {
					// & WAS SLOWER
					// This isn't needed in most use-cases, because the enumerator will get disposed, releasing the reference, if any.
					// _current = _defaultValue;
				// _currentIndex++;
				// 	return false;
				// }
				// else if (!_endOfCollectionReached)
				// {
				// 	// & WAS SLOWER
				// 	// This isn't needed in most use-cases, because the enumerator will get disposed, releasing the reference, if any.
				// 	// _current = _defaultValue;
				// 	// _currentIndex = int.MinValue;
				// 	_endOfCollectionReached = true;
				// 	return false;
				// }

				// & WAS SLOWER
				// else if (!_endOfCollectionReached)
				// {
				// 	_current = default;
				// 	_endOfCollectionReached = true;
				// 	return false;
				// }

				// & WAS SLOWER
				// _current = default;
				// _currentIndex = _list._count + 1;
				return false;
			}

			public Enumerator(RecyclableList<T> list)
			{
				_currentIndex = 0;
				_list = list;
				_enumeratorVersion = list._version;

				// & WAS SLOWER
				// _current = default;
				// _endOfCollectionReached = false;
				// _listCount = list._count;
				// _memoryBlock = list._memoryBlock;
				// _isVersionChanged = false;
				// _listVersion = list.Version;
				// _listVersion.AddEnumerator();
				// _enumeratorVersion = list._version.Version;
				// list._version.RegisterHandler(VersionChanged);
				// _enumeratorVersion = _listVersion.Version;
				// _endOfCollectionReached = false;
				// _listCountMinus1 = list._count - 1;
			}

			public readonly void Dispose()
			{
				// & WAS SLOWER
				//if (_listVersion != null)
				//{
					//_list._version.RemoveEnumerator();
					//_listVersion.RemoveEnumerator(this);
				//}

				// & WAS SLOWER
				// if (_pool != null)
				//{
				//	if (!_returned)
				//	{
				//		_returned = true;
				//		_pool?.Return(this);
				//	}
				//}
				//else
				//{
				//_memoryBlock = null;
				//GC.SuppressFinalize(this);
				//}
			}

			void IEnumerator.Reset()
			{
				_currentIndex = 0;

				// & WAS SLOWER
				// _endOfCollectionReached = false;
				// _isInValidState = false;
				// _current = default;
			}
		}
	}
#nullable restore
}
