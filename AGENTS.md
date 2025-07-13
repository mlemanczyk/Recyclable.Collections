# AGENTS

This repository hosts various libraries distributed as NuGet packages. Keep these links up to date so contributors and tools can easily locate them.

Only the .NET SDK **8.0** is available in the execution environment. Use this version when building or running tests.

All `*.csproj` files should enable nullable reference types with `<Nullable>enable</Nullable>`.
Indent code using four spaces or a single tab per level. Place one blank line after each closing curly brace of a code block, except when several closing braces appear sequentially. Always use braces for `if` and `else` bodies.
All tests must use **xUnit** together with **FluentAssertions**.
Implementations should be optimized for maximum performance even if that introduces some code duplication. Consider applying attributes such as `MethodImplOptions.AggressiveInlining` or faster alternatives to common operations when appropriate.

## NuGet packages
- [Recyclable.Collections](https://github.com/mlemanczyk/Recyclable.Collections)
- [Recyclable.Collections.Compatibility.List](https://github.com/mlemanczyk/Recyclable.Collections.Compatibility.List)
- [Recyclable.Collections.Concurrent](https://github.com/mlemanczyk/Recyclable.Collections.Concurrent)
- [Recyclable.Collections.Linq](https://github.com/mlemanczyk/Recyclable.Collections.Linq)
- [Recyclable.Collections.Searching](https://github.com/mlemanczyk/Recyclable.Collections.Searching)
- [Recyclable.Collections.TestData](https://github.com/mlemanczyk/Recyclable.Collections.TestData)
