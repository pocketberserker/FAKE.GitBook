namespace GitBook

open System
open System.IO
open System.Text
open FSharp.Literate

[<Sealed>]
type GitBook private () =

  static let outputFileName (input: FileInfo) =
    input.Name.Replace(input.Extension, ".md")

  static let generateOutput outDir outFile generateTips doc =
    let doc = GitBook.Transformations.replaceLiterateParagraphs generateTips doc
    if Directory.Exists(outDir) |> not then
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

  static member GenerateOutputFromScriptFile(fsxFile, outDir, ?generateTips, ?fsiEvaluator) =
    let generateTips = defaultArg generateTips true
    checkIfFileExistsAndRun fsxFile (fun () ->
      fsxFile
      |> File.ReadAllText
      |> getFromScriptString (Some fsxFile) fsiEvaluator
      |> generateOutput outDir (outputFileName (FileInfo(fsxFile))) generateTips)

  static member GenerateOutputFromMarkdownFile(mdFile, outDir, ?generateTips, ?fsiEvaluator) =
    let generateTips = defaultArg generateTips true
    checkIfFileExistsAndRun mdFile (fun () ->
      mdFile
      |> File.ReadAllText
      |> getFromMarkdownString (Some mdFile) fsiEvaluator
      |> generateOutput outDir (outputFileName (FileInfo(mdFile))) generateTips)

  static member GenerateFromFile(file: FileInfo, outDir, ?generateTips, ?fsiEvaluator) =
    match file.Extension with
    | ".md" -> GitBook.GenerateOutputFromMarkdownFile(file.FullName, outDir, ?generateTips = generateTips, ?fsiEvaluator = fsiEvaluator)
    | ".fsx" -> GitBook.GenerateOutputFromScriptFile(file.FullName, outDir, ?generateTips = generateTips, ?fsiEvaluator = fsiEvaluator)
    | _ -> failwithf "%s does not support" file.FullName

  static member GenerateFromFile(fileName, outDir, ?generateTips, ?fsiEvaluator) =
    GitBook.GenerateFromFile(FileInfo(fileName), outDir, ?generateTips = generateTips, ?fsiEvaluator = fsiEvaluator)

  static member Generate(inputDir, outDir, ?generateTips, ?fsiEvaluator) =
    let getFiles targets =
      targets
      |> Array.collect (fun t -> Directory.GetFiles(inputDir, t, SearchOption.AllDirectories))
    let inputDir = Path.GetFullPath(inputDir)
    for file in getFiles [| "*.fsx"; "*.md" |] do
      let file = FileInfo(file)
      let outDir = Path.Combine(outDir, file.DirectoryName.Replace(inputDir, "").TrimStart('/', '\\'))
      GitBook.GenerateFromFile(file, outDir, ?generateTips = generateTips, ?fsiEvaluator = fsiEvaluator)
