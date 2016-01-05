#r @"packages/FAKE/tools/FakeLib.dll"
#I @"bin/Fake.GitBook"
#r @"bin/Fake.GitBook/Fake.GitBook.dll"
open Fake

Target "GenerateBook" (fun _ ->
  GitBook id (fun p -> { p with SrcDir = currentDirectory @@ "doc" }) [Html]
)

RunTargetOrDefault "GenerateBook"

