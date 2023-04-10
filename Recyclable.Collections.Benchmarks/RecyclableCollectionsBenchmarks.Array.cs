using BenchmarkDotNet.Attributes;
using MoreLinq.Extensions;

namespace Recyclable.Collections.Benchmarks
{
	[MemoryDiagnoser]
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		[Benchmark(Baseline = false)]
		public void Array_Create_WithCapacity()
		{
			var data = new object[TestObjectCount];
			DoNothing(data);
		}

		[Benchmark(Baseline = false)]
		public void Array_SetItem()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			long index = 0;
			for(var i = 0; i < dataCount; i++)
			{
				data[index] = TestObjects[index++];
			}
		}

		[Benchmark(Baseline = false)]
		public void Array_GetItem()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		[Benchmark(Baseline = false)]
		public void Array_Count()
		{
			var data = _testArray;
			DoNothing(data.Length);
		}

		[Benchmark(Baseline = false)]
		public void Array_LongCount()
		{
			var data = _testArray;
			DoNothing(data.LongLength);
		}

		[Benchmark(Baseline = false)]
		public void Array_IndexOf_FirstItems()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(Array.IndexOf(list, data[i]));
			}
		}
	}
}
