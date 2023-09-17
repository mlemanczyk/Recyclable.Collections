namespace Recyclable.Collections
{
	internal sealed class MemoryBucket<T>
	{
		public MemoryBucket(Array memory, MemoryBucket<T> nextBucket)
		{
			_memory = memory;
			_nextBucket = nextBucket;
		}

		public MemoryBucket(Array memory)
		{
			_memory = memory;
		}

		private Array _memory;
		public Array Memory { get => _memory; set => _memory = value; }

		private MemoryBucket<T>? _nextBucket;
		public MemoryBucket<T>? NextBucket { get => _nextBucket; set => _nextBucket = value; }
	}
}
