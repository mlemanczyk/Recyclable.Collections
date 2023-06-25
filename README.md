# Recyclable.Collections
`Recyclable.Collections` project is an open source framework for operating dynamic lists at performance close to raw arrays, but fairly unlimited in size. It aims at providing minimal memory footprint. It implements `IList<T>` interface and is targeted as direct replacements of `List<T>`, `SortableList<T>`, `PriorityQueue<T>` & similar.

## Included
* `RecyclableList<T>`
* `RecyclableLongList<T>`

# Milestones
1. ✅ Create basic classes
    1. ✅ `RecyclableList<T>`
    1. ✅ `RecyclableLongList<T>`
    1. 🅿️ `RecyclableQueue<T>`
    1. 🅿️ `RecyclableSortedList<T>`
    1. 🅿️ `RecyclableStack<T>`
    1. 🅿️ `RecyclableUnorderedList<T>`
1. ✅ Create basic unit tests
    1. ✅ `RecyclableList<T>`
    1. ✅ `RecyclableLongList<T>`
    1. 🅿️ `RecyclableQueue<T>`
    1. 🅿️ `RecyclableSortedList<T>`
    1. 🅿️ `RecyclableStack<T>`
    1. 🅿️ `RecyclableUnorderedList<T>`
1. ✅ Optimize `RecyclableList<T>`
    1. ✅ `Add`
    1. ✅ `AddRange`
        1. ✅ when source is `array<T>`
        1. ✅ when source is `List<T>`
        1. ✅ when source is `IList<T>`
        1. ✅ when source is `RecyclableList<T>`
        1. ✅ when source is `RecyclableLongList<T>`
        1. ✅ when source is `IEnumerable<T>`
        1. ✅ when source has non-enumerated count
    1. ✅ `Clear`
    1. ✅ `Contains`
    1. ✅ `CopyTo`
    1. ✅ `EnsureCapacity`
    1. ✅ `GetEnumerator`
    1. ✅ `IndexOf`
    1. ✅ `Insert`
    1. ✅ `Remove`
    1. ✅ `RemoveAt`
    1. ✅ `Resize`
    1. ✅ `this[int index]`
1. ✅ Port `RecyclableList<T>` implementation to `RecyclableLongList<T>`
1. 👉 Optimize `RecyclableLongList<T>`
    1. ✅ `Add`
    1. ✅ `AddRange`
        1. ✅ when source is `array<T>`
        1. ✅ when source is `List<T>`
        1. ✅ when source is `IList<T>`
        1. ✅ when source is `RecyclableList<T>`
        1. ✅ when source is `RecyclableLongList<T>`
        1. ✅ when source is `IEnumerable<T>`
        1. ✅ when source has non-enumerated count
    1. ✅ `Clear`
    1. ✅ `Contains`
    1. 🅿️ `CopyTo`
    1. ✅ `EnsureCapacity`
    1. ✅ `GetEnumerator`
    1. ✅ `IndexOf`
    1. ✅ `LongIndexOf`
    1. ✅ `Insert`
    1. ✅ `Remove`
    1. ✅ `RemoveAt(int index)`
    1. ✅ `RemoveAt(long index)`
    1. ✅ `RemoveAt`
    1. ✅ `Resize`
    1. ✅ `this[int index]`
    1. ✅ `this[long index]`
    1. ✅ Rename `RecyclableList<T>` to `RecyclableLongList<T>`
    1. ✅ Rename `RecyclableArrayList<T>` to `RecyclableList<T>`
    1. ✅ Fix failing tests
    1. ✅ Hide not ready classes
        1. ✅ `RecyclableQueue<T>`
        1. ✅ `RecyclableSortedList<T>`
        1. ✅ `RecyclableStack<T>`
        1. ✅ `RecyclableUnorderedList<T>`
    1. ✅ Add support for `ReadOnlySpan<T>`
    1. ✅ Release 0.0.3-alpha
    1. ✅ Implement `List<T>` interfaces
        1. ✅ `IList<T>`
        1. ✅ `ICollection<T>`
        1. ✅ `IEnumerable<T>`
        1. ✅ `IEnumerable`
        1. ✅ `IReadOnlyList<T>`
        1. ✅ `IReadOnlyCollection<T>`
        1. ✅ `IList`
        1. ✅ `ICollection`
    1. ✅ Implement list versioning to allow data change identification
    1. 👉 Make sure that `NeedsClearing` is used & items are cleared in
        1. 🅿️ `Clear`
        1. 🅿️ `Dispose`
        1. 🅿️ `Remove`
        1. 🅿️ `RemoveAt`
        1. 🅿️ `RemoveBlock`
    1. 🅿️ Add `.ToRecyclableList` / `.ToRecyclableLongList` variants for all supported collection types
        1. 🅿️ `RecyclableList`
        1. 🅿️ `RecyclableLongList`
        1. 🅿️ `IList<T>`
        1. 🅿️ `ICollection<T>`
        1. 🅿️ `IEnumerable<T>`
        1. 🅿️ `IEnumerable`
        1. 🅿️ `IReadOnlyList<T>`
        1. 🅿️ `IReadOnlyCollection<T>`
        1. 🅿️ `IList`
        1. 🅿️ `ICollection`
    1. 🅿️ Release 0.0.3-beta
    1. 🅿️ Add support for `ulong` indexing
        1. 🅿️ Convert `_memoryBlocks` to `Array` to allow `ulong` lengths
        1. 🅿️ Convert block indexes from `int` to `ulong` or `long`
    1. 🅿️ Overflow review
        1. 🅿️ Add type casting to `long` for `<<` & `>>` operations, where required
        1. 🅿️ Make type castings `checked`
    1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableList<T>`
    1. 🅿️ Release 0.0.3
1. 🅿️ Implement `ILongList<T>` interface
    1. 🅿️ `RecyclableList<T>`
    1. 🅿️ `RecyclableLongList<T>`
1. 🅿️ Implement `RecyclableQeueue<T>`
    1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableQueue<T>`
    1. 🅿️ Release 0.0.4
1. 🅿️ Implement `RecyclableStack<T>`
    1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableStack<T>`
    1. 🅿️ Release 0.0.5
1. 🅿️ Implement `RecyclableSortedList<T>`
    1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableSortedList<T>`
    1. 🅿️ Release 0.0.6
1. 🅿️ Implement `RecyclableUnorderedList<T>`
    1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableUnorderedList<T>`
    1. 🅿️ Release 0.0.7
1. 🅿️ Implement `RecyclableVersionedList<T>`
    1. 🅿️ Port `RecyclableList<T>` to `RecyclableVersionedList<T>`
    1. 🅿️ Port `RecyclableLongList<T>` to `RecyclableVersionedLongList<T>`
    1. 🅿️ Release 0.0.7a
1. 🅿️ Optimize `OneSizeArrayPool`
    1. 🅿️ Review locks
    1. 🅿️ Measure multi-threading performance
    1. 🅿️ Implement memory bucket disposal in high RAM pressure scenario
1. 🅿️ Optimize `RecyclableCollectionVersionObjectPool`
    1. 🅿️ Implement custom `ObjectPool<T>`
    1. 🅿️ Measure multi-threading performance
    1. 🅿️ Implement memory bucket disposal in high RAM pressure scenario
1. 🅿️ Review `RecyclableArrayPool`
    1. 🅿️ Review locks
    1. 🅿️ Measure multi-threading performance
    1. 🅿️ Implement array disposal in high RAM pressure scenario
1. 🅿️ Optimize `MemoryBucket<T>`
    1. 🅿️ Convert to `struct`, if possible
    1. 🅿️ Find out if there are better replacements
    1. 🅿️ Release 0.0.8
1. 🅿️ Optimize
    1. 🅿️ `IndexOfSynchronizationContext`
    1. 🅿️ `IndexOfSynchronizationContextPool`
    1. 🅿️ `ManualResetEventSlimmer`
        1. 🅿️ Multi-threading benchmarks
    1. 🅿️ `ManualResetEventSlimmerPool`
        1. 🅿️ Multi-threading benchmarks
    1. 🅿️ `SpinLockSlimmer`
        1. 🅿️ Multi-threading benchmarks
1. 🅿️ Release 0.0.9-beta
1. 🅿️ Extend unit tests
    1. 🅿️ `.Add` / `.AddRange` must allow `null` values
    1. 🅿️ `.Remove` / `.RemoveAt` / `.Clear` must clear reference when reference type
    1. 🅿️ `RecyclableLongListExtensions.CopyTo`
1. 🅿️ Cleanup
    1. 🅿️ Replace `LastBlockWithData` property with `_lastBlockWithData` field
    1. ✅ `RecyclableLongListExtensions`
    1. ✅ `ListExtensions`
    1. 🅿️ `MathUtils`
    1. 🅿️ Resolve TODOs
1. 🅿️ Optimize
    1. 🅿️ `ListExtensions`
    1. 🅿️ `MathUtils`
    1. 🅿️ `SystemRandomNumberGenerator`
    1. 🅿️ `RecyclableLongListExtensions`
1. 🅿️ Review and remove warnings & hints
    1. 🅿️ Warnings
    1. 🅿️ Hints
1. Documentation
    1. 🅿️ Document the most efficient iteration over array collections
    1. 🅿️ Document the most efficient iteration over blocked collections
    1. 🅿️ Document differences in behavior
    1. 🅿️ Document other specifics
1. 🅿️ Release 1.0.0
1. 🅿️ Implement source code generators
    1. 🅿️ Add attributes
        1. 🅿️ `GeneratorBaseClassAttribute` for marking base class used for generation
        1. 🅿️ `VersionedAttribute` for marking classes as versioned
        1. 🅿️ `IncVersionAttribute` for marking methods & setters as resulting in version increase
    1. 🅿️ Implement source code generator
        1. 🅿️ Generate partial class for classes marked with `GeneratorBaseClassAttribute`
        1. 🅿️ Add support for fields
        1. 🅿️ Add support for properties
        1. 🅿️ Add support for methods
        1. 🅿️ Add support for constructor
        1. 🅿️ Skip base fields, methods, properties etc. when they're overridden in the child class
1. 🅿️ Optimize
    1. 🅿️ `RecyclableLongList<T>.Resize`
    1. 🅿️ `RecyclableLongList<T>.CopyTo`
    1. 🅿️ Check if we can benefit from Sse2 in `.IndexOf`/`.Contains` methods as given in [MS blog](https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/).
1. 🅿️ Add support for `ICollection<T>` interface in `.AddRange` & `constructor`
1. 🅿️ Final optimizations
    1. 🅿️ Replace `Math` class usages with `if` statements
    1. 🅿️ Replace `a - b > 0` & `a - b < 0` comparisons with `a > b` & `a < b`
    1. 🅿️ Replace `a + b > 0` & `a + b < 0` comparisons with `a > b` & `a < b`
    1. 🅿️ Replace `a / b` & `a * b` calculations with equivalents, where possible
    1. 🅿️ Replace virtual calls with static calls
    1. 🅿️ Replace `blockSize` sums by powers of 2, minus 1
    1. 🅿️ Remove type castings, if possible
    1. 🅿️ Convert generic methods to non-generic
    1. 🅿️ Replace integer comparisons with comparisons to 0 / 1, if possible
1. 🅿️ Release 2.0.0

# Characteristics of the classes

## Common
* All classes implement `IDisposable` interface & SHOULD be disposed after use. That's to return block arrays taken from the shared pool. It may be foreseen as an issue for replacement in existing code, which obviously is missing `using` clause. But considering that `Dispose` will be called by `GC` anyway, it should cause issues in specific scenarios, only. In either case the fix is one-word addition of `using`.
* Memory blocks are created in 2 ways, depending on `blockSize` value
    * when `blockSize < 128` ⇨ blocks are created as regular `arrays`.
    * when `blockSize >= 128` ⇨ blocks are taken from and returned to `ArrayPool<T>.Shared`, when blocks are removed.
    * maximum `blockSize` value is `2_147_483_591`, which corresponds more or less to the maximum array size.
    * `blockSize` MUST be a power of 2. It will be rounded up to the closest power of 2, if needed. That is due to high performance gain on some operations, like the calculation of item index in a block.
* Array pools are shared between the same `T` type. I.e. `List<int>` will use a different pool from `List<short>` and so on. For high concurrency environments you may want to provide your own pools, when this feature becomes available in the upcoming releases.
* Trying to access `this[int index]`, `this[long index]`, `Count`, `LongCount` etc. when `Capacity == 0` will raise `NullReferenceException`. This is by design to remove all non-critical code from the constructor.
* ⚠️ The default enumerators don't check if the collection was modified. That's by design due to high performance hit of the check. That should be fine in most use-cases. If you need to check for modifications, you're welcomed to use `.GetVersionedEnumerator()` / `IRecyclableVersionedXxxList<T>` type-casting, e.g. in `foreach` loops.
```csharp
foreach (var item in (IRecyclableVersionedList<long>)recyclableList)
{
	...
	...
	...
}

