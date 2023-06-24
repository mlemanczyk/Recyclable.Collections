namespace Recyclable.Collections
{
	public interface IRecyclableVersionedLongList<T> : IEnumerable<T>
	{
		new RecyclableLongList<T>.VersionedEnumerator GetEnumerator();
	}
}
