#r @"paket:
nuget Fake.Core.Target
nuget Fake.Core.Process
nuget Fake.Core.ReleaseNotes
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.Paket
nuget Fake.DotNet.FSFormatting
nuget Fake.DotNet.Fsi
nuget Fake.DotNet.NuGet
nuget Fake.DotNet.Testing.Expecto
nuget Fake.Tools.Git
nuget Fake.Api.GitHub //"

#if !FAKE
#load "./.fake/build.fsx/intellisense.fsx"
#r "netstandard" // Temp fix for https://github.com/dotnet/fsharp/issues/5216
#endif

open System
open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Tools

let project = "FSharp.Plotly"

let summary = "A F# interactive charting library using plotly.js"

let solutionFile  = "FSharp.Plotly.sln"

let configuration = "Release"

let testAssemblies = "tests/**/bin" </> configuration </> "**" </> "*Tests.exe"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "muehlhaus"
let gitHome = sprintf "%s/%s" "https://github.com" gitOwner

let gitName = "FSharp.Plotly"

let website = "/FSharp.Plotly"

let pkgDir = "pkg"

let release = ReleaseNotes.load "RELEASE_NOTES.md"

let mutable prereleaseTag = ""
let mutable isPrerelease = false

[<AutoOpen>]
module MessagePrompts =

    let prompt (msg:string) =
        System.Console.Write(msg)
        System.Console.ReadLine().Trim()
        |> function | "" -> None | s -> Some s
        |> Option.map (fun s -> s.Replace ("\"","\\\""))

    let rec promptYesNo msg =
        match prompt (sprintf "%s [Yn]: " msg) with
        | Some "Y" | Some "y" -> true
        | Some "N" | Some "n" -> false
        | _ -> System.Console.WriteLine("Sorry, invalid answer"); promptYesNo msg

    let releaseMsg = """This will stage all uncommitted changes, push them to the origin and bump the release version to the latest number in the RELEASE_NOTES.md file. 
        Do you want to continue?"""

    let releaseDocsMsg = """This will push the docs to gh-pages. Remember building the docs prior to this. Do you want to continue?"""

// Generate assembly info files with the right version & up-to-date information
Target.create "AssemblyInfo" (fun _ ->
    let getAssemblyInfoAttributes projectName =
        [ AssemblyInfo.Title (projectName)
          AssemblyInfo.Product project
          AssemblyInfo.Description summary
          AssemblyInfo.Version release.AssemblyVersion
          AssemblyInfo.FileVersion release.AssemblyVersion ]

    let getProjectDetails projectPath =
        let projectName = Path.GetFileNameWithoutExtension(projectPath)
        (Path.GetDirectoryName(projectPath), getAssemblyInfoAttributes projectName)

    !! "src/**/*.fsproj"
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (folderName, attributes) ->
        AssemblyInfoFile.createFSharp (folderName </> "AssemblyInfo.fs") attributes )
)

