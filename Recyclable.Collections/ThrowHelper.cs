#pragma warning disable IDE0022, CA2208

namespace Recyclable.Collections
{
	internal static class ThrowHelper
	{
		public static void ThrowArgumentException(string argumentName) => throw new ArgumentException("Parameter has invalid value. ", argumentName);
		public static void ThrowArgumentException(string argumentName, Exception? innerException) => throw new ArgumentException("Parameter has invalid value. ", argumentName, innerException);
		public static void ThrowArgumentException(string argumentName, string message) => throw new ArgumentException(message, argumentName);
		public static void ThrowArgumentException(string argumentName, string message, Exception? innerException) => throw new ArgumentException(message, argumentName, innerException);
		public static void ThrowArgumentException_Index() => throw new ArgumentException("Parameter has invalid value", "index");
		public static void ThrowArgumentException_Index(Exception? innerException) => throw new ArgumentException("Parameter has invalid value", "index", innerException);
		public static void ThrowArgumentException_Count() => throw new ArgumentException("Parameter has invalid value", "count");
		public static void ThrowArgumentException_Count(Exception? innerException) => throw new ArgumentException("Parameter has invalid value", "count", innerException);
		public static void ThrowArgumentOutOfRangeException(string argumentName) => throw new ArgumentOutOfRangeException(argumentName);
		public static void ThrowArgumentOutOfRangeException(string argumentName, Exception? innerException) => throw new ArgumentOutOfRangeException($"Argument {argumentName} is out of range", innerException);
		public static void ThrowArgumentOutOfRangeException(string argumentName, string message) => throw new ArgumentOutOfRangeException(argumentName, message);
		public static void ThrowWrongReturnedArraySizeException(int returnedLength, int blockSize) => throw new InvalidOperationException($"The array size {returnedLength} doesn't match the size {blockSize} of the pool buffer and cannot be returned to this pool.");
		public static void ThrowArgumentOutOfRangeException_Index() => throw new ArgumentOutOfRangeException("index", "Parameter `index` has invalid value. It must be within the no. of elements in the collection");
		public static void ThrowArgumentOutOfRangeException_Index(Exception? innerException) => throw new ArgumentOutOfRangeException("Parameter `index` has invalid value. It must be within the no. of elements in the collection", innerException);
		public static void ThrowArgumentOutOfRangeException_Count() => throw new ArgumentOutOfRangeException("count", "Parameter `count` has invalid value. Combined with index, it must be within the no. of elements in the collection");
		public static void ThrowArgumentOutOfRangeException_Count(Exception? innerException) => throw new ArgumentOutOfRangeException("Parameter `count` has invalid value. Combined with index, it must be within the no. of elements in the collection", innerException);
	}
}
