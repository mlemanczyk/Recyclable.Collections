using System.Diagnostics.Contracts;

namespace Recyclable.Collections
{
	internal static class ThrowHelper
	{
		/// <summary>
		/// This is a helper method to throw <see cref="ArgumentOutOfRangeException"/> with the given message.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Always</exception>
		public static void ThrowArgumentOutOfRangeException(string argumentName, string message)
		{
			Contract.EnsuresOnThrow<ArgumentOutOfRangeException>(false);
			throw new ArgumentOutOfRangeException(argumentName, message);
		}

		public static void ThrowWrongReturnedArraySizeException(int returnedLength, int blockSize)
		{
			throw new InvalidOperationException($"The array size {returnedLength} doesn't match the size {blockSize} of the pool buffer and cannot be returned to this pool.");
		}
	}
}
