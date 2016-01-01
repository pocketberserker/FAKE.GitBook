namespace Fake.GitBook.Tests

open System
open System.IO
open System.Text
open Persimmon
open UseTestNameByReflection
open FSharp.Literate

module MarkdownTest =

  let table = """| a | b | c | d |
|---|:--|:-:|--:|
| A | B | C | D |
"""

  let `` parsed and formatted table is default`` = test {
    let doc = Literate.ParseMarkdownString(table)
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    let actual = GitBook.Markdown.formatMarkdown writer Environment.NewLine (dict []) doc.Paragraphs
    do! assertEquals table <| builder.ToString()
  }
