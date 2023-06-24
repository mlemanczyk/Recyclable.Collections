namespace Recyclable.Collections
{
	public interface IRecyclableVersionedList<T> : IEnumerable<T>
	{
		new RecyclableList<T>.VersionedEnumerator GetEnumerator();
	}
}
