namespace Recyclable.Collections
{
	internal static class ThrowHelper
	{
		/// <summary>
		/// This is a helper method to throw <see cref="ArgumentOutOfRangeException"/> with the given message.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Always</exception>
		public static void ThrowArgumentOutOfRangeException(string argumentName, string message) => throw new ArgumentOutOfRangeException(argumentName, message);
		public static void ThrowWrongReturnedArraySizeException(int returnedLength, int blockSize) => throw new InvalidOperationException($"The array size {returnedLength} doesn't match the size {blockSize} of the pool buffer and cannot be returned to this pool.");
		public static void ThrowArgumentOutOfRangeException_Index() => throw new ArgumentOutOfRangeException("index", "Parameter `index` has invalid value. It must be within the no. of elements in the collection");
		public static void ThrowArgumentOutOfRangeException_Count() => throw new ArgumentOutOfRangeException("index", "Parameter `count` has invalid value. Combined with index, it must be within the no. of elements in the collection");
	}
}
