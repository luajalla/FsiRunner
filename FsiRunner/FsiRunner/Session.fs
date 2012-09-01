namespace ClearLines.FsiRunner

open System.Diagnostics
open Microsoft.FSharp.Compiler.Server.Shared


type public FsiSession(fsiPath: string) =
    let pause() = System.Threading.Thread.Sleep 5000 

    let info = new ProcessStartInfo()
    let fsiProcess = new Process()
    let serverName = sprintf "fsiClient%A" (System.Guid.NewGuid())
    
    do
        info.RedirectStandardInput <- true
        info.RedirectStandardOutput <- true
        info.RedirectStandardError <- true
        info.UseShellExecute <- false
        info.CreateNoWindow <- true
        info.FileName <- fsiPath
        info.Arguments <- "--fsi-server:" + serverName
        fsiProcess.StartInfo <- info

    let client = lazy (FSharpInteractiveServer.StartClient serverName) 

    [<CLIEvent>]
    member this.OutputReceived = fsiProcess.OutputDataReceived

    [<CLIEvent>]
    member this.ErrorReceived = fsiProcess.ErrorDataReceived

    member this.Start() =
        fsiProcess.Start() |> ignore
        fsiProcess.BeginOutputReadLine()
        fsiProcess.BeginErrorReadLine() 
        pause()     

    member this.AddLine(line: string) =
        fsiProcess.StandardInput.WriteLine(line)

    member this.Evaluate() =
        this.AddLine(";;")
        fsiProcess.StandardInput.Flush()

    member this.Completions text =
        client.Value.Completions text
