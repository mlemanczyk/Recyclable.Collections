// Ignore Spelling: Poc

using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks.POC
{
	[MemoryDiagnoser]
	public class TaskRunVsTaskFactoryStartNewBenchmarks
	{
		public int NumberOfTasks = 100;
		private static readonly TaskFactory _taskPool = new();

		[Benchmark]
		public void OwnTaskFactory()
		{
			for (var i = 0; i < NumberOfTasks; i++)
			{
				_ = _taskPool.StartNew(() => {});
			}
		}

		[Benchmark]
		public void TaskRun()
		{
			for (var i = 0; i < NumberOfTasks; i++)
			{
				_ = Task.Run(() => { });
			}
		}

		[Benchmark(Baseline = true)]
		public void TaskFactoryStartNew()
		{
			for (var i = 0; i < NumberOfTasks; i++)
			{
				_ = Task.Factory.StartNew(() => { });
			}
		}

		[Benchmark]
		public void TaskConstructor()
		{
			for (var i = 0; i < NumberOfTasks; i++)
			{
				new Task(() => { }).Start();
			}
		}

		[Benchmark]
		public void TaskConstructorWithKnownScheduler()
		{
			TaskScheduler scheduler = TaskScheduler.Current;
			for (var i = 0; i < NumberOfTasks; i++)
			{
				new Task(() => { }).Start(scheduler);
			}
		}
	}
}