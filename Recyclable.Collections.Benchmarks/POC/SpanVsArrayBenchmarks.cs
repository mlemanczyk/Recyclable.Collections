﻿// Ignore Spelling: Poc

using BenchmarkDotNet.Attributes;
using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
	public enum SpanVsArrayBenchmarkType
	{
		ArrayGetWithVar,
		ArrayGetWithField,
		SpanGet,
		ArraySetWithVar,
		ArraySetWithField,
		SpanSet
	}

	public class SpanVsArrayBenchmarks : DataDrivenBenchmarksBase<SpanVsArrayBenchmarkType>
	{
		// [Params(SpanVsArrayBenchmarkType.ArrayGetWithVar, SpanVsArrayBenchmarkType.SpanGet)]
		// [Params(SpanVsArrayBenchmarkType.ArraySetWithVar, SpanVsArrayBenchmarkType.SpanSet)]
		[Params
		(
			SpanVsArrayBenchmarkType.ArrayGetWithVar, SpanVsArrayBenchmarkType.SpanGet,
			SpanVsArrayBenchmarkType.ArraySetWithField, SpanVsArrayBenchmarkType.ArraySetWithVar, SpanVsArrayBenchmarkType.SpanSet
		)]
		public override SpanVsArrayBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		[Params(SpanVsArrayBenchmarkType.ArrayGetWithField)]
		// [Params(SpanVsArrayBenchmarkType.ArraySetWithField)]
		public override SpanVsArrayBenchmarkType BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

		protected override Action GetTestMethod(SpanVsArrayBenchmarkType benchmarkType) => benchmarkType switch
		{
			SpanVsArrayBenchmarkType.ArrayGetWithVar => ArrayGetWithVar,
			SpanVsArrayBenchmarkType.ArrayGetWithField => ArrayGetWithField,
			SpanVsArrayBenchmarkType.SpanGet => SpanGet,
			SpanVsArrayBenchmarkType.ArraySetWithVar => ArraySetWithVar,
			SpanVsArrayBenchmarkType.ArraySetWithField => ArraySetWithField,
			SpanVsArrayBenchmarkType.SpanSet => SpanSet,
			_ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
		};

		public void ArrayGetWithVar()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			for (int i = 0; i < dataCount; i++)
			{
				DoNothing.With(data[i]);
			}
		}

		public void ArrayGetWithField()
		{
			var dataCount = TestObjectCount;
			for (int i = 0; i < dataCount; i++)
			{
				DoNothing.With(_testObjects![i]);
			}
		}

		public void SpanGet()
		{
			var dataCount = TestObjectCount;
			Span<int> data = new(_testObjects, 0, dataCount);
			for (int i = 0; i < dataCount; i++)
			{
				DoNothing.With(data[i]);
			}
		}

		public void ArraySetWithVar()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			for (int i = 0; i < dataCount; i++)
			{
				data[i] = i;
			}
		}

		public void ArraySetWithField()
		{
			var dataCount = TestObjectCount;
			for (int i = 0; i < dataCount; i++)
			{
				_testObjects![i] = i;
			}
		}

		public void SpanSet()
		{
			var dataCount = TestObjectCount;
			Span<int> data = new(_testObjects, 0, dataCount);
			for (int i = 0; i < dataCount; i++)
			{
				data[i] = i;
			}
		}
	}
}
