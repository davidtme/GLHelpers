module Program

open System
open Models
open System.Collections.Generic
open System.Collections.Concurrent
open System.Resources
open System.Reflection

let resourceAgent = MailboxProcessor.Start(fun inbox ->
    let spritesManager = new ResourceManager("Data", Assembly.GetExecutingAssembly())
    
    let rec loop () =
        async {
            let! (path, callback) = inbox.Receive()
            let bitmap : byte[] = spritesManager.GetObject("sprites/"+path) |> unbox
            callback bitmap

            return! loop() }

    loop())

type GameWindow(width, height) =
    inherit OpenTK.GameWindow(width, height)

    let textureQueue = ConcurrentQueue<_>()

    

    let resources : GameResources =
        { RenderElements = 
            [ { Name = "Sprite1"
                Size = 64,64
                Layers =
                    [| { Path = "sprite1"; Level = Level.Object } |] } ]
            |> List.map(fun x -> x.Name, x)
            |> Map.ofList
          RenderItems = Dictionary<_,_>()
          TextureQueue = 
            { Enqueue = textureQueue.Enqueue
              TryDequeue = fun () -> 
                match textureQueue.TryDequeue() with
                | true, x -> Some x
                | _ -> None }
          LoadImage = fun name callback -> resourceAgent.Post(name, callback)
        }

    let mutable gameState : GameState = 
        { Resolution = width, height
          Players = 
            [| { X = 10; Y = 10; RenderItem = getRenderItem resources "Sprite1" } |] }

    let _gameAgent = MailboxProcessor.Start(fun inbox ->
        let rec loop state =
            async {
                let! message = inbox.Receive()

                let state, cmd = 
                    GameStateHandler.update resources message state

                gameState <- state

                cmd
                |> List.iter(fun sub -> sub inbox.Post)

                return! loop state
            }

        loop gameState
    )

    let mutable renderer = fun _ -> ()

    override x.OnLoad(e : EventArgs) =
        renderer <- GameRender.create () resources

    override x.OnRenderFrame(e : OpenTK.FrameEventArgs) =
        renderer gameState
        x.SwapBuffers()

[<EntryPoint>]
[<STAThread>]
let main _ =
    use gameWindow = new GameWindow(800,600)
    gameWindow.Run(60.0)
    0 // return an integer exit code