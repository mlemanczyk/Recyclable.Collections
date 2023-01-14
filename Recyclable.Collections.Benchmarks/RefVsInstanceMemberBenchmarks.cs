using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Recyclable.Collections.Benchmarks
{
	public class RefVsInstanceMemberBenchmarks
	{
		private readonly TestCase _testCase = new();
		private void DoNothing(int count)
		{
			_ = count;
		}

		private class TestCase
		{
			private int _count = 123;
			public int Count
			{
				get => _count;
				set => _count = value;
			}

			public ref int GetRefCount()
			{
				ref int count = ref _count;
				count = 234;
				_ = Add(count);
				return ref count;
			}

			private int Add(int count)
			{
				return count;
			}

			public int GetCount()
			{
				_count = 234;
				_ = Add(_count);
				return _count;
			}
		}

		[Benchmark]
		public void Ref()
		{
			var count = _testCase.GetRefCount();
			DoNothing(count);
		}

		[Benchmark(Baseline = true)]
		public void InstanceMember()
		{
			var count = _testCase.GetCount();
			DoNothing(count);
		}
	}
}
