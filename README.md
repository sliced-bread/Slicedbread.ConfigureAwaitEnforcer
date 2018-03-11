# Slicedbread.ConfigureAwaitEnforcer

A command line tool for ensuring `ConfigureAwait()` is specified for all `await` calls in a solution. Ideal for running in a CI pipeline.

- Run on existing codebases - `await`s without `ConfigureAwait()` will get a free pass on the first run on the tool. Only new/modified code will flag an error, allowing you to add this now and fix existing code later.
- Supports `dynamic` - Enforces that when `await` is called on a a `dynamic` object, `ConfigureAwait()` must be present
- Intergrats with CI pipelines - Uses a non-zero exit code when new invalid `await` is found, allowing you to cause the build to fail

```
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
  
  MyProject - AnotherFile
    12: await DoSomethingAsync();


```

# Parameters

#### Strict Mode
```
> Slicedbread.ConfigureAwaitEnforcer.exe  MySolution.sln  -strict
```

_All_ `await` calls without `ConfigureAwait()` will cause a failure, regardless of whether they were present in the codebase on the first run.

Useful if you wish to find all existing invalid `await`s in your codebase locally.


#### Ignoring Files
```
> Slicedbread.ConfigureAwaitEnforcer.exe  MySolution.sln  /excludeFiles "tests" 
```

Ignores any file whos name contains the string passed to `excludeFiles`. 

You can specify this multiple times, for example:
```
> Slicedbread.ConfigureAwaitEnforcer.exe  MySolution.sln  /excludeFiles "tests"  /excludeFiles "fixture" 
```
