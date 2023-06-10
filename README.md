# Recyclable.Collections
`Recyclable.Collections` project is an open source framework for operating dynamic lists at performance close to raw arrays, but fairly unlimited in size. It aims at providing minimal memory footprint. It implements `IList<T>`'s interface and is targeted as direct replacements of `List<T>`, `SortableList<T>`, `PriorityQueue<T>` & similar.

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
    1. 🅿️ `GetEnumerator`
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
    1. 🅿️ `GetEnumerator`
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
    1. 👉 Hide not ready classes
        1. 🅿️ `RecyclableQueue<T>`
        1. 🅿️ `RecyclableSortedList<T>`
        1. 🅿️ `RecyclableStack<T>`
        1. 🅿️ `RecyclableUnorderedList<T>`
    1. 🅿️ Add support for `ReadOnlySpan<T>`
    1. 🅿️ Release 0.0.3-alpha
    1. 🅿️ Implement `List<T>` interfaces
        1. 🅿️ `ICollection<T>`
        1. 🅿️ `IEnumerable<T>`
        1. 🅿️ `IEnumerable`
        1. 🅿️ `IList<T>`
        1. 🅿️ `IReadOnlyCollection<T>`
        1. 🅿️ `IReadOnlyList<T>`
        1. 🅿️ `ICollection`
        1. 🅿️ `IList`
    1. 🅿️ Make sure that `NeedsClearing` is used & items are cleared in
        1. 🅿️ `Clear`
        1. 🅿️ `Dispose`
        1. 🅿️ `Remove`
        1. 🅿️ `RemoveAt`
        1. 🅿️ `RemoveBlock`
    1. 🅿️ Release 0.0.3-beta
    1. 🅿️ Add support for `ulong` indexing
        1. 🅿️ Convert `_memoryBlocks` to `Array` to allow `ulong` lengths
        1. 🅿️ Convert block indexes from `int` to `ulong` or `long`
    1. 🅿️ Final optimizations
        1. 🅿️ Replace `Math` class usages with `if` statements
        1. 🅿️ Replace `a - b > 0` & `a - b < 0` comparisons with `a > b` & `a < b`
        1. 🅿️ Replace `a + b > 0` & `a + b < 0` comparisons with `a > b` & `a < b`
        1. 🅿️ Replace `a / b` & `a * b` calculations with equivalents, where possible
        1. 🅿️ Replace virtual calls with static calls
        1. 🅿️ Replace `blockSize` sums by powers of 2, minus 1
        1. 🅿️ Remove type castings, if possible
        1. 🅿️ Convert generic methods to non-generic
    1. 🅿️ Overflow review
        1. 🅿️ Add type casting to `long` for `<<` & `>>` operations, where required
        1. 🅿️ Make type castings `checked`
    1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableList<T>`
    1. 🅿️ Release 0.0.3
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
1. 🅿️ Optimize `OneSizeArrayPool`
    1. 🅿️ Review locks
    1. 🅿️ Measure multi-threading performance
1. 🅿️ Review `RecyclableArrayPool`
    1. 🅿️ Review locks
    1. 🅿️ Measure multi-threading performance
1. 🅿️ Optimize `MemoryBucket<T>`
    1. 🅿️ Convert to `struct`, if possible
    1. 🅿️ Find out if there are better replacements
    1. 🅿️ Release 0.0.8
1. 🅿️ Optimize
    1. 🅿️ `IndexOfSynchronizationContext`
    1. 🅿️ `IndexOfSynchronizationContextPool`
    1. 🅿️ `ManualResetEventSlimmer`
    1. 🅿️ `ManualResetEventSlimmerPool`
    1. 🅿️ `SpinLockSlimmer`
1. 🅿️ Release 0.0.9-beta
1. 🅿️ Cleanup
    1. 🅿️ Replace `LastBlockWithData` property with `_lastBlockWithData` field
    1. 🅿️ Cleanup `RecyclableLongListExtensions`
    1. 🅿️ `ListExtensions`
    1. 🅿️ `MathUtils`
1. 🅿️ Optimize
    1. 🅿️ `ListExtensions`
    1. 🅿️ `MathUtils`
    1. 🅿️ `SystemRandomNumberGenerator`
1. 🅿️ Release 1.0.0

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
* ⚠️ `IndexOf` will automatically switch to parallel search mode. Collections smaller than (currently) 850_000 items are guaranteed to find the 1st occurrence of the `item` parameter. Collections bigger than (currently) 850_000 items will be scanned using parallelization. If either case it'll return the index of the first matching item. This note also applies to `Contains`, because it internally uses `IndexOf`. The value of 850_000 may become configurable in the future, unless it proves to result in much worse performance.

The parallelization utilizes default `Task` scheduling, resulting in non-deterministic order in which search tasks are executed. In most cases it resulted in significant performance improvements. Below you can find the benchmarks for various item counts.

```csharp
BenchmarkDotNet=v0.13.3, OS=Windows 11 (10.0.22621.1702)
AMD Ryzen 7 4700U with Radeon Graphics, 1 CPU, 8 logical and 8 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


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
```
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
## `RecyclableList<T>`

## `RecyclableList<T>`

## `RecyclableList<T>`

