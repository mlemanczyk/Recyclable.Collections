namespace Recyclable.Collections
{
	internal static class ThrowHelper
	{
		/// <summary>
		/// This is a helper method to throw <see cref="InvalidOperationException"/>, thrown when the collection was modified during enumeration.
		/// </summary>
		/// <exception cref="InvalidOperationException">Always</exception>
		public static void ThrowListModifiedException() => throw new InvalidOperationException("Can't move to the next item, because the collection was modified. You must restart the enumeration by calling Reset(), if you want to enumerate the collection again");

		/// <summary>
		/// This is a helper method to throw <see cref="InvalidOperationException"/> with the given message.
		/// </summary>
		/// <exception cref="InvalidOperationException">Always</exception>
		public static void ThrowInvalidOperationException(in string message) => throw new InvalidOperationException(message);

		/// <summary>
		/// This is a helper method to throw <see cref="ArgumentOutOfRangeException"/> with the given message.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Always</exception>
		public static void ThrowArgumentOutOfRangeException(in string argumentName, in string message) => throw new ArgumentOutOfRangeException(argumentName, message);
	}
}
