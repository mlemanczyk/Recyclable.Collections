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
	1. ✅ `Dispose`
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
	1. ✅ `Dispose`
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
	1. ✅ Make sure that `NeedsClearing` is used & items are cleared in
		1. ✅ `RecyclableList<T>`
			1. ✅ `Clear`
			1. ✅ `Dispose`
			1. ✅ `Remove`
			1. ✅ `RemoveAt`
			1. ✅ Properties
			1. ✅ Non-versioned & Versioned Enumerators
			1. ✅ `RecyclableListHelpers`
				1. ✅ `EnsureCapacity`
				1. ✅ `Resize`
			1. ✅ `IList<T>` implementation
		1. ✅ `RecyclableLongList<T>`
			1. ✅ `Clear`
			1. ✅ `Dispose`
			1. ✅ `Remove`
			1. ✅ `RemoveAt`
			1. ✅ `RemoveBlock`
			1. ✅ Properties
			1. ✅ Non-versioned & Versioned Enumerators
			1. ✅ `RecyclableLongListHelpers`
				1. ✅ `CopyFollowingItems`
				1. ✅ `CopyTo`
				1. ✅ `EnsureCapacity`
				1. ✅ `Resize`
			1. ✅ `RecyclableLongList<T>.IndexOfHelpers`
	1. ✅ Complete the implementation of `RecyclableLongList<T>`
		1. ✅ `InsertAt` to allow inserting items anywhere
		1. ✅ `RemoveAt` to allow removing item at any index
	1. ✅ Release 0.0.4
	1. 👉 Add `.ToRecyclableList` / `.ToRecyclableLongList` variants for all supported collection types
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
	1. 🅿️ Release 0.0.5
	1. 🅿️ Add support for `ulong` indexing
		1. 🅿️ Convert `_memoryBlocks` to `Array` to allow `ulong` lengths
		1. 🅿️ Convert block indexes from `int` to `ulong` or `long`
	1. 🅿️ Overflow review
		1. 🅿️ Add type casting to `long` for `<<` & `>>` operations, where required
		1. 🅿️ Make type castings `checked`
	1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableList<T>`
		1. `IndexOf` parallelization 
	1. 🅿️ Release 0.0.6
1. 🅿️ Implement `ILongList<T>` interface
	1. 🅿️ `RecyclableList<T>`
	1. 🅿️ `RecyclableLongList<T>`
1. 🅿️ Implement `RecyclableQueue<T>`
	1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableQueue<T>`
	1. 🅿️ Release 0.0.7
1. 🅿️ Implement `RecyclableStack<T>`
	1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableStack<T>`
	1. 🅿️ Release 0.0.8
1. 🅿️ Implement `RecyclableSortedList<T>`
	1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableSortedList<T>`
	1. 🅿️ Release 0.0.9
1. 🅿️ Implement `RecyclableUnorderedList<T>`
	1. 🅿️ Port `RecyclableLongList<T>` optimizations to `RecyclableUnorderedList<T>`
	1. 🅿️ Release 0.0.10
1. 🅿️ Implement `RecyclableVersionedList<T>`
	1. 🅿️ Port `RecyclableList<T>` to `RecyclableVersionedList<T>`
	1. 🅿️ Port `RecyclableLongList<T>` to `RecyclableVersionedLongList<T>`
	1. 🅿️ Release 0.0.11
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
	1. 🅿️ Release 0.0.12
1. 🅿️ Optimize
	1. 🅿️ `IndexOfSynchronizationContext`
	1. 🅿️ `IndexOfSynchronizationContextPool`
	1. 🅿️ `ManualResetEventSlimmer`
		1. 🅿️ Multi-threading benchmarks
	1. 🅿️ `ManualResetEventSlimmerPool`
		1. 🅿️ Multi-threading benchmarks
	1. 🅿️ `SpinLockSlimmer`
		1. 🅿️ Multi-threading benchmarks
1. 🅿️ Release 0.0.13
1. 🅿️ Extend unit tests
	1. 🅿️ `.Add` / `.AddRange` must allow `null` values
	1. 🅿️ `.Remove` / `.RemoveAt` / `.Clear` must clear reference when reference type
	1. 🅿️ `RecyclableLongListExtensions.CopyTo`
1. 🅿️ Cleanup
	1. 🅿️ Replace `LastBlockWithData` property with `_lastBlockWithData` field
	1. 🅿️ Replace properties with field refs, where possible
	1. ✅ `RecyclableLongListExtensions`
	1. ✅ `ListExtensions`
	1. 🅿️ `MathUtils`
	1. 🅿️ Resolve TODOs
	1. 🅿️ Remove obsolete methods
	1. 🅿️ `RecyclableLongListHelpers.Enumerate` to benchmarks
