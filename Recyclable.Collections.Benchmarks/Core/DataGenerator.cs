namespace Recyclable.Collections.Benchmarks.Core
{
    public static class DataGenerator
    {
		public static long[] EnumerateTestObjects(long testObjectCount)
		{
			var result = new long[testObjectCount];
			Console.WriteLine("******* TEST DATA ARRAY CREATED *******");

			Span<long> resultSpan = result;
			for (int i = 0; i < testObjectCount; i++)
			{
				resultSpan[i] = i;
			}

			return result;
		}
    }
}