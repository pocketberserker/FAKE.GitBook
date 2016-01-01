namespace Fake.GitBook.Tests

open System
open System.IO
open System.Text
open Persimmon
open UseTestNameByReflection
open FSharp.Literate

module MarkdownTest =

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

  let code = """```fsharp
let a = "a"
```
"""

  let `` parse and format code`` = test {
    let doc = Literate.ParseMarkdownString(code) |> GitBook.Transformations.replaceLiterateParagraphs
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
