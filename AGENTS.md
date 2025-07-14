# AGENTS

This repository hosts various libraries distributed as NuGet packages. Keep these links up to date so contributors and tools can easily locate them.

Only the .NET SDK **8.0** is available in the execution environment. Use this version when building or running tests.

## Implementation Guidelines
1. All `*.csproj` files should enable nullable reference types with `<Nullable>enable</Nullable>`.
1. Implementations should be optimized for maximum performance even if that introduces some code duplication.
1. Consider applying attributes such as `MethodImplOptions.AggressiveInlining` or faster alternatives to common operations when appropriate. Another would be modulo "%" or division "/" operations, which could be often replaced with faster alternatives, possibly under specific conditions.

## Running Tests
1. When running `dotnet test`, do **not** pass `--no-build` because that leads to missing assemblies, test failures without logs and other issues.
1. When running `dotnet test` specify the `--framework` argument matching the SDK available in the environment (currently `net8.0`). If you omit this parameter, the tests will fail running with no logs etc.
1. The tests run for a very long time, because there are 61_000+ unit tests for all the collections. Set timeout when running them to 15 mis.
1. All tests must use **xUnit** together with **FluentAssertions**.

## Source Code Formatting Guidelines
1. Indent code using four spaces or a single tab per level.
1. Place one blank line after each closing curly brace of a code block, except when several closing braces appear sequentially. 
1. Always use braces for `if` and `else` bodies. 

## NuGet packages
- [Recyclable.Collections](https://github.com/mlemanczyk/Recyclable.Collections)
- [Recyclable.Collections.Compatibility.List](https://github.com/mlemanczyk/Recyclable.Collections.Compatibility.List)
- [Recyclable.Collections.Concurrent](https://github.com/mlemanczyk/Recyclable.Collections.Concurrent)
- [Recyclable.Collections.Linq](https://github.com/mlemanczyk/Recyclable.Collections.Linq)
- [Recyclable.Collections.Searching](https://github.com/mlemanczyk/Recyclable.Collections.Searching)
- [Recyclable.Collections.TestData](https://github.com/mlemanczyk/Recyclable.Collections.TestData)
