namespace Recyclable.Collections
{
	public static class RecyclableDefaults
	{
		public const int BlockSize = 16_384;
		public const int Capacity = 1;
		public const long MinItemsCountForParallelization = 850_000;
		public const int MinPooledArrayLength = 128;
		public const int MaxPooledBlockSize = 2_147_483_591;
		public const long ItemNotFoundIndexLong = -1L;
		public const int ItemNotFoundIndex = -1;
	}
}
