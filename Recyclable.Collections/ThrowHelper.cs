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
	}
}