1. 🅿️ Release 0.0.14
1. 🅿️ Optimize
	1. 🅿️ `ListExtensions`
	1. 🅿️ `MathUtils`
	1. 🅿️ `SystemRandomNumberGenerator`
	1. 🅿️ `RecyclableLongListExtensions`
1. 🅿️ Release 0.0.15
1. ✅ Review and remove warnings & hints
	1. ✅ Warnings
	1. ✅ Hints
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
	1. 🅿️ Memory pools
		1. 🅿️ `OneSizeArrayPool<T>`
		1. 🅿️ `RecyclableArrayPool<T>`
1. 🅿️ Add support for `ICollection<T>` interface in `.AddRange` & `constructor`
1. 🅿️ Final optimizations
	1. 🅿️ Replace `Math` class usages with `if` statements
	1. 🅿️ Replace `a - b > 0` & `a - b < 0` comparisons with `a > b` & `a < b`
	1. 🅿️ Replace `a + b > 0` & `a + b < 0` comparisons with `a > b` & `a < b`
	1. 🅿️ Replace `a / b` & `a * b` calculations with equivalents, where possible
	1. 🅿️ Replace virtual calls with static calls
	1. 🅿️ Separate `++number`, `number++`, `--number` & `number--` operations to their own lines (they add extra variable)
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
	* you MUST ensure that you always respect the result of `.MoveNext()` call, and
	* you MUST NOT access `.Current` property before calling `.MoveNext()` or after reaching the end of the collection.
	
	\
	**Failing to do so WILL result in unpredictable behavior.**

	### [Benchmark results](https://github.com/mlemanczyk/Recyclable.Collections/blob/main/Recyclable.Collections.Benchmarks/Results/Benchmarks.md#`IndexOf` Benchmarks)

## `RecyclableList<T>`
* Range: `int`
* Interfaces: `IList<T>`, `IEnumerable<T>`, `IDisposable`\
\
This is the direct equivalent of `List<T>` class, except that the arrays are taken from the shared pool, instead of directly being allocated. It proves to out perform `List<T>` & the standard arrays in certain operations and is fairly close in others. But they may perform worse in certain scenarios. If you're interested in details, please refer to benchmarks.

## `RecyclableLongList<T>`, `RecyclableSortableList<T>`, `RecyclableQueue<T>`, `RecyclableStack<T>`
* Range: `long`
* Interfaces: `IList<T>`, `ILongList<T>`, `IEnumerable<T>`, `IDisposable`
* All classes provide large storage capabilities, in practice limited only by the memory available in your system.
* All data is stored in blocks, with the provided `int blockSize` or the default, currently being `10,240` items in each block. It's recommended to use smaller numbers, if you foresee storing low no. of items on the list.
* There is no memory copying to increase the capacity of the recyclable classes, unless the no. of items exceeds the no. of allocated blocks. In that case, a new array of blocks is created to accommodate . If more capacity is needed, a new memory block is allocate & added to the internal list of blocks. This is the only list that may re-allocate memory block & copy their content to grow. That shouldn't be an issue considering limited no. of blocks stored by each class in practical scenarios. Currently I'm not planning any works around that. Shall there be a need to eliminate it, you're welcomed to file a PR with the proposed changes.
* ⚠️ `IndexOf` will automatically switch to parallel search mode. Collections bigger than (currently) 850_000 items will be scanned using parallelization. In either case it'll return the index of the first matching item. This note also applies to `Contains`, because it internally uses `IndexOf`. The value of 850_000 may become configurable in the future, unless it proves to result in much worse performance.

	The parallelization utilizes default `Task` scheduling, resulting in non-deterministic order in which search tasks are executed. In most cases it resulted in significant performance improvements. Below you can find the benchmarks for various item counts.

	As `RecyclableLongList<T>` is foreseen for use-cases with big data, you should automatically benefit from it in many use-cases.

	### [Benchmark Results](https://github.com/mlemanczyk/Recyclable.Collections/blob/main/Recyclable.Collections.Benchmarks/Results/Benchmarks.md)

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

### [Benchmark results](https://github.com/mlemanczyk/Recyclable.Collections/blob/main/Recyclable.Collections.Benchmarks/Results/Benchmarks.md#`IndexOf` Benchmarks)
