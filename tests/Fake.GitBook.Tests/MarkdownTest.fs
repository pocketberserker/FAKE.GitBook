﻿namespace Fake.GitBook.Tests

open System
open System.IO
open System.Text
open Persimmon
open UseTestNameByReflection
open FSharp.Literate

module Helper =
  
  open System.Text.RegularExpressions

  let assertEquals expected actual =
     let replace str = Regex.Replace(str, @"\r\n?|\n", System.Environment.NewLine)
     assertEquals (replace expected) (replace actual)

module MarkdownTest =

  open Helper

  let horizontalRule = """# Foo

---
"""

  let `` parse and format anchor horizontal rule`` = test {
    let doc = Literate.ParseMarkdownString(horizontalRule)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals horizontalRule <| builder.ToString()
  }

  let anchorLink = """<a id="bar"></a>

## Bar

[Bar](#bar)
"""

  let `` parse and format anchor link`` = test {
    let doc = Literate.ParseMarkdownString(anchorLink)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals anchorLink <| builder.ToString()
  }

  let list = """- A
- B
"""

  let `` parse and format list`` = test {
    let doc = Literate.ParseMarkdownString(list)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals list <| builder.ToString()
  }

  let table = """| a | b | c | d |
|---|:--|:-:|--:|
| A | B | C | D |
"""

  let `` parse and format table`` = test {
    let doc = Literate.ParseMarkdownString(table)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals table <| builder.ToString()
  }

  let fsharpCode = """```fsharp
let a = "a"

let b = "b"
```
"""

  let `` parse and format F# code`` = test {
    let doc = Literate.ParseMarkdownString(fsharpCode) |> GitBook.Transformations.replaceLiterateParagraphs false
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals fsharpCode <| builder.ToString()
  }

  let `` parse and format F# code with tips`` = test {
    let code = """```fsharp
let a = "a"

let b () = "b"

let rec c n =
  if n = 0 then 0
  else c (n - 1)
```
"""
    let doc = Literate.ParseMarkdownString(code) |> GitBook.Transformations.replaceLiterateParagraphs true
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    let code = """```fsharp
// val a : string
let a = "a"

// val b : unit -> string
let b () = "b"

// val c : n:int -> int
let rec c n =
  if n = 0 then 0
  else c (n - 1)
```
"""
    do! assertEquals code <| builder.ToString()
  }

  let `` parse and format other language code`` = test {
    let code = """```scala
val a = "a"

val b = "b"
```
"""
    let doc = Literate.ParseMarkdownString(code) |> GitBook.Transformations.replaceLiterateParagraphs true
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals code <| builder.ToString()
  }

  let quoteBlock = """> a
> b
"""

  let `` parse and format quote block`` = test {
    let doc = Literate.ParseMarkdownString(quoteBlock)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals quoteBlock <| builder.ToString()
  }

  let paragraphs = """one.
two.

three.
four.
"""

  let `` parse and format paragraphs`` = test {
    let doc = Literate.ParseMarkdownString(paragraphs)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals paragraphs <| builder.ToString()
  }

  let inlineLatex = "$k_{n+1} = n^2 + k_n^2 - k_{n-1}$"

  let `` parse and format inline latex`` = test {
    let doc = Literate.ParseMarkdownString(inlineLatex)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals ("$" + inlineLatex + "$" + Environment.NewLine) <| builder.ToString()
  }

  let displayLatex = """$$$
A_{m,n} =
 \begin{pmatrix}
  a_{1,1} & a_{1,2} & \cdots & a_{1,n} \\
  a_{2,1} & a_{2,2} & \cdots & a_{2,n} \\
  \vdots  & \vdots  & \ddots & \vdots  \\
  a_{m,1} & a_{m,2} & \cdots & a_{m,n}
 \end{pmatrix}
"""

  let `` parse and format display latex`` = test {
    let doc = Literate.ParseMarkdownString(displayLatex)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    let expected = displayLatex.Replace("$$$", "$$") + "$$" + Environment.NewLine
    do! assertEquals expected <| builder.ToString()
  }

  let quoteList = """> 1. a
> 1. b
"""

  let `` parse and format quote and list`` = test {
    let doc = Literate.ParseMarkdownString(quoteList)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals quoteList <| builder.ToString()
  }

  let footnote = """[^1]

[^1]: footnote
"""

  let `` parse and format footnote`` = test {
    let doc = Literate.ParseMarkdownString(footnote)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine doc.DefinedLinks doc.Paragraphs
    do! assertEquals footnote <| builder.ToString()
  }

  let `` parse and format script`` = test {
    let markdown = (paragraphs, list) ||> sprintf """%s
```fsharp
let a = "a"

let b = "b"```

%s"""
    let script = (paragraphs, list) ||> sprintf """(**

%s
*)

let a = "a"

let b = "b"

(**

%s
*)
"""
    let doc = Literate.ParseScriptString(script) |> GitBook.Transformations.replaceLiterateParagraphs false
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals markdown <| builder.ToString()
  }
