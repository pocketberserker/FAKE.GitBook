[<AutoOpen>]
module Fake.GitBookHelper

open System
open System.Text
open NpmHelper
open GitBook
open FSharp.Literate

type OutputFormat =
  | Html
  | Pdf of string
  | EPub of string
  member this.Command(dir) =
    match this with
    | Html -> sprintf "build %s" dir
    | Pdf name -> sprintf "pdf %s %s" dir (dir @@ name)
    | EPub name -> sprintf "epub %s %s" dir (dir @@ name)

type GitBookParams = {
  ToolPath: string
  SrcDir: string
  CompiledSrcDir: string
  BookBuildDir: string
  FsiEvaluator: IFsiEvaluator option
}

let defaultBookBuilderDir = currentDirectory @@ "gitbook"
let defaultNodeModulesPath = currentDirectory @@ "node_modules"

let GitBookDefaults = {
  ToolPath =
    let name = if isUnix then "gitbook" else "gitbook.cmd"
    findToolInSubPath name defaultNodeModulesPath
  SrcDir = currentDirectory @@ "src"
  CompiledSrcDir = defaultBookBuilderDir
  BookBuildDir = defaultBookBuilderDir
  FsiEvaluator = Some(FsiEvaluator() :> IFsiEvaluator)
}

let buildGitBookArgs parameters (format: OutputFormat) =
  StringBuilder()
  |> appendWithoutQuotes (format.Command(parameters.BookBuildDir))
  |> toText

let GitBookOnly setParams format =
  traceStartTask "gitbook" ""
  let parameters = setParams GitBookDefaults
  let args = buildGitBookArgs parameters format
  trace (parameters.ToolPath + " " + args)
  if 0 <> ExecProcess (fun info -> 
    info.FileName <- parameters.ToolPath
    info.Arguments <- args) TimeSpan.MaxValue
  then 
    failwithf "Failed: gitbook %s" args
  traceEndTask "gitbook" ""

let GitBook setNpmParams setParams format =
  if not <| directoryExists defaultNodeModulesPath then
    Npm (fun p ->
      { setNpmParams p with
         Command = Install Standard
         WorkingDirectory = currentDirectory
      })
  let parameters = setParams GitBookDefaults
  match parameters.FsiEvaluator with
  | Some fsi -> GitBook.Generate(parameters.SrcDir, parameters.CompiledSrcDir, fsi)
  | None -> GitBook.Generate(parameters.SrcDir, parameters.CompiledSrcDir)
  GitBookOnly setParams format
