# Recyclable.Collections
`Recyclable.Collections` project is an open source framework for operating dynamic lists at performance close to raw arrays, but fairly unlimited in size. It aims at providing minimal memory footprint. It implements `IList<T>`'s interface and is targeted as direct replacements of `List<T>`, `SortableList<T>`, `PriorityQueue<T>` & similar.

# Characteristicts of the classes

* All classes provide large storage capabilities, in practice limited only by the memory available in your system.
* All data is stored in blocks, with the provided `int blockSize` or the default, currently being `10,240` items in each block. It's recommended to use smaller numbers, if you foresee storing low no. of items on the list.
* Memory blocks are created in 2 ways, depending on `blockSize` value
    * when `blockSize < 100` ⇨ blocks are created as regular `arrays`
    * when `blockSize >= 100` ⇨ blocks are taken from and returned to `ArrayPool<T>.Shared`, when blocks are removed
* All classes implement `IDisposable` interface & SHOULD be disposed after use. That's to return block arrays taken from the shared pool. It may be foreseen as an issue for replacement in existing code, which obviously is missing `using` clause. But considering that `Dispose` will be called by `GC` anyway, it should cause issues in specific scenarios, only. In either case the fix is one-word addition of `using`.
* There is no memory copying to increase the capacity of the recyclable classes, anymore. If more capacity is needed, a new memory block is allocate & added to the internal list of blocks. This is the only list that may re-allocate memory block & copy their content to grow. That shouldn't be an issue considering limited no. of blocks stored by each class in practical scenarios. Currently I'm not planning any works around that. Shall there be a need to eliminate it, you're welcomed to file a PR with the proposed changes.
* `Insert`, `Remove` & `RemoveAt` methods raise `NotSupportedException`. This is by design in the current version, because it involves performance degradadion. I'm planning works around this in the upcoming weeks. If you need that sooner, you're welcomed to file a PR with the proposed changes.

# Architecture Decisions
The architecture of the classes included in this project is driven primarily by performance & memory footprints. The following decisions were made to support it.

## Code duplication
There is some level of code duplication in each of the classes. When you look at them, you will see they are alike & some of the code could be extracted to extension methods. It has been done & proved to perform worse. For this reason, properties & methods expected to be on the hotpath duplicate some code.

## Common base `ILongList<T>`
Having a common base class for all the classes looks very attractive. But it is also proved to perform worse, if you start making properties & methods virtual & there are subsequent calls to `base`. For this reason, each of the classes is the root class & there are hardly any virtual methods & properties. All classes are meant to be `final`, but I've not marked them as `sealed`. If you feel that you'd benefit from it, you're welcomed to inherit from them.

If you desire to have a common base for calls, each of the classes implements `IList<T>` & `ILongList<T>` interfaces. `ILongList<T>` is a new interface, made alike `IList<T>`, but unlocking full support for `long` indexing &amp; use. Considering the characteristics of the classes I feel that the generic name is justified, instead of `IRecyclableList<T>` what you could expect.

## Protection against inccorrect use & state
Having protection against invalid state of the object is an important part of software engineering. The classes included in this project provide the same level of protection as the regular `array` & `List` class. They will raise exceptions when invalid index is given. But there is no additional protection that the content & state of the objects internally used by these classes are valid. You can break them, if you want to.

This decision is driven by performance measures. Considering wide use of the lists on the hot-paths, each additional check & operation starts counting. I've decided to eliminate all the checks for the sake of performance. If you feel that you need additional protection, feel free to inherit or reuse any of the provided classes to provide that.

## `this[int index]` & `Count` may overflow, but...
All the classes included in this package support `long` indexing and addressing. The `LongCount` property will always provide the total no. of items stored on the list. Since `Count` is just `LongCount` typecasted to `int`, its value may overflow.

But `List<T>` addressing is kind of currently limited to `int` ranges anyway in practical scenarios. As such `Count` overflowing shouldn't be an issue when using the classes with existing code. If you use `Count` in your loops, it's recommended to replace it with `LongCount`, tough. You don't need to make any changes in `foreach` or other iterator loops. They are expected to transparently use the new classes & yield all items, beyond `int.MaxValue` range.

This is by design, for the best performance. Initially there have been checks in place to prevent overflowing & return `int.MaxValue` instead, but they proved to slow down the operations. Considering low risk for existing code, I've decided to take it out.

If you foresee the overflowing as an issue, you're welcomed to propose an improvement & file a PR with proposed solution.

# Thread Safety
The `public static` members of the classes are thread safe. Any instance members are not guaraneteed to be thread safe. You need to implement locking mechanism to safely use the classes in a multi-threading environment.

# Use
All the classes included in this package are meant to be direct replacements of their corresponding system classes.
* `List<T>` ⇨ `RecyclableList<T>` 
* `SortedList<T>` ⇨ `RecyclableSortedList<T>`
* `Queue<T>` ⇨ `RecyclableQueue<T>`
* `PriorityQueue<T>` ⇨ `RecyclableSortedList<T>`
* `Stack<T>` ⇨ `RecyclableStack<T>`
* `IList<T>` ⇨ `ILongList<T>`

Examples of each of the above classes are provided below. Please refer automated tests for more.
## `RecyclableList<T>`

```CSharp
public void RecyclableListExample()
{
    int[] testNumbers = new[] { 1, 2, 2, 3 };

    // Create
    using var list = new RecyclableList<int>();

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