// Copies binaries from default VS location to expected bin folder
// But keeps a subdirectory structure for each project in the
// src folder to support multiple project outputs
Target.create "CopyBinaries" (fun _ ->
    !! "src/**/*.fsproj"
    |>  Seq.map (fun f -> ((Path.getDirectory f) </> "bin" </> configuration, "bin" </> (Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> Shell.copyDir toDir fromDir (fun _ -> true))
)

Target.create "CopyBinariesDotnet" (fun _ ->
    !! "src/**/*.fsproj"
    -- "src/FSharp.Plotly.WPF/FSharp.Plotly.WPF.fsproj"
    |>  Seq.map (fun f -> ((Path.getDirectory f) </> "bin" </> "Dotnet", "bin" </> (Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> Shell.copyDir toDir fromDir (fun _ -> true))
)


// --------------------------------------------------------------------------------------
// Clean build results

let buildConfiguration = DotNet.Custom <| Environment.environVarOrDefault "configuration" configuration

let dotnetCoreConfiguration = DotNet.Custom "Dotnet"

Target.create "Clean" (fun _ ->
    Shell.cleanDirs ["bin"; "temp"; "pkg"]
)

Target.create "CleanDocs" (fun _ ->
    Shell.cleanDirs ["docs"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target.create "Build" (fun _ ->
    solutionFile
    |> DotNet.build (fun p ->
        { p with
            Configuration = buildConfiguration })
)

Target.create "BuildDotnet" (fun _ ->
    solutionFile 
    |> DotNet.build (fun p -> 
        { p with
            Configuration = dotnetCoreConfiguration }
        )
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target.create "RunTests" (fun _ ->
    let assemblies = !! testAssemblies

    assemblies
    |> Seq.iter (fun f ->
        Command.RawCommand (
            f,
            Arguments.OfArgs []
        )
        |> CreateProcess.fromCommand
        |> CreateProcess.withFramework
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore
    )
)

// --------------------------------------------------------------------------------------
// Build and publish NuGet package

Target.create "buildPrereleasePackages" (fun _ -> 
        printfn "Please enter pre-release package suffix"
        let suffix = System.Console.ReadLine()
        isPrerelease <- true
        prereleaseTag <- (sprintf "%s-%s" release.NugetVersion suffix)
        if promptYesNo (sprintf "package tag will be %s OK?" prereleaseTag )
            then 
                Paket.pack(fun p -> 
                    { p with
                
                        ToolType = ToolType.CreateLocalTool()
                        OutputPath = pkgDir
                        Version = prereleaseTag
                        ReleaseNotes = release.Notes |> String.toLines })
            else 
                failwith "aborted"
    )

Target.create "BuildReleasePackages" (fun _ ->
        isPrerelease <- false
        Paket.pack(fun p ->
            { p with
                ToolType = ToolType.CreateLocalTool()
                OutputPath = pkgDir
                Version = release.NugetVersion
                ReleaseNotes = release.Notes |> String.toLines })
)
Target.create "BuildCIPackages" (fun _ ->
    Paket.pack(fun p ->
        { p with
            ToolType = ToolType.CreateLocalTool()
            OutputPath = pkgDir
            Version = sprintf "%s-appveyor.%s" release.NugetVersion BuildServer.appVeyorBuildVersion
            ReleaseNotes = release.Notes |> String.toLines })
        )

Target.create "PublishNuget" (fun _ ->
    Paket.push(fun p ->
        { p with
            WorkingDir = pkgDir
            PublishUrl = "https://www.nuget.org"
            ApiKey = Environment.environVarOrDefault "NuGet-key" ""})
)

Target.create "publishPrereleaseNugetPackages"(fun _ ->
    Paket.push(fun p ->
        { p with
            WorkingDir = pkgDir
            ToolType = ToolType.CreateLocalTool()
            ApiKey = Environment.environVarOrDefault "NuGet-key" "" })
)

// --------------------------------------------------------------------------------------
// Generate the documentation

Target.create "GenerateDocs" (fun _ ->
    let result =
        DotNet.exec
            (fun p -> { p with WorkingDirectory = __SOURCE_DIRECTORY__ @@ "docsrc" @@ "tools" })
            "fsi"
            "--define:RELEASE --define:REFERENCE --define:HELP --exec generate.fsx"

    if not result.OK then failwith "error generating docs"
)

// --------------------------------------------------------------------------------------
// Release Scripts


//#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
//open Octokit
Target.create "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    Shell.cleanDir tempDocsDir |> ignore
    Git.Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir
    Shell.copyRecursive "docs" tempDocsDir true |> printfn "%A"
    Git.Staging.stageAll tempDocsDir
    Git.Commit.exec tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Git.Branches.push tempDocsDir
)

Target.create "ReleaseDocsLocal" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    Shell.cleanDir tempDocsDir |> ignore
    Shell.copyRecursive "docs" tempDocsDir true  |> printfn "%A"
    Shell.replaceInFiles 
        (seq {
            yield "href=\"/" + project + "/","href=\""
            yield "src=\"/" + project + "/","src=\""}) 
        (Directory.EnumerateFiles tempDocsDir |> Seq.filter (fun x -> x.EndsWith(".html")))
)
Target.create "Release" (fun _ ->
    Git.Staging.stageAll ""
    Git.Commit.exec "" (sprintf "Bump version to %s" release.NugetVersion)
    Git.Branches.push ""

    Git.Branches.tag "" release.NugetVersion
    Git.Branches.pushTag "" "upstream" release.NugetVersion
)

Target.create "GitReleaseNuget" (fun _ ->
    let tempNugetDir = "temp/nuget"
    Shell.cleanDir tempNugetDir |> ignore
    Git.Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "nuget" tempNugetDir
    let files = Directory.EnumerateFiles(__SOURCE_DIRECTORY__ @@ "bin")
    Shell.copy tempNugetDir files
    Git.Staging.stageAll tempNugetDir
    Git.Commit.exec tempNugetDir (sprintf "Update git nuget packages for version %s" release.NugetVersion)
    Git.Branches.push tempNugetDir
)

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target.create "All" ignore
Target.create "CIBuild" ignore
Target.create "BuildOnly" ignore
Target.create "DotnetCoreBuild" ignore


"Clean"
  ==> "CleanDocs"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "CopyBinaries"
  ==> "RunTests"
  ==> "BuildReleasePackages"
  ==> "PublishNuget"
  ==> "Release"

"Clean"
  ==> "CleanDocs"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "CopyBinaries"
  ==> "RunTests"
  ==> "GenerateDocs"
  ==> "BuildReleasePackages"
  ==> "All"

"Clean"
  ==> "CleanDocs"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "CopyBinaries"
  ==> "RunTests"
  ==> "BuildCIPackages"
  ==> "CIBuild"

"Clean"
==> "CleanDocs"
==> "AssemblyInfo"
==> "BuildDotnet"
==> "CopyBinariesDotnet"
==> "DotnetCoreBuild"

"Clean"
  ==> "CleanDocs"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "CopyBinaries"
  ==> "BuildOnly"

"GenerateDocs"
  ==> "ReleaseDocsLocal"

"Clean"
  ==> "CleanDocs"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "CopyBinaries"
  ==> "RunTests"
  ==> "BuildPreReleasePackages"
  ==> "publishPrereleaseNugetPackages"

Target.runOrDefaultWithArguments "All"
