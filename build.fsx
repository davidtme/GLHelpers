// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open System
open Fake
open Fake.NpmHelper
open Fake.ProcessHelper

Target "All" ignore

let root = __SOURCE_DIRECTORY__ 
let clientWebPath = "./src/WebGLHelpers"

Target "RestoreClient" <| fun _ ->
    Npm (fun p ->
        { p with
            WorkingDirectory = clientWebPath
            Command = Install Standard })

    DotNetCli.Restore(fun p -> 
        { p with 
            WorkingDir = clientWebPath })

Target "ClientWeb" <| fun _ ->
    DotNetCli.RunCommand(fun p -> 
        { p with 
            WorkingDir = clientWebPath }) "fable npm-build"

"RestoreClient"
==> "ClientWeb" 
==> "All"

Target "RunClientWeb" <| fun _ ->
    DotNetCli.RunCommand(fun p -> 
        { p with 
            WorkingDir = clientWebPath }) "fable webpack-dev-server"

"RestoreClient"
==> "RunClientWeb" 

Target "ClientWindows" <| fun _ ->
    let path = "./src/OpenGLHelpers/OpenGLHelpers.fsproj"
    build id path

"ClientWindows"
==> "All" 

Target "RunClientWindows" <| fun _ ->
    let path = IO.Path.Combine(root, @"Build\Client-Windows\GLHelpers.exe")

    Shell.Exec path |> ignore

"ClientWindows"
==> "RunClientWindows"

RunTargetOrDefault "All"
