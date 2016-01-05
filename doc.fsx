#r @"packages/FAKE/tools/FakeLib.dll"
#I @"bin/Fake.GitBook"
#r @"bin/Fake.GitBook/Fake.GitBook.dll"
open Fake

Target "Generate" (fun _ ->
  GitBook id (fun p -> { p with SrcDir = currentDirectory @@ "doc" }) [Html]
)

Target "GenerateAll" (fun _ ->
  GitBook id (fun p -> { p with SrcDir = currentDirectory @@ "doc" }) [ Html; Pdf "book"; EPub "book" ]
)

RunTargetOrDefault "Generate"

