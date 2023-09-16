using System.Numerics;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	internal static class MathUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPow2(long value) => (value != 0) && ((value & (value - 1)) == 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetPow2Shift(long value) => BitOperations.IsPow2(value) ? 63 - BitOperations.LeadingZeroCount((ulong)value) : -1;

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
