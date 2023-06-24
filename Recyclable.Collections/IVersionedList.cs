namespace Recyclable.Collections
{
	public interface IVersionedList<T> : IEnumerable<T>
	{
		new RecyclableList<T>.VersionedEnumerator GetEnumerator();
	}
}
