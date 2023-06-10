using System.Collections;

namespace Recyclable.CollectionsTests
{
	public class EnumerableWithoutCount<T> : IEnumerable<T>
	{
		private readonly IEnumerable<T> _testData;

		public EnumerableWithoutCount(IEnumerable<T> testData)
		{
			_testData = testData;
		}

		public IEnumerator<T> GetEnumerator() => _testData.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _testData.GetEnumerator();
	}
}