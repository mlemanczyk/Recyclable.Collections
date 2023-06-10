using System.Runtime.ExceptionServices;

namespace Recyclable.Collections
{
	internal static class ExceptionExtensions
	{
		public static void CaptureAndRethrow(this Exception ex)
		{
			ExceptionDispatchInfo.Capture(ex).Throw();
		}
	}
}
