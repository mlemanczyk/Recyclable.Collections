using BenchmarkDotNet.Attributes;
using MiscUtil;

namespace Recyclable.Collections.Benchmarks.POC
{
	[MemoryDiagnoser]
    public class DelegateVsComparerPocBenchmarks : PocBenchmarkBase
    {
		#region Dummy classes

		private delegate T Compare<T>(T x, T y);

		public readonly struct MyComparerReadOnlyStruct<T> : IComparer<T>
		{
			public int Compare(T? x, T? y) => Operator.Subtract(x, y) switch
			{
				> 0 => 1,
				< 0 => -1,
				_ => 0,
			};
		}

		private struct MyComparerStruct<T> : IComparer<T>
		{
			public int Compare(T? x, T? y) => Operator.Subtract(x, y) switch
			{
				> 0 => 1,
				< 0 => -1,
				_ => 0,
			};
		}

		private class MyComparer : IComparer<long>
		{
			public int Compare(long x, long y) => (x - y) switch
			{
				> 0 => 1,
				< 0 => -1,
				_ => 0,
			};
		}

		#endregion

        public static void Run()
        {
			var benchmark = new DelegateVsComparerPocBenchmarks();
            benchmark.Comparer();
            benchmark.Delegate();
            benchmark.Func();
            benchmark.ListOfBoxedStructs();
            benchmark.ListOfClasses();
            benchmark.ListOfComparers();
            benchmark.ListOfDelegates();
            benchmark.ListOfFuncs();
            benchmark.ListOfLocalFuncs();
            benchmark.ListOfStructs();
            benchmark.ListOfUnboxedReadOnlyStructs();
            benchmark.ListOfUnboxedStructs();
            benchmark.StaticFunc();
		}

		private static readonly Comparer<long> _comparer = Comparer<long>.Default;

        private const int _objectCount = 1_000_000;
        private static readonly IEnumerable<long> _testObjects = Enumerable.Range(1, _objectCount).Select(x => (long)x);
        private static readonly IEnumerable<Func<long, long, int>> _testListOfFunctions = _testObjects.Select<long, Func<long, long, int>>(static x => new MyComparer().Compare);
        private static readonly IEnumerable<IComparer<long>> _testListOfComparers = _testObjects.Select(x => (IComparer<long>)new MyComparer());
        private static readonly IEnumerable<Compare<long>> _testListOfDelegates = _testObjects.Select(x => new Compare<long>((x, y) => new MyComparer().Compare(x, y)));
        private static readonly IEnumerable<Func<long, long, int>> _testListOfLocalFunctions = GetLocalFunctions(_testObjects);
        private static readonly IEnumerable<MyComparer> _testListOfClasses = _testObjects.Select(x => new MyComparer());
        private static readonly IEnumerable<MyComparerStruct<long>> _testListOfStructs = _testObjects.Select(x => new MyComparerStruct<long>());
        private static readonly IEnumerable<IComparer<long>> _testListOfBoxedStructs = _testObjects.Select(x => (IComparer<long>)new MyComparerStruct<long>());
        private static IEnumerable<TComparer> _testListOfUnboxedStructs<TComparer, TValue>() where TComparer : IComparer<TValue>, new()
        {
            foreach (var item in _testObjects)
            {
                yield return new TComparer();
            }
        }

        private static IEnumerable<Func<long, long, int>> GetLocalFunctions(IEnumerable<long> testObjects)
        {
            static int compareFunc(long x, long y)
            {
                return (x - y) switch
                {
                    > 0 => 1,
                    < 0 => -1,
                    _ => 0
                };
            }

            foreach (var item in testObjects)
            {
                yield return compareFunc;
            }
        }

        private static void TestWithComparer(IComparer<long> comparer)
        {
            var testData = _testObjects.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length - 1; itemIdx++)
            {
                _ = comparer.Compare(testData[itemIdx], testData[itemIdx + 1]);
            }
        }

        [Benchmark(Baseline = true)]
        public void Comparer()
        {
            TestWithComparer(_comparer);
        }

        private static void TestWithFunc(Func<long, long, int> compareFunc)
        {
            var testData = _testObjects.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length - 1; itemIdx++)
            {
                _ = compareFunc(testData[itemIdx], testData[itemIdx + 1]);
            }
        }

        [Benchmark]
        public void Func()
        {
            TestWithFunc((x, y) => (x - y) switch
            {
                > 0 => 1,
                < 0 => -1,
                _ => 0
            });
        }

        [Benchmark]
        public void StaticFunc()
        {
            TestWithFunc(static (x, y) => (x - y) switch
                    {
                        > 0 => 1,
                        < 0 => -1,
                        _ => 0
                    });
        }

        private static void TestWithDelegate<T>(Compare<long> compareFunc)
        {
            var testData = _testObjects.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length - 1; itemIdx++)
            {
                _ = compareFunc(testData[itemIdx], testData[itemIdx + 1]);
            }
        }

        [Benchmark]
        public void Delegate()
        {
            TestWithDelegate<long>((x, y) => (x - y) switch
            {
                > 0 => 1,
                < 0 => -1,
                _ => 0
            });
        }

        [Benchmark]
        public void ListOfFuncs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfFunctions.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var compareFunc = testData[itemIdx];
                _ = compareFunc(x, y);
            }
        }

        [Benchmark]
        public void ListOfLocalFuncs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfLocalFunctions.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var compareFunc = testData[itemIdx];
                _ = compareFunc(x, y);
            }
        }

        [Benchmark]
        public void ListOfComparers()
        {
            const long x = 2, y = 5;
            var testData = _testListOfComparers.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        [Benchmark]
        public void ListOfClasses()
        {
            const long x = 2, y = 5;
            var testData = _testListOfClasses.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        [Benchmark]
        public void ListOfStructs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfStructs.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        [Benchmark]
        public void ListOfBoxedStructs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfBoxedStructs.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        [Benchmark]
        public void ListOfUnboxedStructs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfUnboxedStructs<MyComparerStruct<long>, long>().ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        [Benchmark]
        public void ListOfUnboxedReadOnlyStructs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfUnboxedStructs<MyComparerReadOnlyStruct<long>, long>().ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        [Benchmark]
        public void ListOfDelegates()
        {
            const long x = 2, y = 5;
            var testData = _testListOfDelegates.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var compareDelegate = testData[itemIdx];
                _ = compareDelegate(x, y);
            }
        }
    }
}
