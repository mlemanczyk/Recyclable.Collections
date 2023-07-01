namespace Recyclable.Collections
{
	internal class MemoryBucket<T>
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

#pragma warning disable RCS1085
		private T[] _memory;
		public T[] Memory { get => _memory; set => _memory = value; }

		private MemoryBucket<T>? _nextBucket;
		public MemoryBucket<T>? NextBucket { get => _nextBucket; set => _nextBucket = value; }
#pragma warning restore RCS1085
	}
}
