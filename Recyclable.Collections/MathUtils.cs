using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public static class MathUtils
	{
		private static readonly long[] Pow2 = new long[]
		{
			1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384,
			32768, 65536, 131072, 262144, 524288, 1048576, 2097152, 4194304,
			8388608, 16777216, 33554432, 67108864, 134217728, 268435456,
			536870912, 1073741824, 2147483648
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPow2(int value) => Array.BinarySearch(Pow2, value) >= 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetPow2Shift(int value) => Array.BinarySearch(Pow2, value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long DivMod(long number, int divider, out long remainder)
		{
			if (number >= divider << 4)
			{
				return Math.DivRem(number, divider, out remainder);
			}
			else
			{
				int result = 0;
				remainder = number;
				while (remainder >= divider)
				{
					remainder -= divider;
					result++;
				}

				return result;
			}
		}
	}
}
