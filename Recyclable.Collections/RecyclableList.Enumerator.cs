using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace Recyclable.Collections
{
	public partial class RecyclableList<T>
	{

		public struct Enumerator : IEnumerator<T>, IEnumerator
		{
#nullable disable
			private readonly RecyclableList<T> _list;
			private int _currentIndex;
			private T _current;

			//[Pure]
			//private readonly int _listCount;
			//private readonly int _listCountMinus1;
			//private readonly T[] _memoryBlock;
			//private bool _endOfCollectionReached;

			//private RecyclableCollectionVersion _listVersion;
			//private ulong _enumeratorVersion;
			//private bool _isVersionChanged;

			//private void VersionChanged() => _isVersionChanged = true;

			//internal RecyclableListEnumeratorPool? _pool;
			//internal bool _returned;

			//public RecyclableListEnumerator()
			//{
			//}

			//[Pure]
			internal Enumerator(RecyclableList<T> list)
			{
				_current = default;
				_currentIndex = 0;
				_list = list;
				//_memoryBlock = list._memoryBlock;
				//_listCount = list._count;
				//_listVersion = list._version!;
				//_listVersion.VersionChanged += VersionChanged;
				//_enumeratorVersion = _listVersion.Version;
			}

			//{
			//	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			//	get =>
			//			//if (_currentIndex < 0)
			//			//{
			//			//	ThrowHelper.ThrowInvalidOperationException("The enumerator wasn't initialized. You need to call MoveNext() before getting the current iterator value");
			//			//}
			//			//else
			//			//if (_endOfCollectionReached)
			//			//{
			//			//	throw new InvalidOperationException("The enumerator reached the end of the collection. You need to call Reset(), if you want to enumerate the collection again");
			//			//}
			//			//else
			//			//{
			//			//if (_currentIndex < 0)
			//			//{
			//			//	ThrowHelper.ThrowInvalidOperationException("The enumerator wasn't initialized. You need to call MoveNext() before getting the current iterator value");
			//			//}
			//			//else
			//			_memoryBlock[_currentIndex];//}
			//}

			//[Pure]
			public readonly void Dispose()
			{
				//				if (_listVersion != null)
				//				{
				//					_listVersion.VersionChanged -= VersionChanged;
				//					//_listVersion.RemoveEnumerator(this);
				//#nullable disable
				//					_listVersion = null;
				//#nullable restore
				//				}

				//if (_pool != null)
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

			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				var list = _list;
				//if (_isVersionChanged)
				//{
				//	ThrowHelper.ThrowListModifiedException();
				//	return false;
				//}
				//else
				if (_currentIndex < list._count)
				{
					_current = list._memoryBlock[_currentIndex];
					_currentIndex++;
					return true;
				}

				//else if (!_endOfCollectionReached)
				//{
				//	_endOfCollectionReached = true;
				//	return false;
				//}

				_current = default;
				return false;
			}

#nullable restore
			public T Current => _current;
			object IEnumerator.Current => _current;
#nullable disable

			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IEnumerator.Reset()
			{
				_currentIndex = 0;
				_current = default;
			}

			//_endOfCollectionReached = false;
			//if (_isVersionChanged)
			//{
			//	
			//_enumeratorVersion = _listVersion!.Version;
			//	_isVersionChanged = false;
			//}

			//internal RecyclableListEnumerator Reset(RecyclableListEnumeratorPool pool, RecyclableList<T> list)
			//{
			//	_returned = false;
			//	if (_pool != pool)
			//	{
			//		_pool = pool;
			//	}

			//	_currentIndex = RecyclableDefaults.ItemNotFoundIndex;
			//	_listVersion = list._version!;
			//	_enumeratorVersion = _listVersion!.Version;
			//	list._version!.AddEnumerator();
			//	return this;
			//}

			//[Pure]
			//public object Clone() => MemberwiseClone();
		}
	}
#nullable restore
}
