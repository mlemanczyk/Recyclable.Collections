namespace Recyclable.Collections
{
	internal sealed class MemoryBucket<T>
	{
		public MemoryBucket(T[] memory, MemoryBucket<T> nextBucket)
		{
			_memory = memory;
			_nextBucket = nextBucket;
		}

		public MemoryBucket(T[] memory)
		{
			_memory = memory;
		}

#pragma warning disable IDE0032 // This is intentional due to better performance
		private T[] _memory;
		public T[] Memory { get => _memory; set => _memory = value; }

		private MemoryBucket<T>? _nextBucket;
#pragma warning restore IDE0032 // Use auto property
		public MemoryBucket<T>? NextBucket { get => _nextBucket; set => _nextBucket = value; }
	}
}
