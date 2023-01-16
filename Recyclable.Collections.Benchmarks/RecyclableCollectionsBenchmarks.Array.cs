using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	[MemoryDiagnoser]
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		private object[] _testArray;

		[Benchmark(Baseline = true)]
		public void Array_Create_WithCapacity()
		{
			var data = new object[TestObjectCount];
			DoNothing(data);
		}

		//[Benchmark(Baseline = true)]
		public void Array_SetItem()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		//[Benchmark]
		public void Array_GetItem()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		//[Benchmark]
		public void Array_Count()
		{
			var data = _testArray;
			DoNothing(data.Length);
		}

		//[Benchmark]
		public void Array_LongCount()
		{
			var data = _testArray;
			DoNothing(data.LongLength);
		}
	}
}
