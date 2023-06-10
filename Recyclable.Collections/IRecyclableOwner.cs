namespace Recyclable.Collections
{
	internal interface IRecyclableOwner<T> : IList<T>
	{
		T this[long index] { get; set; }
		int BlockSize { get; }
		long Capacity { get; set; }
		long LongCount { get; set; }
	}
}
