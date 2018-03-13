module Models

open System.Collections.Generic
open System.Collections.Concurrent
open System
open GLHelpers
open AssetManager.Images

type Level = 
    | Object = 0

let totalLevels = 1

type Layer = {
    Path : string
    Level : Level
}

type RenderElement = {
    Name: string
    Size: int * int
    Layers: Layer []
}

type TextureQueue =
    { Enqueue : ((int * int) * TextureData * (Texture -> unit)) -> unit
      TryDequeue : unit -> ((int * int) * TextureData * (Texture -> unit)) option }

[<NoEquality; NoComparison>]
type GameResources =
    { RenderElements : Map<string, RenderElement>
      RenderItems : Dictionary<Layer [], RenderItem>
      TextureQueue : TextureQueue
      LoadImage : string -> (TextureBitmap -> unit) -> unit }

and RenderItem(element : RenderElement, layers, resources : GameResources) =
    let renderSize =
        fst element.Size,
        snd element.Size 

    let images = 
        Array.init totalLevels (fun level ->
            None,
            layers
            |> Array.choose(fun (layer : Layer) ->
                if int layer.Level = level then
                    Some (Choice1Of2(layer))
                else None))

    let textureLoaded level (texture : Texture) =
        images.[level] <- Some (texture), [||]

    let imageLoaded level imageIndex (image : TextureBitmap) =
        let _, layers = images.[level]
        layers.[imageIndex] <- (Choice2Of2 image)

        let loadedImages = 
            layers
            |> Array.choose(function Choice2Of2 x -> Some x | _ -> None)

        if loadedImages.Length = layers.Length then
            let bytes = renderBitmaps renderSize loadedImages

            resources.TextureQueue.Enqueue(
                renderSize,
                bytes,
                (textureLoaded level))

    do
        images
        |> Array.iteri(fun level (_, layers) ->
            layers
            |> Array.iteri(fun imageIndex layer -> 
                match layer with
                | Choice1Of2 layer ->
                    resources.LoadImage layer.Path (imageLoaded level imageIndex) 
                | _ ->
                    ignore()
            )
        )

    member __.Object with get() = images.[int Level.Object] |> fst
    member __.Element with get() = element

let getRenderItem resources name =
    let element = resources.RenderElements |> Map.find name
    let layers = element.Layers

    match resources.RenderItems.TryGetValue(layers) with
    | true, item -> item
    | _ ->
        let result =                
            RenderItem(
                element,
                layers,
                resources)

        resources.RenderItems.Add(layers, result)
        result

type Player =
    { X : int
      Y : int
      RenderItem : RenderItem }

type GameState = 
    { Resolution : int * int
      Players : Player [] }