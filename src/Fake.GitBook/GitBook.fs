namespace GitBook

open System
open System.IO
open System.Text
open FSharp.Literate

[<Sealed>]
type GitBook private () =

  static let outputFileName (input: FileInfo) =
    input.Name.Replace(input.Extension, ".md")

  static let generateOutput outDir outFile (doc: LiterateDocument) =
    let doc = GitBook.Transformations.replaceLiterateParagraphs doc
    if Directory.Exists outDir |> not then
      Directory.CreateDirectory(outDir) |> ignore
      printfn "Creating %s.." outDir
    use w = new StreamWriter(Path.Combine(outDir, outFile))
    Markdown.formatMarkdown w Environment.NewLine doc.DefinedLinks doc.Paragraphs

  static let getFromScriptString fsxFile fsiEvaluator fsx =
    Literate.ParseScriptString(fsx, ?path = Option.map Path.GetFullPath fsxFile, ?fsiEvaluator = fsiEvaluator)

  static let getFromMarkdownString mdFile fsiEvaluator md =
    Literate.ParseMarkdownString(md, ?path = Option.map Path.GetFullPath mdFile, ?fsiEvaluator = fsiEvaluator)

  static let checkIfFileExistsAndRun file f =
    if File.Exists(file) then f ()
    else failwithf "%s does not exist" file

  static member GenerateOutputFromScriptFile(fsxFile, outDir, ?fsiEvaluator) =
    checkIfFileExistsAndRun fsxFile (fun () ->
      fsxFile
      |> File.ReadAllText
      |> getFromScriptString (Some fsxFile) fsiEvaluator
      |> generateOutput outDir (outputFileName (FileInfo fsxFile)))

  static member GenerateOutputFromMarkdownFile(mdFile, outDir, ?fsiEvaluator) =
    checkIfFileExistsAndRun mdFile (fun () ->
      mdFile
      |> File.ReadAllText
      |> getFromMarkdownString  (Some mdFile) fsiEvaluator
      |> generateOutput outDir (outputFileName (FileInfo mdFile)))

  static member GenerateFromFile(fileName, outDir, ?fsiEvaluator) =
    let file = FileInfo fileName
    match file.Extension with
    | ".md" -> GitBook.GenerateOutputFromMarkdownFile(file.FullName, outDir, ?fsiEvaluator = fsiEvaluator)
    | ".fsx" -> GitBook.GenerateOutputFromScriptFile(file.FullName, outDir, ?fsiEvaluator = fsiEvaluator)
    | _ -> failwithf "%s does not support" fileName

  static member Generate(inputDir, outDir, ?fsiEvaluator) =
    for file in Array.append (Directory.GetFiles(inputDir, "*.fsx")) (Directory.GetFiles(inputDir, "*.md")) do
      GitBook.GenerateFromFile(file, outDir, ?fsiEvaluator = fsiEvaluator)
