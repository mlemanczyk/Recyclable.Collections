namespace Recyclable.Collections
{
	public interface IRecyclableOwner<T> : IList<T>
	{
		T this[long index] { get; set; }
		int BlockSize { get; }
		long Capacity { get; set; }
		long LongCount { get; set; }
	}
}
