#r @"packages/FAKE/tools/FakeLib.dll"
#I @"bin/Fake.GitBook"
#r @"bin/Fake.GitBook/Fake.GitBook.dll"
#load @"./Project.fsx"
open Fake
open Fake.Git
open Project

Target "Generate" (fun _ ->
  GitBook id (fun p -> { p with SrcDir = currentDirectory @@ "doc" }) [Html]
)

Target "GenerateAll" (fun _ ->
  GitBook id (fun p -> { p with SrcDir = currentDirectory @@ "doc" }) [ Html; Pdf "book"; EPub "book" ]
)

Target "ReleaseDocs" (fun () ->
  let tempDocsDir = "temp/gh-pages"
  CleanDir tempDocsDir
  Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

  CopyRecursive "gitbook/_book" tempDocsDir true |> tracefn "%A"
  StageAll tempDocsDir
  Git.Commit.Commit tempDocsDir "update docs"
  Branches.push tempDocsDir
)

"Generate"
  ==> "ReleaseDocs"

RunTargetOrDefault "Generate"

