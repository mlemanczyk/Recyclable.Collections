using Microsoft.Extensions.ObjectPool;

namespace Recyclable.Collections.Pools
{
	//public class RecyclableCollectionVersionPool
	//{
	//	private static readonly RecyclableCollectionVersionPool _shared = new();
	//	public static RecyclableCollectionVersionPool Shared => _shared;

	//	private readonly ObjectPool<RecyclableCollectionVersion> _pool = new DefaultObjectPool<RecyclableCollectionVersion>(new DefaultPooledObjectPolicy<RecyclableCollectionVersion>());

	//	public RecyclableCollectionVersion Get() => _pool.Get().Reset();
	//	public void Return(RecyclableCollectionVersion version) => _pool.Return(version);
	//}
}
