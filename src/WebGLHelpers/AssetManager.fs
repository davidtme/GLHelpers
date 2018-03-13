module AssetManager

module Images =
    open Fable.Core
    open Fable.Import

    let resultCanvas = Browser.document.createElement_canvas()
    let resultContext = resultCanvas.getContext_2d()

    let renderBitmaps (width, height) bitmaps =
        let width = float width
        let height = float height

        resultCanvas.width <- width
        resultCanvas.height <- height
        resultContext.clearRect(0., 0., width, height)

        bitmaps
        |> Array.iter(fun (currentImage : Browser.HTMLImageElement) ->
            resultContext.drawImage(U3.Case1(currentImage), 0., 0., width, height)
        )

        resultContext.getImageData(0., 0., width, height)