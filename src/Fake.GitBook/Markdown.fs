module GitBook.Markdown

open System
open System.IO
open System.Text
open System.Collections.Generic
open FSharp.Markdown
open FSharp.Literate

type FormattingContext =
  { LineBreak : unit -> unit
    Newline : string
    Writer : TextWriter
    Links : IDictionary<string, string * option<string>> }

let rec formatSpan (ctx:FormattingContext) = function
| LatexDisplayMath body ->
  fprintf ctx.Writer "$$%s%s%s$$" ctx.Newline body ctx.Newline
| LatexInlineMath body ->
  fprintf ctx.Writer "$$%s$$" body
| AnchorLink id -> fprintf ctx.Writer "<a name=\"%s\">&#160;</a>" id
| EmbedSpans cmd -> formatSpans ctx (cmd.Render())
| Literal str -> ctx.Writer.Write(str)
| HardLineBreak -> ctx.Writer.Write(Environment.NewLine)
| IndirectLink(body, _, key) when key.StartsWith("^") ->
  // footnote
  ctx.Writer.Write("[")
  formatSpans ctx body 
  ctx.Writer.Write("]")
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
  body |> fprintf ctx.Writer "`%s`"
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
    fprintf ctx.Writer "$$%s%s%s$$" ctx.Newline body ctx.Newline
  | EmbedParagraphs cmd -> formatParagraphs ctx (cmd.Render())
  | Heading(n, spans) -> 
    String.replicate n "#" |> fprintf ctx.Writer "%s "
    formatSpans ctx spans
  | Paragraph spans ->
    formatSpans ctx spans
  | HorizontalRule c ->
    c |> string |> String.replicate 3 |> fprintf ctx.Writer "%s"
  | CodeBlock(code, codeLanguage, _) when String.IsNullOrWhiteSpace(codeLanguage) ->
    fprintf ctx.Writer "```%s%s%s```" ctx.Newline code ctx.Newline
  | CodeBlock(code, codeLanguage, _) ->
    fprintf ctx.Writer "```%s%s%s%s```" codeLanguage ctx.Newline code ctx.Newline
  | TableBlock(headers, alignments, rows) ->
    headers
    |> Option.iter (fun headers ->
      List.zip headers alignments
      |> List.map (fun (c, a) ->
        let cell = new StringBuilder()
        use writer = new StringWriter(cell)
        for paragraph in c do
          formatParagraph { ctx with LineBreak = ignore; Writer = writer } paragraph
        let cell = cell.ToString()
        let l = String.length cell
        let align =
          match a with
          | AlignLeft -> ":" + String.replicate (l + 1) "-"
          | AlignRight -> (String.replicate (l + 1) "-") + ":"
          | AlignCenter -> ":" + (String.replicate l "-") + ":"
          | AlignDefault -> String.replicate (l + 2) "-"
        (" " + cell  + " ", align)
      )
      |> List.fold (fun (ac, aa) (c, a) -> (ac + "|" + c, aa + "|" + a)) ("", "")
      |> (fun (h, a) -> fprintfn ctx.Writer "%s|%s%s|" h ctx.Newline a)
    )
    for i, row in rows |> List.mapi (fun i x -> (i, x)) do
      ctx.Writer.Write("|")
      for cell in row do
        ctx.Writer.Write(" ")
        for paragraph in cell do
          formatParagraph { ctx with LineBreak = ignore } paragraph
        ctx.Writer.Write(" |")
      if i <> List.length rows - 1 then ctx.LineBreak()
  | ListBlock(kind, items) ->
    let tag = if kind = Ordered then "1." else "-"
    for i, body in items |> List.mapi (fun i x -> (i, x)) do
      body
      |> List.iter (fun x ->
        fprintf ctx.Writer "%s " tag
        formatParagraph { ctx with LineBreak = ignore } x
      ) 
      if i <> List.length items - 1 then ctx.LineBreak()
  | QuotedBlock body ->
    for i, p in body |> List.mapi (fun i x -> (i, x)) do
      let builder = StringBuilder()
      use writer = new StringWriter(builder)
      formatParagraph { ctx with Writer = writer; LineBreak = fun () -> writer.Write(ctx.Newline) } p
      let sep = [| ctx.Newline |]
      builder.ToString().TrimEnd(ctx.Newline.ToCharArray()).Split(sep, StringSplitOptions.None)
      |> Array.map (fun x -> if String.IsNullOrEmpty x then x else "> " + x)
      |> String.concat ctx.Newline
      |> fprintf ctx.Writer "%s"
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
  let footnotes =
    ctx.Links
    |> Seq.filter (fun (KeyValue(k, _)) -> k.StartsWith("^"))
  if not <| Seq.isEmpty footnotes then ctx.Writer.Write(ctx.Newline)
  footnotes
  |> Seq.iter (fun (KeyValue(k, (v, _))) ->
    fprintfn ctx.Writer "[%s]: %s" k v
  )

let formatMarkdown writer newline links =
  {
    Writer = writer
    Links = links
    Newline = newline
    LineBreak = ignore
  }
  |> formatParagraphs
