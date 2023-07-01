using BenchmarkDotNet.Attributes;

#pragma warning disable RCS1085

namespace Recyclable.Collections.Benchmarks.POC
{
    public class RefVsInstanceMemberPocBenchmarks
    {
		public static void Run()
		{
			var benchmark = new RefVsInstanceMemberPocBenchmarks();
            benchmark.ArrayInstanceMember2();
            benchmark.ArrayInstanceMember3();
            benchmark.ArrayInstanceMember4();
            benchmark.ArrayInstanceMember5();
            benchmark.ArrayInstanceMember6();
            benchmark.ArrayInstanceMemberLocalVar5();
            benchmark.ArrayInstanceMemberLocalVar6();
            benchmark.IntInstanceMember2();
            benchmark.IntInstanceMember3();
            benchmark.IntInstanceMember4();
            benchmark.IntInstanceMember5();
            benchmark.RefToArray2();
            benchmark.RefToArray3();
            benchmark.RefToArray4();
            benchmark.RefToArray5();
            benchmark.RefToArray6();
            benchmark.RefToInt2();
            benchmark.RefToInt3();
		}

        private readonly TestCase _testCase = new();
        private static void DoNothing(int count)
        {
            _ = count;
        }

        private class TestCase
        {
            private int[] _items = new int[123];
            private int _count = 123;
			private static int Add(int count)
            {
                return count;
            }

            public int Count
            {
                get => _count;
                set => _count = value;
            }

            public ref int GetCountAsRefCount2()
            {
                ref int count = ref _count;
                count = 234;
                return ref count;
            }

            public ref int GetCountAsRefCount3()
            {
                ref int count = ref _count;
                count = 234;
                _ = TestCase.Add(count);
                return ref count;
            }

            public ref int GetCountAsRefCount4()
            {
                ref int count = ref _count;
                count = 234;
                _ = TestCase.Add(count);
                _ = TestCase.Add(count);
                return ref count;
            }

            public ref int GetCountAsRefCount5()
            {
                ref int count = ref _count;
                count = 234;
                _ = TestCase.Add(count);
                _ = TestCase.Add(count);
                _ = TestCase.Add(count);
                return ref count;
            }

            public int GetCountAsInstanceMember2()
            {
                _count = 234;
                return _count;
            }

            public int GetCountAsInstanceMember3()
            {
                _count = 234;
                _ = TestCase.Add(_count);
                return _count;
            }

            public int GetCountAsInstanceMember4()
            {
                _count = 234;
                _ = TestCase.Add(_count);
                _ = TestCase.Add(_count);
                return _count;
            }

            public int GetCountAsInstanceMember5()
            {
                _count = 234;
                _ = TestCase.Add(_count);
                _ = TestCase.Add(_count);
                _ = TestCase.Add(_count);
                return _count;
            }

            public int GetLengthAsArray2()
            {
                _ = TestCase.Add(_items.Length);
                return _items.Length;
            }

            public int GetLengthAsArray3()
            {
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                return _items.Length;
            }

            public int GetLengthAsArray4()
            {
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                return _items.Length;
            }

            public int GetLengthAsArray5()
            {
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                return _items.Length;
            }

            public int GetLengthAsArray6()
            {
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                _ = TestCase.Add(_items.Length);
                return _items.Length;
            }

            public int GetLengthAsArrayLocalVar5()
            {
                var items = _items;
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                return items.Length;
            }

            public int GetLengthAsArrayLocalVar6()
            {
                var items = _items;
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                return items.Length;
            }

            public int GetLengthAsRefArray2()
            {
                ref int[] items = ref _items;
                _ = TestCase.Add(items.Length);
                return items.Length;
            }

            public int GetLengthAsRefArray3()
            {
                ref int[] items = ref _items;
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                return items.Length;
            }

            public int GetLengthAsRefArray4()
            {
                ref int[] items = ref _items;
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                return items.Length;
            }

            public int GetLengthAsRefArray5()
            {
                ref int[] items = ref _items;
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                return items.Length;
            }

            public int GetLengthAsRefArray6()
            {
                ref int[] items = ref _items;
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                _ = TestCase.Add(items.Length);
                return items.Length;
            }
        }

        [Benchmark]
        public void RefToInt2()
        {
            var count = _testCase.GetCountAsRefCount2();
			DoNothing(count);
        }

        [Benchmark]
        public void RefToInt3()
        {
            var count = _testCase.GetCountAsRefCount3();
			DoNothing(count);
        }

        [Benchmark]
        public void RefToInt4()
        {
            var count = _testCase.GetCountAsRefCount4();
			DoNothing(count);
        }

        [Benchmark]
        public void RefToInt5()
        {
            var count = _testCase.GetCountAsRefCount5();
			DoNothing(count);
        }

        [Benchmark(Baseline = true)]
        public void IntInstanceMember2()
        {
            var count = _testCase.GetCountAsInstanceMember2();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void IntInstanceMember3()
        {
            var count = _testCase.GetCountAsInstanceMember3();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void IntInstanceMember4()
        {
            var count = _testCase.GetCountAsInstanceMember4();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void IntInstanceMember5()
        {
            var count = _testCase.GetCountAsInstanceMember5();
			DoNothing(count);
        }

        [Benchmark]
        public void RefToArray2()
        {
            var count = _testCase.GetLengthAsRefArray2();
			DoNothing(count);
        }

        [Benchmark]
        public void RefToArray3()
        {
            var count = _testCase.GetLengthAsRefArray3();
			DoNothing(count);
        }

        [Benchmark]
        public void RefToArray4()
        {
            var count = _testCase.GetLengthAsRefArray4();
			DoNothing(count);
        }

        [Benchmark]
        public void RefToArray5()
        {
            var count = _testCase.GetLengthAsRefArray5();
			DoNothing(count);
        }

        [Benchmark]
        public void RefToArray6()
        {
            var count = _testCase.GetLengthAsRefArray6();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void ArrayInstanceMember2()
        {
            var count = _testCase.GetLengthAsArray2();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void ArrayInstanceMember3()
        {
            var count = _testCase.GetLengthAsArray3();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void ArrayInstanceMember4()
        {
            var count = _testCase.GetLengthAsArray4();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void ArrayInstanceMember5()
        {
            var count = _testCase.GetLengthAsArray5();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void ArrayInstanceMember6()
        {
            var count = _testCase.GetLengthAsArray6();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void ArrayInstanceMemberLocalVar5()
        {
            var count = _testCase.GetLengthAsArrayLocalVar5();
			DoNothing(count);
        }

        [Benchmark(Baseline = false)]
        public void ArrayInstanceMemberLocalVar6()
        {
            var count = _testCase.GetLengthAsArrayLocalVar6();
			DoNothing(count);
        }
    }
}
