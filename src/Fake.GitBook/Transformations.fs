module GitBook.Transformations

open System
open System.IO
open System.Text
open System.Collections.Generic
open FSharp.Literate
open FSharp.CodeFormat
open FSharp.Markdown

let rec replaceSpecialCodes (formatted:IDictionary<_, _>) = function
| Matching.LiterateParagraph(special) ->
  match special with
  | RawBlock lines -> Some(InlineBlock (Transformations.unparse lines))
  | LiterateCode(_, { Visibility = (HiddenCode | NamedCode _) }) -> None
  | FormattedCode lines
  | LiterateCode(lines, _) -> Some(formatted.[Choice1Of2 lines])
  | CodeReference ref -> Some(formatted.[Choice2Of2 ref])
  | OutputReference _
  | ItValueReference _
  | ValueReference _ ->
    failwith "Output, it-value and value references should be replaced by FSI evaluator"
  | LanguageTaggedCode(lang, code) ->
    let builder = StringBuilder()
    use writer = new StringWriter(builder)
    fprintfn writer "```%s" lang
    fprintfn writer "%s" code
    writer.Write("```")
    Some(InlineBlock(builder.ToString()))
| Matching.ParagraphNested(pn, nested) ->
  let nested = List.map (List.choose (replaceSpecialCodes formatted)) nested
  Some(Matching.ParagraphNested(pn, nested))
| par -> Some par

let rec formatSpans (writer: TextWriter) lineBreak spans =
  for span in spans do
    match span with
    | Token(_, value, _) ->
      writer.Write(value)
    | Error(_, _, body) ->
      formatSpans writer lineBreak body
    | Output body -> writer.Write(body)
    | Omitted(_, _) -> ()
  lineBreak ()

let formatSpansWithTips (writer: TextWriter) lineBreak newline = function
| [] -> lineBreak ()
| Token(TokenKind.Keyword, "let", _) :: Token(TokenKind.Default, " ", _) :: Token(TokenKind.Identifier, value, Some(ToolTipSpan.Literal tip :: _)) :: xs
| Token(TokenKind.Keyword, "let", _) :: Token(TokenKind.Default, " ", _) :: Token(TokenKind.Function, value, Some(ToolTipSpan.Literal tip :: _)) :: xs ->
  fprintf writer "// %s%slet %s" tip newline value
  formatSpans writer lineBreak xs
| Token(TokenKind.Keyword, "let", _) :: Token(TokenKind.Default, " ", _) :: Token(TokenKind.Keyword, "rec", _) :: Token(TokenKind.Default, " ", _) :: Token(TokenKind.Function, value, Some(ToolTipSpan.Literal tip :: _)) :: xs ->
  fprintf writer "// %s%slet rec %s" tip newline value
  formatSpans writer lineBreak xs
| Token(_, value, _) :: xs ->
  writer.Write(value)
  formatSpans writer lineBreak xs
| Error(_, _, body) :: xs ->
  formatSpans writer lineBreak body
  formatSpans writer lineBreak xs
| Output body :: xs ->
  writer.Write(body)
  formatSpans writer lineBreak xs
| Omitted(_, _) :: xs ->
  formatSpans writer lineBreak xs

let formatLines (newline: string) generateTips lines =
  let builder = StringBuilder()
  use writer = new StringWriter(builder)
  fprintfn writer "```fsharp"
  for (i, Line spans) in lines |> Seq.mapi (fun i x -> (i, x)) do
    let lineBreak =
      if i = Seq.length lines - 1 then ignore
      else fun () -> writer.Write(newline)
    if generateTips then formatSpansWithTips writer lineBreak newline spans
    else formatSpans writer lineBreak spans
  writer.Write("```")
  builder.ToString()

let replaceLiterateParagraphs generateTips (doc:LiterateDocument) = 
  let replacements = Seq.collect Transformations.collectCodes doc.Paragraphs
  let codes = replacements |> Seq.map (snd >> formatLines Environment.NewLine generateTips)
  let lookup =
    [
      for (key, _), fmtd in Seq.zip replacements codes ->
        key, InlineBlock(fmtd)
    ]
    |> dict 
  let newParagraphs = List.choose (replaceSpecialCodes lookup) doc.Paragraphs
  doc.With(paragraphs = newParagraphs)
