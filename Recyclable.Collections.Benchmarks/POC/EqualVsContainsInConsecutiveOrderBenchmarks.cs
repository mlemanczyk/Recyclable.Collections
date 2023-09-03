using BenchmarkDotNet.Attributes;
using FluentAssertions;
using Recyclable.CollectionsTests;

namespace Recyclable.Collections.Benchmarks.POC
{

	public class EqualVsContainsInConsecutiveOrderBenchmarks : IDisposable
	{
		private const int TestItemsCount = 10000;

		private readonly IEnumerable<long> _list1 = RecyclableLongListTestData.CreateTestData(TestItemsCount).ToRecyclableList();
		private readonly IEnumerable<long> _list2 = RecyclableLongListTestData.CreateTestData(TestItemsCount).ToArray();

		[Benchmark(Baseline = true)]
		public void Equal()
		{
			_ = _list1.Should().Equal(_list2);
		}

		[Benchmark]
		public void ContainsInConsecutiveOrder()
		{
			_ = _list1.Should().ContainInConsecutiveOrder(_list2);
		}

		public void Dispose()
		{
			if (_list1 is IDisposable disposable1)
			{
				disposable1.Dispose();
			}

			if (_list2 is IDisposable disposable2)
			{
				disposable2.Dispose();
			}
		}
	}
}
