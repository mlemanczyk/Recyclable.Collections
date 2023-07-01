using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	internal static class RecyclableQueueHelpers
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToArrayIndex(long index, int blockSize) => (int)(index / blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ToItemIndex(long index, int blockSize) => index % blockSize;
	}
}
