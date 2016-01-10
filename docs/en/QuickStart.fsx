(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use
// it to define helpers that you do not want to show in the documentation.
#r @"../../packages/FAKE/tools/FakeLib.dll"
#I @"../../bin/Fake.GitBook"
open Fake

(**

# Quick Start

To use FAKE.GitBook, we can generate a document [GitBook][gitbook].

## Installing dependencies

Show an example to install FAKE and FAKE.GitBook using [Paket][paket]。

Create a `paket.dependencies` file in your project's root and specify some dependencies in it.
If you don't install `npm`, you need to add `Npm.js` depencencies。

```
source https://api.nuget.org/v3/index.json
source https://nuget.org/api/v2

nuget FAKE
nuget FAKE.GitBook
nuget Npm.js // if you don't install npm
```

Install dependencies referring to [Paket - Getting Started][paket-getting-started].

## pakage.json

Create a `package.json` file in your project's root.

```
{
  "name": "sample-book",
  "private": true,
  "version": "1.0.0",
  "description": "gitbook sample",
  "scripts": {
    "prepublish": "gitbook install"
  },
  "devDependencies": {
    "gitbook-cli": "^1.0.0",
    "gitbook-plugin-include-codeblock": "^1.5.0"
  }
}
```

## The build script

Create a `build.fsx` file in your project's root and write codes to load libraries.

```fsharp
#r @"packages/FAKE/tools/FakeLib.dll"
#I "packages/FSharp.Formatting/lib/net40"
#I "packages/FSharp.Compiler.Service/lib/net40"
#I "packages/FSharpVSPowerTools.Core/lib/net45"
#r @"packages/FAKE.GitBook/lib/net451/Fake.GitBook.dll"
```
And define a target.

*)

Target "Generate" (fun _ ->
  GitBook id id [Html]
)

RunTargetOrDefault "Generate"

(**

## book.json

Create a `book.json` file in `gitbook` directory.

```json
{
  "structure": {
    "readme": "INTRODUCTION.md",
    "summary": "SUMMARY.md"
  },
  "plugins": [
    "include-codeblock"
  ]
}
```

## Writing documents

Write some documents using FSharp.Formatting style.

## Running

Run the command below:

    [lang=powershell]
    packages\FAKE\tools\Fake.exe build.fsx

## Sample porject

See also [FAKE.GitBook.Sample](https://github.com/pocketberserker/FAKE.GitBook.Sample).

[fhsarp-formatting]: http://tpetricek.github.io/FSharp.Formatting/
[gitbook]: https://github.com/GitbookIO/gitbook
[paket]: https://fsprojects.github.io/Paket/index.html
[paket-getting-started]: https://fsprojects.github.io/Paket/getting-started.html
[fake-getting-started]: http://fsharp.github.io/FAKE/gettingstarted.html

*)

