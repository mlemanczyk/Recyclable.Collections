using Microsoft.Extensions.ObjectPool;

namespace Recyclable.Collections
{
	public partial class RecyclableList<T>
	{
		//internal class RecyclableListEnumeratorPool
		//{
		//	private static readonly RecyclableListEnumeratorPool _shared = new();
		//	public static RecyclableListEnumeratorPool Shared => _shared;

		//	private readonly ObjectPool<RecyclableListEnumerator> _pool = new DefaultObjectPool<RecyclableListEnumerator>(new DefaultPooledObjectPolicy<RecyclableListEnumerator>());

		//	public RecyclableListEnumerator Get(RecyclableList<T> list) => _pool.Get().Reset(this, list);

		//	public void Return(RecyclableListEnumerator version) => _pool.Return(version);
		//}
	}
}