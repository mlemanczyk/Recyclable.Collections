using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks.POC
{
	public enum DelegateVsComparerPocBenchmarkType
	{
            Comparer,
            Delegate,
            Func,
            ListOfBoxedStructs,
            ListOfClasses,
            ListOfComparers,
            ListOfDelegates,
            ListOfFuncs,
            ListOfLocalFuncs,
            ListOfStructs,
            ListOfUnboxedReadOnlyStructs,
            ListOfUnboxedStructs,
            StaticFunc,
	}

	[MemoryDiagnoser]
    public class DelegateVsComparerPocBenchmarks : PocBenchmarkBase<DelegateVsComparerPocBenchmarkType>
    {
        [Params
        (
            DelegateVsComparerPocBenchmarkType.Delegate,
            DelegateVsComparerPocBenchmarkType.Func,
            DelegateVsComparerPocBenchmarkType.ListOfBoxedStructs,
            DelegateVsComparerPocBenchmarkType.ListOfClasses,
            DelegateVsComparerPocBenchmarkType.ListOfComparers,
            DelegateVsComparerPocBenchmarkType.ListOfDelegates,
            DelegateVsComparerPocBenchmarkType.ListOfFuncs,
            DelegateVsComparerPocBenchmarkType.ListOfLocalFuncs,
            DelegateVsComparerPocBenchmarkType.ListOfStructs,
            DelegateVsComparerPocBenchmarkType.ListOfUnboxedReadOnlyStructs,
            DelegateVsComparerPocBenchmarkType.ListOfUnboxedStructs,
            DelegateVsComparerPocBenchmarkType.StaticFunc
        )]
        public override DelegateVsComparerPocBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

        [Params(DelegateVsComparerPocBenchmarkType.Comparer)]
        public override DelegateVsComparerPocBenchmarkType BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

		#region Dummy classes

		private delegate T Compare<T>(T x, T y);

		public readonly struct MyComparerReadOnlyStruct<T> : IComparer<T>
		{
			public int Compare(T? x, T? y) => Comparer<T>.Default.Compare(x, y);
		}

		private struct MyComparerStruct<T> : IComparer<T>
		{
            public int Compare(T? x, T? y) => Comparer<T>.Default.Compare(x, y);
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
            benchmark.Setup();
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
            benchmark.Cleanup();
		}

		private static readonly Comparer<long> _comparer = Comparer<long>.Default;
        private IEnumerable<long>? _testObjects;
        private IEnumerable<Func<long, long, int>>? _testListOfFunctions;
        private IEnumerable<IComparer<long>>? _testListOfComparers;
        private IEnumerable<Compare<long>>? _testListOfDelegates;
        private IEnumerable<Func<long, long, int>>? _testListOfLocalFunctions;
        private IEnumerable<MyComparer>? _testListOfClasses;
        private IEnumerable<MyComparerStruct<long>>? _testListOfStructs;
        private IEnumerable<IComparer<long>>? _testListOfBoxedStructs;
        private  IEnumerable<TComparer> TestListOfUnboxedStructs<TComparer, TValue>() where TComparer : IComparer<TValue>, new()
        {
            foreach (var _ in _testObjects!)
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

            foreach (var _ in testObjects)
            {
                yield return compareFunc;
            }
        }

        private void TestWithComparer(IComparer<long> comparer)
        {
            var testData = _testObjects!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length - 1; itemIdx++)
            {
                _ = comparer.Compare(testData[itemIdx], testData[itemIdx + 1]);
            }
        }

        private void TestWithDelegate<T>(Compare<long> compareFunc)
        {
            var testData = _testObjects!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length - 1; itemIdx++)
            {
                _ = compareFunc(testData[itemIdx], testData[itemIdx + 1]);
            }
        }

        private void TestWithFunc(Func<long, long, int> compareFunc)
        {
            var testData = _testObjects!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length - 1; itemIdx++)
            {
                _ = compareFunc(testData[itemIdx], testData[itemIdx + 1]);
            }
        }

        public void Comparer()
        {
            TestWithComparer(_comparer);
        }

        public void Func()
        {
            TestWithFunc((x, y) => (x - y) switch
            {
                > 0 => 1,
                < 0 => -1,
                _ => 0
            });
        }

        public void StaticFunc()
        {
            TestWithFunc(static (x, y) => (x - y) switch
                    {
                        > 0 => 1,
                        < 0 => -1,
                        _ => 0
                    });
        }

        public void Delegate()
        {
            TestWithDelegate<long>((x, y) => (x - y) switch
            {
                > 0 => 1,
                < 0 => -1,
                _ => 0
            });
        }

        public void ListOfFuncs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfFunctions!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var compareFunc = testData[itemIdx];
                _ = compareFunc(x, y);
            }
        }

        public void ListOfLocalFuncs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfLocalFunctions!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var compareFunc = testData[itemIdx];
                _ = compareFunc(x, y);
            }
        }

        public void ListOfComparers()
        {
            const long x = 2, y = 5;
            var testData = _testListOfComparers!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        public void ListOfClasses()
        {
            const long x = 2, y = 5;
            var testData = _testListOfClasses!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        public void ListOfStructs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfStructs!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        public void ListOfBoxedStructs()
        {
            const long x = 2, y = 5;
            var testData = _testListOfBoxedStructs!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        public void ListOfUnboxedStructs()
        {
            const long x = 2, y = 5;
            var testData = TestListOfUnboxedStructs<MyComparerStruct<long>, long>().ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        public void ListOfUnboxedReadOnlyStructs()
        {
            const long x = 2, y = 5;
            var testData = TestListOfUnboxedStructs<MyComparerReadOnlyStruct<long>, long>().ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var comparer = testData[itemIdx];
                _ = comparer.Compare(x, y);
            }
        }

        public void ListOfDelegates()
        {
            const long x = 2, y = 5;
            var testData = _testListOfDelegates!.ToArray();
            for (var itemIdx = 0; itemIdx < testData.Length; itemIdx++)
            {
                var compareDelegate = testData[itemIdx];
                _ = compareDelegate(x, y);
            }
        }

        protected override Action? GetTestMethod(DelegateVsComparerPocBenchmarkType benchmarkType) => benchmarkType switch
            {
                DelegateVsComparerPocBenchmarkType.Comparer => Comparer,
                DelegateVsComparerPocBenchmarkType.Delegate => Delegate,
                DelegateVsComparerPocBenchmarkType.Func => Func,
                DelegateVsComparerPocBenchmarkType.ListOfBoxedStructs => ListOfBoxedStructs,
                DelegateVsComparerPocBenchmarkType.ListOfClasses => ListOfClasses,
                DelegateVsComparerPocBenchmarkType.ListOfComparers => ListOfComparers,
                DelegateVsComparerPocBenchmarkType.ListOfDelegates => ListOfDelegates,
                DelegateVsComparerPocBenchmarkType.ListOfFuncs => ListOfFuncs,
                DelegateVsComparerPocBenchmarkType.ListOfLocalFuncs => ListOfLocalFuncs,
                DelegateVsComparerPocBenchmarkType.ListOfStructs => ListOfStructs,
                DelegateVsComparerPocBenchmarkType.ListOfUnboxedReadOnlyStructs => ListOfUnboxedReadOnlyStructs,
                DelegateVsComparerPocBenchmarkType.ListOfUnboxedStructs => ListOfUnboxedStructs,
                DelegateVsComparerPocBenchmarkType.StaticFunc => StaticFunc,
                _ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
            };

        public override void Setup()
        {
			Console.WriteLine("******* SETTING UP COMMON TEST DATA *******");
            _testObjects = Enumerable.Range(1, TestObjectCount).Select(x => (long)x);
            base.Setup();
        }

        public override void Cleanup()
        {
            _testObjects = null;
            base.Cleanup();
        }

        protected override void PrepareData<T>(T benchmarkType)
        {
            switch (benchmarkType)
            {
                case DelegateVsComparerPocBenchmarkType.ListOfBoxedStructs:
                    _testListOfBoxedStructs = _testObjects!.Select(_ => (IComparer<long>)new MyComparerStruct<long>());
                    break;
                case DelegateVsComparerPocBenchmarkType.ListOfClasses:
                    _testListOfClasses = _testObjects!.Select(_ => new MyComparer());
                    break;
                case DelegateVsComparerPocBenchmarkType.ListOfComparers:
                    _testListOfComparers = _testObjects!.Select(_ => (IComparer<long>)new MyComparer());
                    break;
                case DelegateVsComparerPocBenchmarkType.ListOfDelegates:
                    _testListOfDelegates = _testObjects!.Select(_ => new Compare<long>((x, y) => new MyComparer().Compare(x, y)));
                    break;
                case DelegateVsComparerPocBenchmarkType.ListOfFuncs:
                    _testListOfFunctions = _testObjects!.Select<long, Func<long, long, int>>(static _ => new MyComparer().Compare);
                    break;
                case DelegateVsComparerPocBenchmarkType.ListOfLocalFuncs:
                    _testListOfLocalFunctions = GetLocalFunctions(_testObjects!);
                    break;
                case DelegateVsComparerPocBenchmarkType.ListOfStructs:
                    _testListOfStructs = _testObjects!.Select(_ => new MyComparerStruct<long>());
                    break;
            }
        }
    }
}
