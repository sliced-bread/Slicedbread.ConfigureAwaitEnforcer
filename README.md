# Slicedbread.ConfigureAwaitEnforcer

A command line tool for ensuring `ConfigureAwait()` is specified for all `await` calls in a solution. Ideal for running in a CI pipeline.

- Run on existing codebases - `await`s without `ConfigureAwait()` will get a free pass on the first run of the tool. Only new/modified code will flag an error, allowing you to add this now and fix existing code later
- Supports `dynamic` - Enforces that when `await` is called on something `dynamic`, `ConfigureAwait()` must still be present
- Quickly fix invalid `await`s - Prints the project name, file name, line number and code for each invalid `await`
- Integrates with CI pipelines - Uses a non-zero exit code when new invalid `await` is found, allowing you to fail the build

```bash
> Slicedbread.ConfigureAwaitEnforcer.exe   MySolution.sln
```

```
Parsing command line args
Loading solution MySolution.sln

Analysing:
  MyProject - 91 documents
  AnotherProject - 53 documents

Found 3 new await call(s) without ConfigureAwait
  
  MyProject - SomeFile.cs
    54: await DoSomethingAsync();
    70: var a = DoSomethingElseAsync();
  
  MyProject - AnotherFile.cs
    12: await DoSomethingAsync();
```

# Parameters

#### Strict Mode
```bash
> Slicedbread.ConfigureAwaitEnforcer.exe  MySolution.sln  -strict
```

_All_ `await` calls without `ConfigureAwait()` will cause a failure, regardless of whether they were present in the codebase on the first run.

Useful if you wish to find all existing invalid `await`s in your codebase locally.


#### Ignoring Files
```bash
> Slicedbread.ConfigureAwaitEnforcer.exe  MySolution.sln  /excludeFiles "tests"  /excludeFiles "fixture" 
```

Ignores any file with a name containing the string passed to `/excludeFiles`. 

You can specify this multiple times.


# How does it work?
This tool uses The .NET Compiler Platform (a.k.a. Roslyn).

Most of the repository is boilerplate for writing to the console etc. The interesting code for parsing a document to find `await`s without `ConfigureAwait()` is in [DocumentAnalyser.cs](https://github.com/sliced-bread/Slicedbread.ConfigureAwaitEnforcer/blob/master/src/ConfigureAwaitEnforcer/Analyser/DocumentAnalyser.cs#L12)

The tests for this code (useful for checking what syntax the tool does/does not support) are [here](https://github.com/sliced-bread/Slicedbread.ConfigureAwaitEnforcer/blob/master/src/Tests/Tests.cs)
