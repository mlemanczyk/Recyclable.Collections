using BenchmarkDotNet.Attributes;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Recyclable.Collections.Benchmarks.POC
{
    [MemoryDiagnoser]
    public class ModuloPocBenchmarks
    {
		public static void Run()
		{
			var benchmark = new ModuloPocBenchmarks();
            benchmark.Setup();
            benchmark.AddWithFor();
            benchmark.AddWithForOptimized();
            benchmark.DividerAndModulusWithOperator();
            benchmark.MathDivRem();
            benchmark.MixedApproachWithForLoopOptimized();
            benchmark.MixedApproachWithSubtractWithStackalloc();
            benchmark.MixedApproachWithSubtractWithWhile();
            benchmark.MixedApproachWithSubtractWithWhileAsVector();
            benchmark.MixedApproachWithSubtractWithWhileWithSse2();
            benchmark.SubtractWithWhile();
            benchmark.SubtractWithWhileWithSse2();
            benchmark.XDivYMulX();
            benchmark.XDivYMulXWithLocalVar();
		}

        private long _cutoff;
        private Vector128<long> _increment;
        private long _lastCaseIndex;

        [GlobalSetup]
        public void Setup()
        {
            _cutoff = BlockSize << 4;
            _increment = Vector128.Create(-BlockSize, 1L);
            _lastCaseIndex = 0;
        }

        private static void DoNothing<T>(T result, T remainder, [CallerMemberName] string? callerName = null)
        {
            //Console.WriteLine($"{callerName}: result = {result}, remainder = {remainder}");
        }

        //[Params(123, 10, 2, int.MaxValue, int.MaxValue / 2, int.MaxValue / 3, int.MaxValue / 4, int.MaxValue / 6, int.MaxValue / 10, int.MaxValue / 15, int.MaxValue / 20)]
        [Params(int.MaxValue, int.MaxValue / 2, int.MaxValue / 3, int.MaxValue / 4, int.MaxValue / 6, int.MaxValue / 8, int.MaxValue / 10, int.MaxValue / 100)]
        //[Params(int.MaxValue / 10, int.MaxValue / 20, int.MaxValue / 100)]
        public int BlockSize { get; set; } = int.MaxValue;

        [Params(int.MaxValue, int.MaxValue / 1000, int.MaxValue / 10000)]
        //[Params(int.MaxValue)]
        //[Params(int.MaxValue * 100L)]
        public long TestNumber { get; set; } = int.MaxValue;

        private void CalculateDividerAndModulusWithOperator(long i)
        {
            DoNothing(i / BlockSize, i % BlockSize);
        }

        private void CalculateWithMathDivRem(long i)
        {
            DoNothing(Math.DivRem(i, BlockSize, out long remainder), remainder);
        }

        private void CalculateWithAddWithFor(long i)
        {
            int result = 0;
            int remainder = 0;
            int blockSize = BlockSize;
            for (long iteration = 0; iteration < i; iteration++)
            {
                if (remainder > blockSize - 1)
                {
                    remainder = 0;
                    result++;
                }

                remainder++;
            }

            DoNothing(result, remainder);
        }

        private void CalculateWithAddWithForOptimized(long i)
        {
            long total = 0;
            int result = 0;
            int blockSize = BlockSize;
            while (total + blockSize <= i)
            {
                total += blockSize;
                result++;
            }

            DoNothing(result, i - total);
        }

        private void CalculateWithXDivYMulX(long i)
        {
            DoNothing(i / BlockSize, i - (i / BlockSize * BlockSize));
        }

        private void CalculateWithXDivYMulXWithLocalVar(long i)
        {
            int blockSize = BlockSize;
            int result = (int)(i / blockSize);
            DoNothing(result, i - (result * blockSize));
        }

        private void CalculateWithSubtractWithWhile(long i)
        {
            int result = 0;
            long remainder = i;
            int blockSize = BlockSize;
            while (remainder >= blockSize)
            {
                remainder -= blockSize;
                result++;
            }

            DoNothing(result, remainder);
        }

        private void CalculateWithSubractWithWhileWithSse2(long i)
        {
            var state = Vector128.Create(i, 0);
            int blockSize = BlockSize;
            var inc = _increment;
            while (state.GetElement(0) >= blockSize)
            {
                state = Sse2.Add(state, inc);
            }

            DoNothing(state.GetElement(1), state.GetElement(0));
        }

        private void CalculateWithMixedApproachWithSubractWithWhile(long i)
        {
            long remainder;
            if (i >= _cutoff)
            {
                DoNothing(Math.DivRem(i, BlockSize, out remainder), remainder);
            }
            else
            {
                int result = 0;
                int blockSize = BlockSize;
                remainder = i;
                while (remainder >= blockSize)
                {
                    remainder -= blockSize;
                    result++;
                }

                DoNothing(result, remainder);
            }
        }

        private void CalculateWithMixedApproachWithSubtractWithWhileAsVector(long i)
        {
            if (i >= _cutoff)
            {
                DoNothing(Math.DivRem(i, BlockSize, out long remainder), remainder);
            }
            else
            {
                var state = new Vector<long>(new[] { 0L, i, 0, 0 });
                var increment = new Vector<long>(new[] { 1L, -BlockSize, 0, 0 });
                int blockSize = BlockSize;
                while (state[1] >= blockSize)
                {
                    state += increment;
                }

                DoNothing(state[0], state[1]);
            }
        }

        private void CalculateWithMixedApproachWithSubtractWithWhileWithSse2(long i)
        {
            if (i >= _cutoff)
            {
                DoNothing(Math.DivRem(i, BlockSize, out long remainder), remainder);
            }
            else
            {
                var increment = _increment;
                var state = Vector128.Create(i, 0);
                int blockSize = BlockSize;
                while (state.GetElement(0) >= blockSize)
                {
                    state = Sse2.Add(state, increment);
                }

                DoNothing(state.GetElement(1), state.GetElement(0));
            }
        }

        private void CalculateWithMixedApproachWithSubtractWithStackalloc(long i)
        {
            if (i >= _cutoff)
            {
                DoNothing(Math.DivRem(i, BlockSize, out long remainder), remainder);
            }
            else
            {
                Span<long> state = stackalloc long[2] { i, 0 };// Vector128.Create(BlockSize, 0);
                int blockSize = BlockSize;
                while (state[0] >= blockSize)
                {
                    state[0] -= blockSize;
                    state[1]++;
                }

                DoNothing(state[1], state[0]);
            }
        }

        private void CalculateWithMixedApproachWithForLoopOptimized(long i)
        {
            long total;
            if (i >= _cutoff)
            {
                DoNothing(Math.DivRem(i, BlockSize, out total), total);
            }
            else
            {
                total = 0;
                int result = 0;
                int blockSize = BlockSize;
                while (total + blockSize <= i)
                {
                    total += blockSize;
                    result++;
                }

                DoNothing(result, i - total);
            }
        }

        [Benchmark(Baseline = true)]
        public void DividerAndModulusWithOperator()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateDividerAndModulusWithOperator(i);
            }
        }

        [Benchmark]
        public void MathDivRem()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithMathDivRem(i);
            }
        }

        [Benchmark]
        public void AddWithFor()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithAddWithFor(i);
            }
        }

        [Benchmark(Baseline = true)]
        public void AddWithForOptimized()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithAddWithForOptimized(i);
            }
        }

        [Benchmark]
        public void XDivYMulX()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithXDivYMulX(i);
            }
        }

        [Benchmark]
        public void XDivYMulXWithLocalVar()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithXDivYMulXWithLocalVar(i);
            }
        }

        [Benchmark]
        public void SubtractWithWhile()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithSubtractWithWhile(i);
            }
        }

        [Benchmark]
        public void SubtractWithWhileWithSse2()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithSubractWithWhileWithSse2(i);
            }
        }

        [Benchmark(Baseline = false)]
        public void MixedApproachWithSubtractWithWhile()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithMixedApproachWithSubractWithWhile(i);
            }
        }

        [Benchmark(Baseline = false)]
        public void MixedApproachWithSubtractWithWhileAsVector()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithMixedApproachWithSubtractWithWhileAsVector(i);
            }
        }

        [Benchmark(Baseline = false)]
        public void MixedApproachWithSubtractWithWhileWithSse2()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithMixedApproachWithSubtractWithWhileWithSse2(i);
            }
        }

        [Benchmark(Baseline = false)]
        public void MixedApproachWithSubtractWithStackalloc()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithMixedApproachWithSubtractWithStackalloc(i);
            }
        }

        [Benchmark(Baseline = false)]
        public void MixedApproachWithForLoopOptimized()
        {
            long testNumber = TestNumber;
            for (long i = testNumber; i >= _lastCaseIndex; i--)
            {
                CalculateWithMixedApproachWithForLoopOptimized(i);
            }
        }
    }
}