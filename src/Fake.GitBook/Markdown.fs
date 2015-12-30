module GitBook.Markdown

open System
open System.IO
open System.Collections.Generic
open FSharp.Markdown
open FSharp.Literate

type FormattingContext =
  { LineBreak : unit -> unit
    Newline : string
    Writer : TextWriter
    Links : IDictionary<string, string * option<string>> }

let specialChars = [|
  "*"
  "\\"
  "`"
  "_"
  "{"
  "}"
  "["
  "]" 
  "("
  ")"
  ">"
  "#"
  "."
  "!"
  "+"
  "-"
  "$"
|]

let encode x =
  specialChars |> Array.fold (fun (acc:string) k -> acc.Replace(k, "\\" + k)) x

let rec formatSpan (ctx:FormattingContext) = function
| LatexDisplayMath body ->
  fprintfn ctx.Writer "$$%s%s%s$$" ctx.Newline body ctx.Newline
| LatexInlineMath body ->
  fprintfn ctx.Writer "$$%s$$" body
| AnchorLink id -> fprintf ctx.Writer "<a name=\"%s\">&#160;</a>" id
| EmbedSpans cmd -> formatSpans ctx (cmd.Render())
| Literal str -> ctx.Writer.Write(str)
| HardLineBreak -> ctx.Writer.Write(Environment.NewLine)
| IndirectLink(body, _, Html.LookupKey ctx.Links (link, title)) 
| DirectLink(body, (link, title)) ->
  ctx.Writer.Write("[")
  formatSpans ctx body 
  ctx.Writer.Write("]")
  match title with 
  | Some title ->
    fprintf ctx.Writer "(%s %s)" link title
  | _ ->
    fprintf ctx.Writer "(%s)" link
| IndirectLink(body, original, _) ->
  ctx.Writer.Write("[")
  formatSpans ctx body
  ctx.Writer.Write("]")
  ctx.Writer.Write(original)
| IndirectImage(body, _, Html.LookupKey ctx.Links (link, title)) 
| DirectImage(body, (link, title)) -> 
  match title with 
  | Some title ->
    fprintf ctx.Writer "![%s](%s %s)" body link title
  | _ ->
    fprintf ctx.Writer "![%s](%s)" body link
| IndirectImage(body, original, _) ->
  fprintf ctx.Writer "![%s]%s" body original
| Strong(body) -> 
  ctx.Writer.Write("**")
  formatSpans ctx body
  ctx.Writer.Write("**")
| InlineCode(body) -> 
  encode body |> fprintf ctx.Writer "`%s`"
| Emphasis(body) -> 
  ctx.Writer.Write("*")
  formatSpans ctx body
  ctx.Writer.Write("*")

and formatSpans ctx = List.iter (formatSpan ctx)

let bigBreak (ctx:FormattingContext) () =
  ctx.Writer.Write(ctx.Newline + ctx.Newline)
let smallBreak (ctx:FormattingContext) () =
  ctx.Writer.Write(ctx.Newline)

let rec formatParagraph (ctx:FormattingContext) paragraph =
  match paragraph with
  | LatexBlock lines ->
    let body = String.concat ctx.Newline lines
    fprintfn ctx.Writer "$$%s%s%s$$" ctx.Newline body ctx.Newline
  | EmbedParagraphs cmd -> formatParagraphs ctx (cmd.Render())
  | Heading(n, spans) -> 
    String.replicate n "#" |> fprintf ctx.Writer "%s "
    // TODO: anchor
    formatSpans ctx spans
  | Paragraph spans ->
    ctx.LineBreak(); ctx.LineBreak()
    for span in spans do 
      formatSpan ctx span
  | HorizontalRule c ->
    c |> string |> String.replicate 3 |> fprintf ctx.Writer "%s"
    ctx.LineBreak(); ctx.LineBreak()
  | CodeBlock(code, codeLanguage, _) when String.IsNullOrWhiteSpace(codeLanguage) ->
    fprintfn ctx.Writer "```%s%s%s```" ctx.Newline code ctx.Newline
  | CodeBlock(code, codeLanguage, _) ->
    fprintfn ctx.Writer "```%s%s%s%s```" codeLanguage ctx.Newline code ctx.Newline
  | TableBlock(headers, alignments, rows) ->
    // TODO: implement
    ctx.Writer.Write(ctx.Newline)
  | ListBlock(kind, items) ->
    let tag = if kind = Ordered then "1." else "-"
    for body in items do
      body
      |> List.iter (fun x ->
        fprintf ctx.Writer "%s " tag
        formatParagraph { ctx with LineBreak = ignore } x
        ctx.LineBreak()
      ) 
    ctx.LineBreak()
  | QuotedBlock body ->
    for p in body do
      ctx.Writer.Write("> ")
      formatParagraph { ctx with LineBreak = ignore } p
      ctx.LineBreak()
    ctx.LineBreak()
  | Span spans -> 
    formatSpans ctx spans
  | InlineBlock code ->
    ctx.Writer.Write(code)
  ctx.LineBreak()

and formatParagraphs ctx paragraphs = 
  let length = List.length paragraphs
  let smallCtx = { ctx with LineBreak = smallBreak ctx }
  let bigCtx = { ctx with LineBreak = bigBreak ctx }
  for last, paragraph in paragraphs |> Seq.mapi (fun i v -> (i = length - 1), v) do
    formatParagraph (if last then smallCtx else bigCtx) paragraph

let formatMarkdown writer newline links =
  {
    Writer = writer
    Links = links
    Newline = newline
    LineBreak = ignore
  }
  |> formatParagraphs
