(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use
// it to define helpers that you do not want to show in the documentation.
#r @"../../packages/FAKE/tools/FakeLib.dll"
#I @"../../bin/Fake.GitBook"
open Fake

(**

# クイックスタート

FAKE.GitBookを使うことで、[FSharp.Formatting][fhsarp-formatting]スタイルで書いたドキュメントを[GitBook][gitbook]で出力できます。

## 依存関係のインストール

[Paket][paket]を使ってFAKEとFAKE.GitBookをインストールする例を示します。

プロジェクトのルートに`paket.dependencies`ファイルを作成し、依存関係を定義してください。
`npm`をインストールしていない場合は、`Npm.js`を依存関係に追加する必要があります。

```
source https://api.nuget.org/v3/index.json
source https://nuget.org/api/v2

nuget FAKE
nuget FAKE.GitBook
nuget Npm.js // if you don't install npm
```

[Paket - Getting Started][paket-getting-started]を参考に、依存関係をインストールしてください。

## pakage.json

プロジェクトのルートに`package.json`を作成してください。

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
    "gitbook-plugin-include-codeblock": "^1.5.0",
    "gitbook-plugin-japanese-support": "0.0.1"
  }
}
```

## ビルドスクリプト

まず、ライブラリをロードします。

```fsharp
#r @"packages/FAKE/tools/FakeLib.dll"
#I "packages/FSharp.Formatting/lib/net40"
#I "packages/FSharp.Compiler.Service/lib/net40"
#I "packages/FSharpVSPowerTools.Core/lib/net45"
#r @"packages/FAKE.GitBook/lib/net451/Fake.GitBook.dll"
```
ターゲットを定義します。

*)

Target "Generate" (fun _ ->
  GitBook id id [Html]
)

RunTargetOrDefault "Generate"

(**

## book.json

`gitbook`ディレクトリに`book.json`ファイルを作成してください。

```json
{
  "structure": {
    "readme": "INTRODUCTION.md",
    "summary": "SUMMARY.md"
  },
  "plugins": [
    "include-codeblock",
    "japanese-support"
  ]
}
```

## ドキュメントの作成

FSharp.Formattingスタイルでドキュメントを書いてください。

## 実行

次のコマンドを実行してください。

    [lang=powershell]
    packages\FAKE\tools\Fake.exe build.fsx

## サンプルプロジェクト

[FAKE.GitBook.Sample](https://github.com/pocketberserker/FAKE.GitBook.Sample)を参考にしてください。

[fhsarp-formatting]: http://tpetricek.github.io/FSharp.Formatting/
[gitbook]: https://github.com/GitbookIO/gitbook
[paket]: https://fsprojects.github.io/Paket/index.html
[paket-getting-started]: https://fsprojects.github.io/Paket/getting-started.html
[fake-getting-started]: http://fsharp.github.io/FAKE/gettingstarted.html

*)