foreach (var item in (IRecyclableVersionedLongList<long>)recyclableLongList)
{
	...
	...
	...
}
```
* ⚠️ The state of the default enumerators before the first & after the last call to `.MoveNext()` is undefined, but no exception is raised. That's by design due to high performance hit of the checks. This behavior is compatible with `foreach` loops, which will never access `.Current` property
	* when the collection is empty, or
	* when `.MoveNext()` returned `false`, after reaching the end of the collection.

* ⚠️ If you decide to use `.GetEnumerator()` / `.GetVersionedEnumerator()`, instead of relying on `foreach` loops
	* you MUST ensure that you always respect the the result of `.MoveNext()` call, and
	* you MUST NOT access `.Current` property before calling `.MoveNext()` or after reaching the end of the collection.\
	
	**Failing to do so WILL result in unpredictable behavior.**
	### `foreach` Benchmarks
	```csharp
	BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2)
	AMD Ryzen 7 4700U with Radeon Graphics, 1 CPU, 8 logical and 8 physical cores
	.NET SDK=7.0.305
	  [Host]     : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX2
	  DefaultJob : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX2
	```
	<details>
	<summary><strong>Non-Versioned & Versioned foreach Benchmark Results - click to expand</strong></summary>

	|   Method |         TestCase | BaseDataType |           DataType | TestObjectCount |             Mean |          Error |          StdDev |           Median | Ratio | RatioSD | Allocated | Alloc Ratio |
	|--------- |----------------- |------------- |------------------- |---------------- |-----------------:|---------------:|----------------:|-----------------:|------:|--------:|----------:|------------:|
	| Baseline |          ForEach |         List |         PooledList |               0 |         3.936 ns |      0.0346 ns |       0.0323 ns |         3.936 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               0 |         3.905 ns |      0.0068 ns |       0.0053 ns |         3.904 ns |  0.99 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |               1 |         5.127 ns |      0.0282 ns |       0.0263 ns |         5.120 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               1 |         5.141 ns |      0.0419 ns |       0.0392 ns |         5.128 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |               2 |         6.832 ns |      0.0362 ns |       0.0339 ns |         6.822 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               2 |         6.852 ns |      0.0561 ns |       0.0524 ns |         6.847 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |               3 |         8.517 ns |      0.0511 ns |       0.0478 ns |         8.492 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               3 |         8.509 ns |      0.0423 ns |       0.0395 ns |         8.502 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |               4 |        10.934 ns |      0.0195 ns |       0.0172 ns |        10.928 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               4 |        10.926 ns |      0.0220 ns |       0.0171 ns |        10.926 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |               5 |        12.614 ns |      0.0217 ns |       0.0169 ns |        12.616 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               5 |        12.647 ns |      0.1010 ns |       0.0945 ns |        12.606 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |               6 |        14.928 ns |      0.0865 ns |       0.0809 ns |        14.920 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               6 |        14.980 ns |      0.1356 ns |       0.1269 ns |        15.051 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |               7 |        16.542 ns |      0.1544 ns |       0.1369 ns |        16.593 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               7 |        16.529 ns |      0.1579 ns |       0.1477 ns |        16.543 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |               8 |        18.113 ns |      0.1446 ns |       0.1353 ns |        18.111 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               8 |        18.078 ns |      0.1254 ns |       0.1173 ns |        18.055 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |               9 |        19.938 ns |      0.1234 ns |       0.1155 ns |        19.902 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |               9 |        19.720 ns |      0.0873 ns |       0.0773 ns |        19.727 ns |  0.99 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              10 |        21.640 ns |      0.2245 ns |       0.2100 ns |        21.769 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              10 |        21.756 ns |      0.1020 ns |       0.0955 ns |        21.773 ns |  1.01 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              11 |        23.042 ns |      0.0521 ns |       0.0487 ns |        23.028 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              11 |        23.048 ns |      0.0566 ns |       0.0530 ns |        23.034 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              12 |        24.785 ns |      0.1018 ns |       0.0902 ns |        24.773 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              12 |        24.803 ns |      0.1389 ns |       0.1299 ns |        24.800 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              13 |        26.369 ns |      0.0316 ns |       0.0264 ns |        26.367 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              13 |        26.479 ns |      0.1255 ns |       0.1174 ns |        26.427 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              14 |        28.194 ns |      0.1505 ns |       0.1408 ns |        28.162 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              14 |        28.165 ns |      0.1365 ns |       0.1277 ns |        28.115 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              15 |        29.737 ns |      0.1491 ns |       0.1394 ns |        29.636 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              15 |        29.834 ns |      0.1467 ns |       0.1373 ns |        29.771 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              16 |        31.564 ns |      0.1464 ns |       0.1370 ns |        31.605 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              16 |        31.543 ns |      0.1635 ns |       0.1529 ns |        31.446 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              17 |        33.191 ns |      0.1545 ns |       0.1445 ns |        33.153 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              17 |        33.279 ns |      0.1908 ns |       0.1785 ns |        33.176 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              18 |        34.917 ns |      0.1912 ns |       0.1789 ns |        34.807 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              18 |        34.954 ns |      0.1473 ns |       0.1378 ns |        34.872 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              19 |        36.621 ns |      0.1246 ns |       0.1165 ns |        36.596 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              19 |        36.587 ns |      0.0829 ns |       0.0692 ns |        36.600 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              20 |        38.281 ns |      0.1486 ns |       0.1390 ns |        38.294 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              20 |        38.347 ns |      0.2297 ns |       0.2149 ns |        38.251 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              30 |        61.011 ns |      0.1297 ns |       0.1083 ns |        60.982 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              30 |        61.004 ns |      0.0610 ns |       0.0541 ns |        60.995 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              32 |        64.886 ns |      0.1906 ns |       0.1690 ns |        64.882 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              32 |        64.847 ns |      0.3077 ns |       0.2878 ns |        64.780 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              40 |        79.658 ns |      0.3185 ns |       0.2980 ns |        79.593 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              40 |        79.288 ns |      0.3209 ns |       0.3002 ns |        79.431 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              50 |        96.836 ns |      0.3645 ns |       0.3410 ns |        96.729 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              50 |        96.285 ns |      0.2424 ns |       0.1893 ns |        96.330 ns |  0.99 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              60 |       112.781 ns |      0.3832 ns |       0.3200 ns |       112.850 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              60 |       112.995 ns |      0.3734 ns |       0.3493 ns |       112.819 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              80 |       146.714 ns |      0.5420 ns |       0.4805 ns |       146.582 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              80 |       147.167 ns |      0.5887 ns |       0.5507 ns |       147.028 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |          850000 | 1,558,675.039 ns |  5,274.7021 ns |   4,933.9596 ns | 1,556,917.773 ns |  1.00 |    0.00 |       1 B |        1.00 |
	|   Actual |          ForEach |         List |         PooledList |          850000 | 1,556,703.841 ns |  3,951.1886 ns |   3,695.9442 ns | 1,556,840.039 ns |  1.00 |    0.00 |       1 B |        1.00 |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |              90 |       163.856 ns |      0.2805 ns |       0.2487 ns |       163.839 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |              90 |       163.976 ns |      0.6668 ns |       0.6237 ns |       163.984 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |             100 |       181.266 ns |      0.7021 ns |       0.6568 ns |       181.080 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |             100 |       181.161 ns |      0.7195 ns |       0.6730 ns |       180.828 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |             128 |       228.416 ns |      0.8094 ns |       0.7571 ns |       227.976 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |             128 |       228.751 ns |      0.9305 ns |       0.8704 ns |       228.228 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |             256 |       445.696 ns |      2.0877 ns |       1.9528 ns |       444.403 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |             256 |       444.787 ns |      0.5112 ns |       0.3991 ns |       444.646 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |             512 |       882.980 ns |      4.3942 ns |       4.1103 ns |       881.532 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |             512 |       881.162 ns |      3.5839 ns |       3.3524 ns |       879.287 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |            1024 |     1,751.893 ns |      5.1841 ns |       4.8492 ns |     1,752.316 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |            1024 |     1,754.110 ns |     10.6107 ns |       9.9253 ns |     1,749.218 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |            2048 |     3,493.803 ns |     11.8092 ns |      10.4686 ns |     3,491.287 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |            2048 |     3,492.052 ns |     14.6699 ns |      13.7223 ns |     3,485.993 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |            4096 |     6,965.861 ns |     29.2659 ns |      27.3753 ns |     6,956.546 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |            4096 |     6,960.693 ns |     27.3000 ns |      25.5365 ns |     6,953.525 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |            8192 |    13,923.282 ns |     18.7357 ns |      16.6087 ns |    13,924.474 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |            8192 |    13,926.325 ns |     26.0192 ns |      23.0654 ns |    13,921.763 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |           16384 |    27,829.084 ns |     12.2988 ns |      10.2700 ns |    27,832.645 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |           16384 |    27,873.496 ns |     62.4882 ns |      55.3941 ns |    27,870.857 ns |  1.00 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |           32768 |    55,711.124 ns |    125.7963 ns |     111.5152 ns |    55,658.243 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |           32768 |    55,701.692 ns |    286.4975 ns |     267.9899 ns |    55,547.827 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |           65536 |   111,533.124 ns |    486.0134 ns |     454.6172 ns |   111,309.283 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |           65536 |   111,594.969 ns |    596.2326 ns |     557.7163 ns |   111,266.650 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |         PooledList |          131072 |   223,540.018 ns |  1,078.1652 ns |   1,008.5164 ns |   223,232.495 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |         PooledList |          131072 |   224,096.100 ns |  1,045.2538 ns |     977.7310 ns |   224,519.482 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               0 |         3.573 ns |      0.0316 ns |       0.0281 ns |         3.560 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               0 |         3.677 ns |      0.0253 ns |       0.0237 ns |         3.667 ns |  1.03 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               1 |         5.725 ns |      0.1359 ns |       0.1565 ns |         5.628 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               1 |         5.386 ns |      0.0805 ns |       0.0672 ns |         5.348 ns |  0.94 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               2 |         7.440 ns |      0.1072 ns |       0.0950 ns |         7.395 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               2 |         6.623 ns |      0.0722 ns |       0.0603 ns |         6.601 ns |  0.89 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               3 |         9.028 ns |      0.0609 ns |       0.0508 ns |         9.009 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               3 |         8.098 ns |      0.0810 ns |       0.0718 ns |         8.072 ns |  0.90 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               4 |        11.223 ns |      0.0851 ns |       0.0711 ns |        11.194 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               4 |         9.936 ns |      0.0387 ns |       0.0302 ns |         9.927 ns |  0.89 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               5 |        12.911 ns |      0.0368 ns |       0.0308 ns |        12.908 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               5 |        11.691 ns |      0.1999 ns |       0.1870 ns |        11.719 ns |  0.90 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               6 |        15.309 ns |      0.1102 ns |       0.1031 ns |        15.270 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               6 |        12.895 ns |      0.0650 ns |       0.0576 ns |        12.880 ns |  0.84 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               7 |        16.970 ns |      0.1357 ns |       0.1203 ns |        16.966 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               7 |        14.904 ns |      0.1461 ns |       0.1366 ns |        14.899 ns |  0.88 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               8 |        18.812 ns |      0.2318 ns |       0.3677 ns |        18.708 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               8 |        16.277 ns |      0.0729 ns |       0.0682 ns |        16.272 ns |  0.86 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |               9 |        20.318 ns |      0.0681 ns |       0.0604 ns |        20.315 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |               9 |        17.955 ns |      0.2289 ns |       0.2141 ns |        17.823 ns |  0.88 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              10 |        22.176 ns |      0.3557 ns |       0.3954 ns |        22.041 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              10 |        19.280 ns |      0.1709 ns |       0.1598 ns |        19.238 ns |  0.87 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              11 |        23.667 ns |      0.0875 ns |       0.0819 ns |        23.649 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              11 |        20.764 ns |      0.1366 ns |       0.1211 ns |        20.706 ns |  0.88 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              12 |        25.399 ns |      0.1055 ns |       0.0881 ns |        25.400 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              12 |        22.212 ns |      0.1188 ns |       0.1112 ns |        22.221 ns |  0.87 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              13 |        27.179 ns |      0.1993 ns |       0.1864 ns |        27.132 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              13 |        23.676 ns |      0.1315 ns |       0.1230 ns |        23.632 ns |  0.87 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              14 |        31.524 ns |      0.8151 ns |       2.4035 ns |        31.432 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              14 |        25.171 ns |      0.0813 ns |       0.0679 ns |        25.188 ns |  0.81 |    0.07 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              15 |        30.444 ns |      0.0938 ns |       0.0783 ns |        30.429 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              15 |        26.533 ns |      0.0907 ns |       0.0758 ns |        26.522 ns |  0.87 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              16 |        32.068 ns |      0.0985 ns |       0.0822 ns |        32.060 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              16 |        28.042 ns |      0.0764 ns |       0.0714 ns |        28.042 ns |  0.87 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              17 |        33.824 ns |      0.1044 ns |       0.0872 ns |        33.824 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              17 |        29.474 ns |      0.0841 ns |       0.0787 ns |        29.472 ns |  0.87 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              18 |        35.611 ns |      0.2112 ns |       0.1764 ns |        35.581 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              18 |        30.986 ns |      0.1966 ns |       0.1839 ns |        30.952 ns |  0.87 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              19 |        37.262 ns |      0.0762 ns |       0.0675 ns |        37.271 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              19 |        32.441 ns |      0.1600 ns |       0.1249 ns |        32.456 ns |  0.87 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              20 |        38.931 ns |      0.1018 ns |       0.0850 ns |        38.930 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              20 |        33.895 ns |      0.1376 ns |       0.1220 ns |        33.951 ns |  0.87 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              30 |        63.098 ns |      0.3730 ns |       0.3307 ns |        63.058 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              30 |        48.477 ns |      0.1267 ns |       0.1123 ns |        48.489 ns |  0.77 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              32 |        66.097 ns |      0.2311 ns |       0.1930 ns |        66.130 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              32 |        58.778 ns |      0.8375 ns |       0.7834 ns |        58.815 ns |  0.89 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              40 |        80.402 ns |      0.3562 ns |       0.3158 ns |        80.421 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              40 |        68.394 ns |      0.3736 ns |       0.3494 ns |        68.285 ns |  0.85 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              50 |        97.995 ns |      0.4361 ns |       0.3866 ns |        98.033 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              50 |        82.702 ns |      0.4232 ns |       0.3959 ns |        82.656 ns |  0.84 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              60 |       114.854 ns |      0.5083 ns |       0.4755 ns |       114.785 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              60 |        97.644 ns |      0.4362 ns |       0.3867 ns |        97.661 ns |  0.85 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              80 |       177.900 ns |      3.5342 ns |       5.3971 ns |       179.072 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              80 |       126.888 ns |      0.4589 ns |       0.4068 ns |       126.923 ns |  0.71 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |             100 |       182.616 ns |      0.7336 ns |       0.6126 ns |       182.544 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |             100 |       155.469 ns |      0.2101 ns |       0.1965 ns |       155.453 ns |  0.85 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |             128 |       228.919 ns |      0.4583 ns |       0.4287 ns |       229.051 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |             128 |       196.699 ns |      0.5857 ns |       0.5192 ns |       196.600 ns |  0.86 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |             256 |       445.757 ns |      0.3324 ns |       0.2776 ns |       445.751 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |             256 |       383.544 ns |      0.8871 ns |       0.7864 ns |       383.443 ns |  0.86 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |             512 |       879.077 ns |      0.7074 ns |       0.5907 ns |       879.168 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |             512 |       757.877 ns |      2.1158 ns |       1.8756 ns |       757.568 ns |  0.86 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |            1024 |     1,756.738 ns |      4.3950 ns |       4.1111 ns |     1,755.500 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |            1024 |     1,507.831 ns |      4.4551 ns |       4.1673 ns |     1,506.162 ns |  0.86 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |            2048 |     3,500.613 ns |     18.6534 ns |      17.4484 ns |     3,491.713 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |            2048 |     3,007.182 ns |      7.3989 ns |       6.9209 ns |     3,006.447 ns |  0.86 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |            4096 |     6,993.660 ns |     41.6187 ns |      38.9301 ns |     6,973.602 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |            4096 |     6,003.662 ns |     39.0598 ns |      36.5366 ns |     5,996.016 ns |  0.86 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |            8192 |    13,917.362 ns |     26.2692 ns |      21.9360 ns |    13,921.748 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |            8192 |    12,020.240 ns |    101.2115 ns |      94.6733 ns |    11,974.687 ns |  0.86 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |           16384 |    34,353.630 ns |    683.8943 ns |   1,690.4173 ns |    34,762.256 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |           16384 |    23,903.155 ns |    140.6087 ns |     131.5254 ns |    23,821.667 ns |  0.70 |    0.05 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |           32768 |    55,753.044 ns |    100.1503 ns |      93.6806 ns |    55,727.875 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |           32768 |    47,899.076 ns |    129.9954 ns |     108.5521 ns |    47,855.658 ns |  0.86 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |           65536 |   112,345.839 ns |    779.9663 ns |     651.3072 ns |   112,174.561 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |           65536 |    96,217.446 ns |    570.3274 ns |     505.5804 ns |    96,063.531 ns |  0.86 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |          131072 |   226,823.985 ns |  1,796.6241 ns |   1,592.6604 ns |   226,966.663 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |          131072 |   192,420.098 ns |    717.9031 ns |     636.4024 ns |   192,319.641 ns |  0.85 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |          850000 | 1,841,711.523 ns | 36,053.3671 ns |  65,925.6204 ns | 1,852,565.723 ns |  1.00 |    0.00 |       1 B |        1.00 |
	|   Actual |          ForEach |         List |     RecyclableList |          850000 | 1,334,551.833 ns |  6,726.8579 ns |   5,617.2310 ns | 1,333,434.180 ns |  0.73 |    0.03 |       1 B |        1.00 |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List |     RecyclableList |              90 |       166.122 ns |      0.6006 ns |       0.5324 ns |       166.059 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List |     RecyclableList |              90 |       141.301 ns |      0.5691 ns |       0.5324 ns |       141.060 ns |  0.85 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               0 |         3.451 ns |      0.0202 ns |       0.0169 ns |         3.444 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               0 |         5.166 ns |      0.0485 ns |       0.0454 ns |         5.138 ns |  1.50 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               1 |         5.210 ns |      0.0255 ns |       0.0226 ns |         5.206 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               1 |         8.314 ns |      0.0363 ns |       0.0303 ns |         8.303 ns |  1.60 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               2 |         7.340 ns |      0.0436 ns |       0.0386 ns |         7.329 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               2 |        11.745 ns |      0.0922 ns |       0.0862 ns |        11.706 ns |  1.60 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               3 |         8.793 ns |      0.1648 ns |       0.1376 ns |         8.770 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               3 |        15.156 ns |      0.0809 ns |       0.0717 ns |        15.132 ns |  1.72 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               4 |        11.502 ns |      0.1494 ns |       0.1247 ns |        11.531 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               4 |        18.547 ns |      0.0601 ns |       0.0532 ns |        18.526 ns |  1.61 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               5 |        12.692 ns |      0.0566 ns |       0.0473 ns |        12.713 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               5 |        22.187 ns |      0.4046 ns |       0.3378 ns |        22.020 ns |  1.75 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               6 |        15.165 ns |      0.2346 ns |       0.2080 ns |        15.204 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               6 |        25.421 ns |      0.0943 ns |       0.0787 ns |        25.405 ns |  1.68 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               7 |        16.639 ns |      0.1436 ns |       0.1344 ns |        16.618 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               7 |        28.744 ns |      0.1388 ns |       0.1084 ns |        28.716 ns |  1.73 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               8 |        18.549 ns |      0.1210 ns |       0.1132 ns |        18.598 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               8 |        32.169 ns |      0.0892 ns |       0.0745 ns |        32.163 ns |  1.73 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |               9 |        20.106 ns |      0.1066 ns |       0.0945 ns |        20.097 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |               9 |        35.541 ns |      0.0549 ns |       0.0458 ns |        35.552 ns |  1.77 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              10 |        21.724 ns |      0.1129 ns |       0.1000 ns |        21.733 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              10 |        39.095 ns |      0.2501 ns |       0.2340 ns |        39.052 ns |  1.80 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              11 |        23.105 ns |      0.0833 ns |       0.0779 ns |        23.075 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              11 |        42.483 ns |      0.1848 ns |       0.1728 ns |        42.447 ns |  1.84 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              12 |        24.848 ns |      0.1406 ns |       0.1316 ns |        24.791 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              12 |        45.916 ns |      0.2962 ns |       0.2626 ns |        45.807 ns |  1.85 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              13 |        26.544 ns |      0.1423 ns |       0.1331 ns |        26.468 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              13 |        49.343 ns |      0.2813 ns |       0.2632 ns |        49.269 ns |  1.86 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              14 |        28.196 ns |      0.1295 ns |       0.1211 ns |        28.152 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              14 |        52.987 ns |      0.4440 ns |       0.4153 ns |        52.900 ns |  1.88 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              15 |        29.980 ns |      0.0719 ns |       0.0600 ns |        29.963 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              15 |        64.152 ns |      0.1469 ns |       0.1227 ns |        64.111 ns |  2.14 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              16 |        31.607 ns |      0.0441 ns |       0.0368 ns |        31.608 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              16 |        66.616 ns |      0.2321 ns |       0.2057 ns |        66.510 ns |  2.11 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              17 |        33.343 ns |      0.1282 ns |       0.1199 ns |        33.326 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              17 |        70.337 ns |      0.1355 ns |       0.1058 ns |        70.312 ns |  2.11 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              18 |        34.964 ns |      0.0774 ns |       0.0646 ns |        34.938 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              18 |        74.920 ns |      0.2134 ns |       0.1666 ns |        74.855 ns |  2.14 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              19 |        36.762 ns |      0.0784 ns |       0.0612 ns |        36.765 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              19 |        76.986 ns |      0.1228 ns |       0.1025 ns |        76.963 ns |  2.09 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              20 |        38.534 ns |      0.0978 ns |       0.0915 ns |        38.542 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              20 |        80.702 ns |      0.4608 ns |       0.4085 ns |        80.555 ns |  2.09 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              30 |        61.583 ns |      0.1261 ns |       0.1118 ns |        61.588 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              30 |       122.136 ns |      2.4919 ns |       7.1898 ns |       119.341 ns |  1.89 |    0.05 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              32 |        66.378 ns |      1.1733 ns |       1.0975 ns |        65.776 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              32 |       123.249 ns |      1.4587 ns |       1.2931 ns |       122.896 ns |  1.86 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              40 |        79.931 ns |      0.5089 ns |       0.4761 ns |        79.871 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              40 |       151.141 ns |      1.6100 ns |       1.4272 ns |       150.947 ns |  1.89 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              50 |        98.275 ns |      1.0477 ns |       0.9287 ns |        97.953 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              50 |       190.028 ns |      3.6509 ns |       4.3461 ns |       189.139 ns |  1.94 |    0.06 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              60 |       115.539 ns |      1.4487 ns |       1.3551 ns |       115.726 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              60 |       221.467 ns |      2.4464 ns |       2.2884 ns |       220.472 ns |  1.92 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              80 |       149.222 ns |      0.6115 ns |       0.5720 ns |       148.952 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              80 |       287.202 ns |      1.7438 ns |       1.6311 ns |       286.990 ns |  1.92 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |          850000 | 1,593,106.270 ns | 29,368.6936 ns |  27,471.4942 ns | 1,584,600.684 ns |  1.00 |    0.00 |       1 B |        1.00 |
	|   Actual |          ForEach |         List | RecyclableLongList |          850000 | 2,811,210.026 ns | 30,468.8839 ns |  28,500.6129 ns | 2,810,521.484 ns |  1.77 |    0.03 |       2 B |        2.00 |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |              90 |       165.541 ns |      0.9212 ns |       0.8617 ns |       165.614 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |              90 |       321.361 ns |      1.9588 ns |       1.7364 ns |       320.982 ns |  1.94 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |             100 |       182.535 ns |      0.9233 ns |       0.8637 ns |       182.525 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |             100 |       363.869 ns |      5.9269 ns |       5.2540 ns |       362.336 ns |  1.99 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |             128 |       231.124 ns |      1.6953 ns |       1.5858 ns |       230.812 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |             128 |       453.238 ns |      2.4329 ns |       2.1567 ns |       452.756 ns |  1.96 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |             256 |       449.973 ns |      1.6447 ns |       1.4580 ns |       450.382 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |             256 |       891.533 ns |      7.1533 ns |       6.6912 ns |       889.027 ns |  1.98 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |             512 |       891.034 ns |      6.6521 ns |       6.2223 ns |       890.716 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |             512 |     1,767.210 ns |     13.6312 ns |      12.0837 ns |     1,763.807 ns |  1.98 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |            1024 |     1,799.927 ns |     29.4035 ns |      26.0654 ns |     1,794.744 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |            1024 |     3,766.511 ns |     56.5563 ns |      52.9028 ns |     3,765.460 ns |  2.09 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |            2048 |     3,577.963 ns |     49.7494 ns |      46.5357 ns |     3,564.046 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |            2048 |     7,463.437 ns |    127.7065 ns |     113.2085 ns |     7,413.597 ns |  2.09 |    0.04 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |            4096 |     8,665.134 ns |    169.5006 ns |     287.8248 ns |     8,612.761 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |            4096 |    14,722.664 ns |    218.2332 ns |     204.1355 ns |    14,610.922 ns |  1.71 |    0.06 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |            8192 |    14,035.711 ns |     64.8120 ns |      60.6252 ns |    14,035.502 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |            8192 |    29,276.958 ns |    275.4928 ns |     257.6961 ns |    29,168.214 ns |  2.09 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |           16384 |    28,146.286 ns |    325.0572 ns |     288.1547 ns |    28,010.663 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |           16384 |    54,165.050 ns |    572.6042 ns |     535.6144 ns |    54,027.719 ns |  1.93 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |           32768 |    56,828.050 ns |    460.5160 ns |     408.2354 ns |    56,894.708 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |           32768 |   109,099.779 ns |  1,653.0396 ns |   1,546.2543 ns |   108,821.436 ns |  1.92 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |           65536 |   112,320.331 ns |    381.9480 ns |     318.9439 ns |   112,323.083 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |           65536 |   215,683.722 ns |  2,713.8804 ns |   2,538.5654 ns |   214,474.780 ns |  1.92 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline |          ForEach |         List | RecyclableLongList |          131072 |   228,741.565 ns |  2,879.6768 ns |   2,552.7585 ns |   228,787.000 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual |          ForEach |         List | RecyclableLongList |          131072 |   437,942.662 ns |  6,268.3292 ns |   5,556.7106 ns |   438,062.964 ns |  1.91 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               0 |         4.170 ns |      0.1081 ns |       0.1061 ns |         4.175 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               0 |         4.102 ns |      0.1025 ns |       0.0909 ns |         4.101 ns |  0.98 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               1 |         5.372 ns |      0.0369 ns |       0.0345 ns |         5.360 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               1 |         5.333 ns |      0.0738 ns |       0.0654 ns |         5.339 ns |  0.99 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               2 |         6.948 ns |      0.0810 ns |       0.0718 ns |         6.924 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               2 |         6.882 ns |      0.0294 ns |       0.0245 ns |         6.877 ns |  0.99 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               3 |         8.612 ns |      0.1212 ns |       0.1074 ns |         8.569 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               3 |         8.670 ns |      0.1966 ns |       0.1742 ns |         8.583 ns |  1.01 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               4 |        11.041 ns |      0.1164 ns |       0.1032 ns |        11.013 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               4 |        11.113 ns |      0.1139 ns |       0.1066 ns |        11.123 ns |  1.01 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               5 |        13.248 ns |      0.2016 ns |       0.1886 ns |        13.259 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               5 |        12.822 ns |      0.0711 ns |       0.0594 ns |        12.834 ns |  0.97 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               6 |        15.078 ns |      0.1963 ns |       0.1639 ns |        15.050 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               6 |        15.034 ns |      0.1051 ns |       0.0932 ns |        15.018 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               7 |        16.736 ns |      0.1050 ns |       0.0931 ns |        16.772 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               7 |        16.611 ns |      0.1433 ns |       0.1341 ns |        16.645 ns |  0.99 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               8 |        18.309 ns |      0.1516 ns |       0.1344 ns |        18.327 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               8 |        18.324 ns |      0.3292 ns |       0.3079 ns |        18.288 ns |  1.00 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |               9 |        20.065 ns |      0.2781 ns |       0.2601 ns |        19.965 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |               9 |        20.001 ns |      0.2718 ns |       0.2542 ns |        19.966 ns |  1.00 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              10 |        21.715 ns |      0.0647 ns |       0.0505 ns |        21.729 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              10 |        22.683 ns |      0.4789 ns |       0.7168 ns |        22.601 ns |  1.03 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              11 |        23.738 ns |      0.2479 ns |       0.2319 ns |        23.766 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              11 |        23.725 ns |      0.3366 ns |       0.2983 ns |        23.608 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              12 |        25.234 ns |      0.2520 ns |       0.2234 ns |        25.148 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              12 |        25.016 ns |      0.1384 ns |       0.1080 ns |        24.993 ns |  0.99 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              13 |        26.903 ns |      0.2184 ns |       0.1936 ns |        26.836 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              13 |        27.121 ns |      0.3033 ns |       0.2837 ns |        26.993 ns |  1.01 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              14 |        28.320 ns |      0.1640 ns |       0.1369 ns |        28.272 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              14 |        28.556 ns |      0.4329 ns |       0.4049 ns |        28.382 ns |  1.01 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              15 |        30.950 ns |      0.4724 ns |       0.4187 ns |        31.055 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              15 |        30.638 ns |      0.4602 ns |       0.4304 ns |        30.511 ns |  0.99 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              16 |        32.515 ns |      0.5508 ns |       0.4882 ns |        32.655 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              16 |        32.572 ns |      0.5061 ns |       0.4734 ns |        32.470 ns |  1.00 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              17 |        33.724 ns |      0.1540 ns |       0.1366 ns |        33.712 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              17 |        33.862 ns |      0.3266 ns |       0.3055 ns |        33.839 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              18 |        35.814 ns |      0.4675 ns |       0.4144 ns |        35.749 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              18 |        36.046 ns |      0.6879 ns |       0.6098 ns |        35.929 ns |  1.01 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              19 |        37.620 ns |      0.6326 ns |       0.5283 ns |        37.466 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              19 |        37.442 ns |      0.5918 ns |       0.5246 ns |        37.425 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              20 |        39.292 ns |      0.4505 ns |       0.3762 ns |        39.267 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              20 |        39.344 ns |      0.4094 ns |       0.3629 ns |        39.244 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              30 |        62.338 ns |      0.7110 ns |       0.6651 ns |        62.086 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              30 |        62.490 ns |      0.5156 ns |       0.4570 ns |        62.379 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              32 |        65.698 ns |      0.1781 ns |       0.1391 ns |        65.679 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              32 |        65.858 ns |      0.3777 ns |       0.3533 ns |        65.727 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              40 |        79.678 ns |      0.2551 ns |       0.2262 ns |        79.729 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              40 |        80.179 ns |      0.2988 ns |       0.2333 ns |        80.255 ns |  1.01 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              50 |        98.581 ns |      1.2428 ns |       1.7011 ns |        98.038 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              50 |        98.365 ns |      0.9843 ns |       0.8219 ns |        98.489 ns |  0.99 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              60 |       115.812 ns |      1.5418 ns |       1.4422 ns |       115.515 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              60 |       114.660 ns |      0.5155 ns |       0.4570 ns |       114.585 ns |  0.99 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              80 |       149.044 ns |      0.7603 ns |       0.6740 ns |       148.779 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              80 |       149.237 ns |      0.8349 ns |       0.7810 ns |       149.142 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |              90 |       165.196 ns |      0.6880 ns |       0.6435 ns |       164.967 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |              90 |       165.244 ns |      0.8600 ns |       0.7623 ns |       165.061 ns |  1.00 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |             100 |       182.624 ns |      0.6592 ns |       0.5844 ns |       182.625 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |             100 |       185.868 ns |      2.1608 ns |       2.0212 ns |       185.058 ns |  1.02 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |             128 |       232.270 ns |      2.7215 ns |       2.4126 ns |       230.876 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |             128 |       234.566 ns |      4.0220 ns |       3.5654 ns |       233.897 ns |  1.01 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |             256 |       454.474 ns |      6.7036 ns |       6.2706 ns |       451.345 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |             256 |       461.572 ns |      8.9902 ns |       8.4095 ns |       458.094 ns |  1.02 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |             512 |       901.971 ns |     14.3278 ns |      12.7012 ns |       900.796 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |             512 |       912.171 ns |     12.3817 ns |      11.5818 ns |       913.911 ns |  1.01 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |            1024 |     1,808.524 ns |     27.9056 ns |      26.1029 ns |     1,810.923 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |            1024 |     1,795.226 ns |     23.8849 ns |      22.3420 ns |     1,793.173 ns |  0.99 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |            2048 |     3,583.843 ns |     70.4928 ns |      72.3909 ns |     3,562.454 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |            2048 |     4,244.738 ns |     84.3127 ns |     115.4082 ns |     4,258.823 ns |  1.20 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |            4096 |     7,142.871 ns |    101.4314 ns |      94.8790 ns |     7,118.380 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |            4096 |     7,160.909 ns |    115.3247 ns |     113.2643 ns |     7,109.839 ns |  1.00 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |            8192 |    14,382.896 ns |    277.8337 ns |     246.2923 ns |    14,346.375 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |            8192 |    14,308.837 ns |    161.4476 ns |     151.0182 ns |    14,350.789 ns |  0.99 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |           16384 |    28,230.601 ns |    155.4418 ns |     145.4004 ns |    28,236.281 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |           16384 |    28,502.915 ns |    534.9612 ns |     446.7169 ns |    28,418.047 ns |  1.01 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |           32768 |    56,834.798 ns |    377.5454 ns |     334.6841 ns |    56,735.428 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |           32768 |    57,354.676 ns |  1,097.2304 ns |   1,077.6270 ns |    56,917.178 ns |  1.01 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |           65536 |   116,209.092 ns |  1,390.8538 ns |   1,085.8872 ns |   116,394.702 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |           65536 |   114,665.507 ns |  1,166.8947 ns |     974.4099 ns |   114,299.097 ns |  0.99 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |          131072 |   231,587.781 ns |  3,052.2006 ns |   2,855.0303 ns |   230,845.203 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |         PooledList |          131072 |   226,839.675 ns |  2,570.5076 ns |   2,146.4903 ns |   226,250.806 ns |  0.98 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |         PooledList |          850000 | 1,580,908.329 ns | 11,062.7560 ns |   9,806.8451 ns | 1,581,162.109 ns |  1.00 |    0.00 |       1 B |        1.00 |
	|   Actual | VersionedForEach |         List |         PooledList |          850000 | 1,600,714.896 ns | 22,235.0178 ns |  20,798.6495 ns | 1,598,917.578 ns |  1.01 |    0.01 |       1 B |        1.00 |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               0 |         3.662 ns |      0.0151 ns |       0.0134 ns |         3.661 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               0 |         3.632 ns |      0.1013 ns |       0.1126 ns |         3.630 ns |  1.00 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               1 |         6.073 ns |      0.1313 ns |       0.2265 ns |         6.034 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               1 |         5.877 ns |      0.1473 ns |       0.3473 ns |         5.900 ns |  0.97 |    0.06 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               2 |         7.770 ns |      0.1630 ns |       0.2176 ns |         7.755 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               2 |         6.913 ns |      0.1515 ns |       0.1417 ns |         6.864 ns |  0.89 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               3 |         9.736 ns |      0.2251 ns |       0.4117 ns |         9.626 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               3 |         8.358 ns |      0.1936 ns |       0.1811 ns |         8.398 ns |  0.86 |    0.04 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               4 |        11.690 ns |      0.2498 ns |       0.2336 ns |        11.674 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               4 |        10.303 ns |      0.1442 ns |       0.1278 ns |        10.304 ns |  0.88 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               5 |        13.115 ns |      0.1670 ns |       0.1562 ns |        13.049 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               5 |        11.831 ns |      0.1823 ns |       0.1705 ns |        11.848 ns |  0.90 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               6 |        15.511 ns |      0.1763 ns |       0.1472 ns |        15.439 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               6 |        13.673 ns |      0.3035 ns |       0.3117 ns |        13.633 ns |  0.88 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               7 |        17.417 ns |      0.2872 ns |       0.2686 ns |        17.482 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               7 |        14.905 ns |      0.2715 ns |       0.2540 ns |        14.901 ns |  0.86 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               8 |        19.079 ns |      0.2278 ns |       0.2019 ns |        19.041 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               8 |        16.566 ns |      0.1929 ns |       0.1805 ns |        16.494 ns |  0.87 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |               9 |        21.106 ns |      0.3345 ns |       0.2965 ns |        21.117 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |               9 |        18.313 ns |      0.3845 ns |       0.3210 ns |        18.159 ns |  0.87 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              10 |        23.688 ns |      0.4888 ns |       1.2705 ns |        23.253 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              10 |        21.041 ns |      0.4498 ns |       0.8983 ns |        20.882 ns |  0.89 |    0.06 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              11 |        24.481 ns |      0.4602 ns |       0.7165 ns |        24.251 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              11 |        21.279 ns |      0.2309 ns |       0.2047 ns |        21.200 ns |  0.87 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              12 |        26.202 ns |      0.5516 ns |       0.5418 ns |        26.021 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              12 |        23.170 ns |      0.4547 ns |       0.4031 ns |        23.026 ns |  0.88 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              13 |        27.707 ns |      0.4782 ns |       0.4239 ns |        27.469 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              13 |        24.329 ns |      0.2913 ns |       0.2725 ns |        24.212 ns |  0.88 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              14 |        29.367 ns |      0.2983 ns |       0.2491 ns |        29.285 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              14 |        26.011 ns |      0.3929 ns |       0.3676 ns |        25.917 ns |  0.88 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              15 |        32.009 ns |      0.4594 ns |       0.3836 ns |        31.969 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              15 |        27.833 ns |      0.4437 ns |       0.3705 ns |        27.864 ns |  0.87 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              16 |        33.019 ns |      0.3113 ns |       0.2759 ns |        32.962 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              16 |        29.823 ns |      0.4110 ns |       0.3643 ns |        29.761 ns |  0.90 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              17 |        35.153 ns |      0.4993 ns |       0.4426 ns |        35.124 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              17 |        31.257 ns |      0.6326 ns |       0.5917 ns |        31.035 ns |  0.89 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              18 |        36.654 ns |      0.5923 ns |       0.5250 ns |        36.407 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              18 |        32.631 ns |      0.6588 ns |       0.6470 ns |        32.528 ns |  0.89 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              19 |        39.171 ns |      0.4820 ns |       0.4508 ns |        39.156 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              19 |        33.975 ns |      0.5713 ns |       0.5344 ns |        33.818 ns |  0.87 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              20 |        39.829 ns |      0.6428 ns |       0.6013 ns |        39.617 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              20 |        34.932 ns |      0.5515 ns |       0.5158 ns |        34.754 ns |  0.88 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              30 |        64.842 ns |      1.3032 ns |       1.1552 ns |        64.657 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              30 |        50.514 ns |      0.5255 ns |       0.4388 ns |        50.449 ns |  0.78 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              32 |        69.780 ns |      1.4282 ns |       2.2235 ns |        69.112 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              32 |        61.867 ns |      0.9904 ns |       0.8780 ns |        61.893 ns |  0.89 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              40 |        83.162 ns |      0.7948 ns |       0.7046 ns |        83.105 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              40 |        73.047 ns |      0.8652 ns |       0.8093 ns |        73.062 ns |  0.88 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              50 |       100.518 ns |      1.3208 ns |       1.1709 ns |       100.204 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              50 |        87.612 ns |      0.6497 ns |       0.5760 ns |        87.488 ns |  0.87 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              60 |       115.416 ns |      0.6522 ns |       0.5092 ns |       115.159 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              60 |       101.275 ns |      0.6366 ns |       0.5955 ns |       100.980 ns |  0.88 |    0.00 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              80 |       180.498 ns |      3.5968 ns |       4.6768 ns |       182.070 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              80 |       130.384 ns |      0.6436 ns |       0.6020 ns |       130.245 ns |  0.72 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |              90 |       166.748 ns |      0.8692 ns |       0.8131 ns |       166.924 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |              90 |       147.034 ns |      2.2523 ns |       2.1068 ns |       145.965 ns |  0.88 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |             100 |       187.028 ns |      3.1350 ns |       2.7791 ns |       186.818 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |             100 |       161.191 ns |      1.8091 ns |       1.6922 ns |       160.552 ns |  0.86 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |             128 |       232.149 ns |      2.2247 ns |       2.0810 ns |       231.116 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |             128 |       210.336 ns |      2.2589 ns |       2.1130 ns |       209.626 ns |  0.91 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |             256 |       454.603 ns |      6.4374 ns |       6.0216 ns |       452.711 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |             256 |       415.848 ns |      7.2011 ns |       7.7051 ns |       413.804 ns |  0.92 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |             512 |       887.438 ns |      5.3199 ns |       4.7160 ns |       885.431 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |             512 |       817.053 ns |      7.9832 ns |       7.0769 ns |       815.831 ns |  0.92 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |            1024 |     1,777.287 ns |     15.3636 ns |      14.3712 ns |     1,773.709 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |            1024 |     1,620.522 ns |     13.0642 ns |      11.5811 ns |     1,622.521 ns |  0.91 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |            2048 |     3,555.328 ns |     49.0378 ns |      45.8700 ns |     3,531.557 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |            2048 |     3,226.549 ns |     20.9797 ns |      19.6244 ns |     3,231.659 ns |  0.91 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |            4096 |     7,083.254 ns |     40.2312 ns |      31.4099 ns |     7,083.910 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |            4096 |     6,391.336 ns |     45.3105 ns |      40.1665 ns |     6,381.340 ns |  0.90 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |            8192 |    14,150.508 ns |    160.7683 ns |     142.5169 ns |    14,103.947 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |            8192 |    12,951.632 ns |    252.7588 ns |     236.4308 ns |    12,839.685 ns |  0.92 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |           16384 |    28,174.120 ns |    219.4159 ns |     205.2418 ns |    28,125.076 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |           16384 |    24,465.357 ns |    408.7098 ns |     382.3074 ns |    24,296.729 ns |  0.87 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |           32768 |    56,579.534 ns |    585.3275 ns |     518.8776 ns |    56,425.235 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |           32768 |    48,830.217 ns |    473.5733 ns |     419.8104 ns |    48,699.948 ns |  0.86 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |           65536 |   142,610.610 ns |  1,346.8333 ns |   1,259.8287 ns |   143,257.397 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |           65536 |    98,560.833 ns |  1,133.4299 ns |   1,060.2110 ns |    98,441.614 ns |  0.69 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |          131072 |   227,308.963 ns |  2,539.6607 ns |   2,251.3430 ns |   226,288.538 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List |     RecyclableList |          131072 |   197,657.012 ns |  2,737.4239 ns |   2,426.6550 ns |   196,840.356 ns |  0.87 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List |     RecyclableList |          850000 | 1,614,043.960 ns | 11,889.6846 ns |   9,928.4251 ns | 1,613,390.625 ns |  1.00 |    0.00 |       1 B |        1.00 |
	|   Actual | VersionedForEach |         List |     RecyclableList |          850000 | 1,349,524.623 ns |  2,630.5316 ns |   2,331.8978 ns | 1,349,204.688 ns |  0.84 |    0.01 |       1 B |        1.00 |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               0 |         3.523 ns |      0.0447 ns |       0.0373 ns |         3.517 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               0 |         9.324 ns |      0.0671 ns |       0.0628 ns |         9.294 ns |  2.65 |    0.04 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               1 |         5.318 ns |      0.0518 ns |       0.0459 ns |         5.312 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               1 |        14.197 ns |      0.3033 ns |       0.3943 ns |        14.160 ns |  2.67 |    0.09 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               2 |         6.915 ns |      0.0277 ns |       0.0231 ns |         6.910 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               2 |        18.310 ns |      0.3390 ns |       0.4408 ns |        18.354 ns |  2.62 |    0.06 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               3 |         8.764 ns |      0.0951 ns |       0.0843 ns |         8.786 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               3 |        21.395 ns |      0.2388 ns |       0.2117 ns |        21.393 ns |  2.44 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               4 |        11.563 ns |      0.2572 ns |       0.2406 ns |        11.552 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               4 |        26.309 ns |      0.2658 ns |       0.2486 ns |        26.325 ns |  2.28 |    0.05 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               5 |        12.933 ns |      0.0969 ns |       0.0859 ns |        12.918 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               5 |        29.433 ns |      0.5992 ns |       0.6660 ns |        29.147 ns |  2.28 |    0.06 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               6 |        15.392 ns |      0.3293 ns |       0.3524 ns |        15.318 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               6 |        32.820 ns |      0.4798 ns |       0.4488 ns |        32.733 ns |  2.13 |    0.06 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               7 |        16.698 ns |      0.1192 ns |       0.0995 ns |        16.723 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               7 |        36.723 ns |      0.5201 ns |       0.4865 ns |        36.727 ns |  2.20 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               8 |        18.550 ns |      0.1663 ns |       0.1474 ns |        18.533 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               8 |        43.143 ns |      0.4068 ns |       0.3805 ns |        42.933 ns |  2.32 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |               9 |        21.331 ns |      0.4478 ns |       0.7481 ns |        21.301 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |               9 |        45.471 ns |      0.8887 ns |       2.1800 ns |        44.451 ns |  2.19 |    0.15 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              10 |        21.776 ns |      0.2230 ns |       0.2086 ns |        21.755 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              10 |        48.297 ns |      0.4689 ns |       0.4387 ns |        48.206 ns |  2.22 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              11 |        23.557 ns |      0.3304 ns |       0.2929 ns |        23.481 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              11 |        52.951 ns |      0.4832 ns |       0.4035 ns |        53.070 ns |  2.25 |    0.04 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              12 |        25.365 ns |      0.2192 ns |       0.1831 ns |        25.369 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              12 |        56.237 ns |      0.9406 ns |       0.8798 ns |        56.478 ns |  2.21 |    0.04 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              13 |        26.914 ns |      0.3191 ns |       0.2829 ns |        26.829 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              13 |        60.108 ns |      0.6361 ns |       0.5950 ns |        60.158 ns |  2.23 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              14 |        28.940 ns |      0.4242 ns |       0.3968 ns |        28.773 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              14 |        65.173 ns |      1.3081 ns |       1.3434 ns |        65.266 ns |  2.25 |    0.04 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              15 |        30.596 ns |      0.4513 ns |       0.3523 ns |        30.583 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              15 |        76.328 ns |      1.3343 ns |       1.2481 ns |        75.702 ns |  2.49 |    0.05 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              16 |        32.729 ns |      0.3667 ns |       0.3062 ns |        32.796 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              16 |        86.312 ns |      0.7071 ns |       0.5905 ns |        86.331 ns |  2.64 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              17 |        34.076 ns |      0.4444 ns |       0.4157 ns |        33.971 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              17 |        83.004 ns |      0.4126 ns |       0.3658 ns |        82.970 ns |  2.43 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              18 |        35.417 ns |      0.3067 ns |       0.2868 ns |        35.246 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              18 |        86.030 ns |      0.4640 ns |       0.3875 ns |        85.857 ns |  2.43 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              19 |        36.841 ns |      0.1066 ns |       0.0890 ns |        36.810 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              19 |        90.468 ns |      0.3312 ns |       0.3098 ns |        90.335 ns |  2.45 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              20 |        38.512 ns |      0.1351 ns |       0.1055 ns |        38.488 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              20 |        94.096 ns |      1.3335 ns |       1.1821 ns |        94.056 ns |  2.45 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              30 |        61.832 ns |      0.2697 ns |       0.2522 ns |        61.739 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              30 |       130.708 ns |      0.7176 ns |       0.6712 ns |       130.421 ns |  2.11 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              32 |        65.344 ns |      0.4106 ns |       0.3841 ns |        65.374 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              32 |       150.613 ns |      0.8025 ns |       0.6701 ns |       150.505 ns |  2.30 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              40 |        80.571 ns |      0.3720 ns |       0.3106 ns |        80.717 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              40 |       171.390 ns |      2.4117 ns |       2.2559 ns |       171.875 ns |  2.12 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              50 |        97.366 ns |      1.3366 ns |       1.1848 ns |        96.912 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              50 |       210.867 ns |      2.2683 ns |       2.1217 ns |       210.785 ns |  2.16 |    0.04 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              60 |       120.541 ns |      2.4415 ns |       3.6543 ns |       119.941 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              60 |       253.299 ns |      3.7372 ns |       3.4958 ns |       252.504 ns |  2.09 |    0.09 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              80 |       154.351 ns |      3.0279 ns |       2.8323 ns |       153.659 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              80 |       333.286 ns |      6.6353 ns |       9.7259 ns |       331.267 ns |  2.19 |    0.08 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |             100 |       189.679 ns |      3.8158 ns |       9.8497 ns |       185.357 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |             100 |       403.809 ns |      3.5781 ns |       2.9879 ns |       402.469 ns |  2.14 |    0.11 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |             128 |       231.559 ns |      1.8376 ns |       1.7189 ns |       231.672 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |             128 |       550.164 ns |      3.2537 ns |       3.0435 ns |       549.213 ns |  2.38 |    0.03 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |             256 |       450.065 ns |      2.7403 ns |       2.4292 ns |       449.653 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |             256 |     1,082.522 ns |      6.6654 ns |       6.2349 ns |     1,079.465 ns |  2.41 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |             512 |       887.143 ns |      4.1466 ns |       3.8787 ns |       886.272 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |             512 |     2,150.602 ns |     12.2585 ns |      11.4666 ns |     2,157.932 ns |  2.42 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |            1024 |     1,764.301 ns |      8.4588 ns |       7.9123 ns |     1,765.930 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |            1024 |     4,286.746 ns |     13.4166 ns |      12.5499 ns |     4,286.220 ns |  2.43 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |            2048 |     4,216.533 ns |     85.4917 ns |     252.0742 ns |     4,298.388 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |            2048 |     8,509.223 ns |     12.4561 ns |      11.0420 ns |     8,506.629 ns |  2.02 |    0.15 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |            4096 |     8,676.483 ns |    167.8749 ns |     199.8431 ns |     8,712.919 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |            4096 |    17,049.051 ns |     82.0162 ns |      76.7180 ns |    17,041.325 ns |  1.96 |    0.05 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |            8192 |    13,938.398 ns |     44.9571 ns |      39.8533 ns |    13,922.381 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |            8192 |    34,133.194 ns |    181.2965 ns |     169.5849 ns |    34,120.786 ns |  2.45 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |           16384 |    27,927.112 ns |    189.2721 ns |     167.7848 ns |    27,840.433 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |           16384 |    61,271.269 ns |    191.0036 ns |     169.3197 ns |    61,248.755 ns |  2.19 |    0.02 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |           32768 |    55,954.745 ns |    178.5899 ns |     167.0531 ns |    55,953.790 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |           32768 |   122,397.876 ns |    795.0836 ns |     743.7217 ns |   122,076.416 ns |  2.19 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |           65536 |   111,873.295 ns |    466.3331 ns |     436.2083 ns |   111,790.027 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |           65536 |   244,920.799 ns |  1,173.3577 ns |   1,097.5596 ns |   245,148.096 ns |  2.19 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |          131072 |   224,331.458 ns |  1,277.0991 ns |   1,194.5993 ns |   224,404.077 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |          131072 |   490,011.882 ns |  3,335.1804 ns |   3,119.7298 ns |   488,019.141 ns |  2.18 |    0.01 |         - |          NA |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |          850000 | 1,704,467.372 ns | 33,215.7962 ns |  83,331.9381 ns | 1,689,684.766 ns |  1.00 |    0.00 |       1 B |        1.00 |
	|   Actual | VersionedForEach |         List | RecyclableLongList |          850000 | 3,599,336.695 ns | 70,531.2969 ns | 153,329.2348 ns | 3,578,202.930 ns |  2.11 |    0.16 |       2 B |        2.00 |
	|          |                  |              |                    |                 |                  |                |                 |                  |       |         |           |             |
	| Baseline | VersionedForEach |         List | RecyclableLongList |              90 |       169.352 ns |      2.9741 ns |       6.4654 ns |       166.354 ns |  1.00 |    0.00 |         - |          NA |
	|   Actual | VersionedForEach |         List | RecyclableLongList |              90 |       360.043 ns |      2.4994 ns |       2.3380 ns |       359.931 ns |  2.04 |    0.08 |         - |          NA |
	</details>

## `RecyclableList<T>`
* Range: `int`
* Interfaces: `IList<T>`, `IEnumerable<T>`, `IDisposable`
This is the direct equivalent of `List<T>` class, except that the arrays are taken from the shared pool, instead of directly being allocated. It proves to out perform `List<T>` & the standard arrays in certain operations and is fairly close in others. But they may perform worse in certain scenarios. If you're interested in details, please refer to benchmarks.

## `RecyclableLongList<T>`, `RecyclableSortableList<T>`, `RecyclableQueue<T>`, `RecyclableStack<T>`
* Range: `long`
* Interfaces: `IList<T>`, `ILongList<T>`, `IEnumerable<T>`, `IDisposable`
* All classes provide large storage capabilities, in practice limited only by the memory available in your system.
* All data is stored in blocks, with the provided `int blockSize` or the default, currently being `10,240` items in each block. It's recommended to use smaller numbers, if you foresee storing low no. of items on the list.
* There is no memory copying to increase the capacity of the recyclable classes, unless the no. of items exceeds the no. of allocated blocks. In that case, a new array of blocks is created to accommodate . If more capacity is needed, a new memory block is allocate & added to the internal list of blocks. This is the only list that may re-allocate memory block & copy their content to grow. That shouldn't be an issue considering limited no. of blocks stored by each class in practical scenarios. Currently I'm not planning any works around that. Shall there be a need to eliminate it, you're welcomed to file a PR with the proposed changes.
* ⚠️ `IndexOf` will automatically switch to parallel search mode. Collections bigger than (currently) 850_000 items will be scanned using parallelization. In either case it'll return the index of the first matching item. This note also applies to `Contains`, because it internally uses `IndexOf`. The value of 850_000 may become configurable in the future, unless it proves to result in much worse performance.

	The parallelization utilizes default `Task` scheduling, resulting in non-deterministic order in which search tasks are executed. In most cases it resulted in significant performance improvements. Below you can find the benchmarks for various item counts.

	### `IndexOf` Benchmarks
	```csharp
	BenchmarkDotNet=v0.13.3, OS=Windows 11 (10.0.22621.1702)
	AMD Ryzen 7 4700U with Radeon Graphics, 1 CPU, 8 logical and 8 physical cores
	.NET SDK=7.0.203
	  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
	  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
	```
	<details>
	<summary><strong>IndexOf Benchmark Results - click to expand</strong></summary>

	|   Method |             TestCase | BaselineBenchmarkType |      BenchmarkType | TestObjectCount |                  Mean |                Error |             StdDev |                Median | Ratio |     RatioSD | Allocated  | Alloc Ratio |
	|--------- |--------------------- |---------------------- |------------------- |---------------- |----------------------:|---------------------:|-------------------:|----------------------:|------:|------------:|-----------:|------------:|
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |               1 |              16.60 ns |             0.060 ns |           0.056 ns |              16.57 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |               1 |              19.20 ns |             0.137 ns |           0.121 ns |              19.16 ns |  1.16 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |               2 |              27.64 ns |             0.188 ns |           0.176 ns |              27.65 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |               2 |              31.90 ns |             0.102 ns |           0.095 ns |              31.89 ns |  1.15 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |               3 |              28.35 ns |             0.146 ns |           0.137 ns |              28.42 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |               3 |              32.63 ns |             0.177 ns |           0.166 ns |              32.55 ns |  1.15 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |               4 |              39.05 ns |             0.112 ns |           0.105 ns |              38.99 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |               4 |              44.01 ns |             0.168 ns |           0.149 ns |              43.98 ns |  1.13 |        0.00 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |               5 |              39.77 ns |             0.163 ns |           0.152 ns |              39.69 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |               5 |              46.72 ns |             0.943 ns |           1.258 ns |              46.87 ns |  1.17 |        0.03 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |               6 |              51.14 ns |             0.202 ns |           0.189 ns |              51.10 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |               6 |              59.75 ns |             1.076 ns |           1.105 ns |              59.75 ns |  1.17 |        0.02 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |               7 |              51.60 ns |             0.126 ns |           0.105 ns |              51.58 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |               7 |              58.40 ns |             1.125 ns |           0.997 ns |              58.38 ns |  1.13 |        0.02 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |               8 |              61.22 ns |             0.301 ns |           0.281 ns |              61.22 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |               8 |              73.96 ns |             0.397 ns |           0.372 ns |              73.98 ns |  1.21 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |               9 |              62.21 ns |             0.074 ns |           0.062 ns |              62.22 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |               9 |              96.22 ns |             0.712 ns |           0.631 ns |              96.16 ns |  1.55 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              10 |              75.18 ns |             0.244 ns |           0.228 ns |              75.07 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              10 |              83.85 ns |             0.224 ns |           0.209 ns |              83.74 ns |  1.12 |        0.00 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              11 |              74.93 ns |             0.214 ns |           0.200 ns |              74.86 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              11 |              83.78 ns |             0.484 ns |           0.453 ns |              83.53 ns |  1.12 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              12 |              86.86 ns |             0.316 ns |           0.295 ns |              86.88 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              12 |             106.61 ns |             1.246 ns |           1.224 ns |             106.88 ns |  1.23 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              13 |              88.02 ns |             0.273 ns |           0.255 ns |              87.90 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              13 |              96.74 ns |             0.074 ns |           0.066 ns |              96.73 ns |  1.10 |        0.00 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              14 |             100.74 ns |             0.344 ns |           0.322 ns |             100.79 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              14 |             110.46 ns |             0.359 ns |           0.336 ns |             110.27 ns |  1.10 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              15 |             100.67 ns |             0.405 ns |           0.359 ns |             100.55 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              15 |             127.81 ns |             1.472 ns |           1.305 ns |             128.04 ns |  1.27 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              16 |             114.05 ns |             0.520 ns |           0.486 ns |             113.75 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              16 |             141.99 ns |             0.510 ns |           0.477 ns |             141.82 ns |  1.24 |        0.00 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              17 |             114.63 ns |             0.432 ns |           0.404 ns |             114.41 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              17 |             125.91 ns |             0.363 ns |           0.322 ns |             125.79 ns |  1.10 |        0.00 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              18 |             127.90 ns |             0.404 ns |           0.378 ns |             128.07 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              18 |             140.47 ns |             0.452 ns |           0.423 ns |             140.24 ns |  1.10 |        0.00 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              19 |             128.56 ns |             0.375 ns |           0.350 ns |             128.41 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              19 |             152.84 ns |             1.948 ns |           1.822 ns |             152.48 ns |  1.19 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              20 |             150.88 ns |             0.538 ns |           0.477 ns |             150.97 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              20 |             184.59 ns |             0.871 ns |           0.772 ns |             184.61 ns |  1.22 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              30 |             217.63 ns |             0.967 ns |           0.905 ns |             217.71 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              30 |             244.89 ns |             1.417 ns |           1.326 ns |             244.56 ns |  1.13 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              32 |             266.32 ns |             2.378 ns |           2.224 ns |             266.71 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              32 |             310.60 ns |             1.946 ns |           1.820 ns |             310.93 ns |  1.17 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              40 |             362.07 ns |             2.741 ns |           2.564 ns |             362.60 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              40 |             401.02 ns |             1.604 ns |           1.500 ns |             400.31 ns |  1.11 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              50 |             400.35 ns |             3.445 ns |           3.223 ns |             400.41 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              50 |             485.05 ns |             3.031 ns |           2.531 ns |             485.54 ns |  1.21 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              60 |             582.48 ns |             4.689 ns |           4.386 ns |             581.95 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              60 |             664.89 ns |             3.413 ns |           3.192 ns |             663.75 ns |  1.14 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              80 |             845.29 ns |             7.416 ns |           6.937 ns |             843.90 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              80 |             915.71 ns |             5.696 ns |           5.328 ns |             916.91 ns |  1.08 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |              90 |             994.91 ns |            15.228 ns |          14.244 ns |             998.73 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |              90 |           1,074.04 ns |            12.721 ns |          11.277 ns |           1,077.75 ns |  1.08 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |             100 |           1,130.91 ns |            20.393 ns |          19.076 ns |           1,130.99 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |             100 |           1,185.56 ns |            23.173 ns |          21.676 ns |           1,184.10 ns |  1.05 |        0.02 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |             128 |           1,610.08 ns |            16.899 ns |          15.807 ns |           1,604.90 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |             128 |           1,753.55 ns |            17.254 ns |          16.140 ns |           1,752.63 ns |  1.09 |        0.02 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |             256 |           5,750.75 ns |            28.609 ns |          26.760 ns |           5,758.04 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |             256 |           5,893.43 ns |            29.453 ns |          27.551 ns |           5,887.99 ns |  1.02 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |             512 |          21,669.95 ns |           427.575 ns |         399.954 ns |          21,713.08 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |             512 |          21,674.73 ns |            68.941 ns |          64.487 ns |          21,668.86 ns |  1.00 |        0.02 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |            1024 |          69,740.31 ns |           838.846 ns |         932.375 ns |          69,475.32 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |            1024 |          68,980.90 ns |           450.707 ns |         421.591 ns |          68,923.79 ns |  0.99 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |            2048 |         244,266.95 ns |         2,449.277 ns |       2,045.257 ns |         243,462.06 ns |  1.00 |        0.00 |           - |          NA |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |            2048 |         239,887.40 ns |           854.306 ns |         799.118 ns |         240,015.48 ns |  0.98 |        0.01 |           - |          NA |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |            4096 |         904,644.52 ns |         3,379.464 ns |       2,638.463 ns |         904,380.27 ns |  1.00 |        0.00 |         1 B |        1.00 |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |            4096 |         915,459.26 ns |         6,529.766 ns |       6,107.947 ns |         917,327.34 ns |  1.01 |        0.01 |         1 B |        1.00 |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |            8192 |       3,513,716.89 ns |        62,893.465 ns |      90,199.928 ns |       3,474,476.56 ns |  1.00 |        0.00 |         2 B |        1.00 |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |            8192 |       3,486,690.89 ns |        14,705.600 ns |      13,755.627 ns |       3,481,303.91 ns |  0.99 |        0.02 |         2 B |        1.00 |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |           16384 |      13,736,865.85 ns |       120,702.128 ns |     106,999.292 ns |      13,714,744.53 ns |  1.00 |        0.00 |         9 B |        1.00 |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |           16384 |      14,167,800.31 ns |        43,856.428 ns |      41,023.330 ns |      14,149,225.00 ns |  1.03 |        0.01 |         9 B |        1.00 |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |           32768 |      53,875,332.00 ns |       245,220.180 ns |     229,379.108 ns |      53,809,450.00 ns |  1.00 |        0.00 |        60 B |        1.00 |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |           32768 |      55,149,804.00 ns |       273,037.835 ns |     255,399.760 ns |      55,076,500.00 ns |  1.02 |        0.01 |        60 B |        1.00 |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |           65536 |     218,431,663.33 ns |     4,265,864.273 ns |   4,912,575.640 ns |     214,920,550.00 ns |  1.00 |        0.00 |       200 B |        1.00 |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |           65536 |     217,449,610.26 ns |       493,374.080 ns |     411,989.702 ns |     217,368,133.33 ns |  1.01 |        0.02 |       200 B |        1.00 |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |          131072 |     936,350,615.38 ns |     2,290,617.990 ns |   1,912,769.764 ns |     936,031,100.00 ns |  1.00 |        0.00 |       600 B |        1.00 |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |          131072 |     928,423,416.67 ns |     1,034,931.818 ns |     808,006.689 ns |     928,198,750.00 ns |  0.99 |        0.00 |       600 B |        1.00 |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |          849999 |  99,588,892,028.57 ns |   998,348,445.797 ns | 885,009,893.256 ns |  99,218,977,100.00 ns |  1.00 |        0.00 |       600 B |        1.00 |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |          849999 | 102,323,847,080.00 ns | 1,028,672,431.300 ns | 962,220,830.472 ns | 101,982,058,200.00 ns |  1.03 |        0.01 |       600 B |        1.00 |
	|          |                      |                       |                    |                 |                       |                      |                    |                       |       |             |             |             |
	| Baseline | Index(...)Cases [25] |                  List | RecyclableLongList |          850000 |               113.8 s |               2.26 s |             2.69 s |                       |  1.00 |        0.00 |       600 B |        1.00 |
	|   Actual | Index(...)Cases [25] |                  List | RecyclableLongList |          850000 |               110.3 s |               2.11 s |             2.07 s |                       |  0.97 |        0.03 | 399750776 B |  666,251.29 |
	</details>

	As `RecyclableLongList<T>` is foreseen for use-cases with big data, you should automatically benefit from it in many use-cases.

* `Insert`, `Remove` & `RemoveAt` methods raise `NotSupportedException`. This is by design in the current version, because it involves performance degradation. I'm planning works around this in the upcoming weeks. If you need that sooner, you're welcomed to file a PR with the proposed changes.

# Architecture Decisions
The architecture of the classes included in this project is driven primarily by performance & memory footprints. The following decisions were made to support it.

## Code duplication
There is high level of code duplication in each of the classes. When you look at them, you will see they are alike & some of the code could be extracted to extension methods. It has been done & has proven to perform worse. For this reason, properties & methods expected to be on the hot-path, duplicate much of the code to get the most out of it.

## Common base - `ILongList<T>`
Having a common base class for all the classes looks very attractive. But it is also proved to perform worse, if you start making properties & methods virtual & there are subsequent calls to `base`. For this reason, each of the classes is the root class & there are hardly any virtual methods & properties. All classes are meant to be `final`, but I've not marked them as `sealed`. If you feel that you'd benefit from it, you're welcomed to inherit from them. The protected stuff is there for your use.

If you desire to have a common base for calls, each of the classes implements `IList<T>` & `ILongList<T>` interfaces. `ILongList<T>` is a new interface, made alike `IList<T>`, but unlocking full support for `long` indexing & use. Considering the characteristics of the classes I feel that the generic name is justified, instead of `IRecyclableList<T>` what you could expect.

## Protection against incorrect use & state
Having protection against invalid state of the object is an important part of software engineering. The classes included in this project provide the same level of protection as the regular `array` & `List` class. They will raise exceptions when invalid index is given. But there is no additional protection that the content & state of the objects internally used by these classes are valid. You can break them, if you want to.

This decision is driven by performance measures. Considering wide use of the lists on the hot-paths, each additional check & operation starts counting. I've decided to eliminate all the checks for the sake of performance. If you feel that you need additional protection, feel free to inherit or reuse any of the provided classes to provide that.

## `this[int index]`, `Count`, `IndexOf(T item)` etc. may overflow, but...
All the classes included in this package support `long` indexing and addressing. The `LongCount` property will always provide the total no. of items stored on the list. Since `Count` is just `LongCount` type-casted to `int`, its value may overflow.

But `List<T>` addressing is kind of currently limited to `int` ranges anyway in practical scenarios. As such `Count` overflowing shouldn't be an issue when using the classes with existing code. If you use `Count` in your loops, it's recommended to replace it with `LongCount`, tough. You don't need to make any changes in `foreach` or other iterator loops. They are expected to transparently use the new classes & yield all items, beyond `int.MaxValue` range.

This is by design, for the best performance. Initially there have been checks in place to prevent overflowing & return `int.MaxValue` instead, but they proved to slow down the operations. Considering low risk for existing code, I've decided to take it out.

If you foresee the overflowing as an issue, you're welcomed to propose an improvement & file a PR with proposed solution.

# Auto-properties, new language features & code style
When you look at the code you may note, that it looks like in early c# days. Where the new language features yield the same or better performance, they are & will be used.

However, many of the new language features have proved to provide worse performance at some point in my testing. It includes auto-properties, for which I could see getters & setters generated in IL, instead of fields. Because all the classes are expected to be on the hot-paths, I've eliminated most of them.

Similarly, you will find places in code with hints or warnings disabled. The decision was always driven by performance measures. The use case & business logic was carefully reviewed to ensure it's safe to take shortcuts, before making the change. If you run into failing scenario, please feel free to create a work item against it and/or file a PR to fix it.

Finally, you'll find places with code commented out. That's to easily track what has been tried & what proved to perform worse.

At any time, if you know & can show in your benchmarks that the code can be simplified without impacting the performance, feel free to file a PR with proposed changes. Please include benchmark results from your testing to speed up things.

# Thread Safety
The `public static` members of the classes are thread safe. Any instance members are not guaranteed to be thread safe. You need to implement locking mechanism to safely use the classes in a multi-threading environment.

# Use
All the classes included in this package are meant to be direct replacements of their corresponding system classes.
* `IList<T>` ⇨ `ILongList<T>`
* `List<T>` ⇨ `RecyclableLongList<T>` 
* `Queue<T>` ⇨ `RecyclableQueue<T>`
* `PriorityQueue<T>` ⇨ `RecyclableSortedList<T>`
* `SortedList<T>` ⇨ `RecyclableSortedList<T>`
* `Stack<T>` ⇨ `RecyclableStack<T>`

Examples of each of the above classes are provided below. Please refer automated tests for more.

## `RecyclableList<T>`
It's the closest equivalent to `List<T>` in regards to functionality. It supports items' removal & delivers the best performance in most scenarios. But it's limited to `int` range, tough. If you find that acceptable, my recommendation is to use this class as the direct replacement for `List<T>`.

Do note, that it implements `ILongList<T>` interface, too. This is to allow having common code base regardless, if you use the basic `RecyclableList<T>` or it's `long`-range version - `RecyclableLongList<T>`. By doing that you can switch between them with highly limited changes in code.

## `RecyclableLongList<T>`

```CSharp
public void RecyclableListExample()
{
    int[] testNumbers = new[] { 1, 2, 2, 3 };

    // Create
    using var list = new RecyclableLongList<int>();

    // Add items
    foreach (var index in testNumbers)
    {
        list.Add(index);
    }

    foreach (var item in list)
    {
        Console.WriteLine(item);
    }

    // This will iterate over this[long index]
    for (var itemIdx = 0; itemIdx <= list.LongCount; itemIdx++)
    {
        Console.WriteLine(list[itemIdx]);
    }

    // This will iterate over this[int index] and may overflow if there are > int.MaxValue no. of items on the list. See: Architecture Decisions
    for (var itemIdx = 0; itemIdx <= list.LongCount; itemIdx++)
    {
        Console.WriteLine(list[itemIdx]);
    }

    list.Clear();
}
```
