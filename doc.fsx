#r @"packages/FAKE/tools/FakeLib.dll"
#I @"src/Fake.GitBook/bin/Release"
#r @"src/Fake.GitBook/bin/Release/Fake.GitBook.dll"
open Fake

Target "GenerateBook" (fun _ ->
  GitBook id (fun p -> { p with SrcDir = currentDirectory @@ "doc" }) [Html]
)

RunTargetOrDefault "GenerateBook"

