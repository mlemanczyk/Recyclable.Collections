namespace Recyclable.Collections
{
	public interface IVersionedLongList<T> : IEnumerable<T>
	{
		new RecyclableLongList<T>.VersionedEnumerator GetEnumerator();
	}
}
