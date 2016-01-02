namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("Fake.GitBook")>]
[<assembly: AssemblyProductAttribute("FAKE.GitBook")>]
[<assembly: GuidAttribute("de681942-cfcc-41d6-ac28-b1c9a5859679")>]
[<assembly: AssemblyDescriptionAttribute("FAKE extension for GitBook")>]
[<assembly: AssemblyVersionAttribute("0.0.2")>]
[<assembly: AssemblyFileVersionAttribute("0.0.2")>]
[<assembly: AssemblyInformationalVersionAttribute("0.0.2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.2"
