using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		private object[] _testArray;
		private RecyclableArrayList<object> _testRecyclableArrayList;
		private RecyclableList<object> _testRecyclableList;

		[GlobalSetup]
		public void Setup()
		{
			Console.WriteLine("******* GLOBAL SETUP RAISED *******");
			_testObjects = Enumerable.Range(1, TestObjectCount).Select(x => new object()).ToArray();
			_testRecyclableArrayList = new(TestObjects, initialCapacity: TestObjectCount);
			_testRecyclableList = new(TestObjects, expectedItemsCount: TestObjectCount);
			_testArray = TestObjects.ToArray();
			_testObjectsAsList = TestObjects.ToList();
			_testObjectsAsRecyclableArrayList = TestObjects.ToRecyclableArrayList();
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			Console.WriteLine("******* GLOBAL CLEANUP *******");
			_testRecyclableArrayList?.Dispose();
			_testRecyclableList?.Dispose();
			TestObjectsAsRecyclableArrayList?.Dispose();
			_testObjectsAsRecyclableArrayList = default;
			_testObjectsAsList = default;
			_testObjects = default;
		}
	}
}
