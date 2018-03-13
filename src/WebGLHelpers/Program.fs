module Program

open Fable.Import
open Models
open System.Collections.Generic
open GLHelpers

let textureQueue = ResizeArray<_>()

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
        { Enqueue = fun x -> textureQueue.Add(x)
          TryDequeue = fun () -> 
                if textureQueue.Count > 0 then
                    let a = textureQueue.[0]
                    textureQueue.RemoveAt(0)
                    Some a
                else None }

      LoadImage = fun name callback -> 
        let image = Browser.document.createElement_img()
        image.onload <- fun _ ->
            callback image
            obj()

        image.src <- "images/sprites/" + name + ".png"
    }

let holder = Browser.document.getElementById("App")

let canvas = Browser.document.createElement_canvas()
(holder : Browser.HTMLElement).appendChild(canvas) |> ignore
canvas.width <- holder.clientWidth
canvas.height <- holder.clientHeight

let context = canvas.``getContext_experimental-webgl``()

let mutable gameState : GameState = 
    { Resolution = int canvas.clientWidth, int canvas.clientHeight
      Players = 
        [| { X = 10; Y = 10; RenderItem = getRenderItem resources "Sprite1" } |] }

let _gameAgent : MailboxProcessor<obj> = MailboxProcessor.Start(fun inbox ->
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

let renderer = GameRender.create context resources

let rec renderFrame _ =
    renderer gameState

    Browser.window.requestAnimationFrame(renderFrame) |> ignore

renderFrame 0.