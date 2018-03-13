module GameRender

open GLHelpers
open GLHelpers.Templates
open Models

let inline intsToFloat2 (x : int, y : int) = glFloat x, glFloat y
let inline intsToFloat3 (x : int, y : int, z : int) = glFloat x, glFloat y, glFloat z
let inline intsToFloat4 (x : int, y : int, z : int, w : int) = glFloat x, glFloat y, glFloat z, glFloat w

let spriteVertexShader = """
attribute vec3 aVertexPosition;
attribute vec2 aTextureCoord;

uniform vec2 uResolution;
uniform vec4 uPosition;

varying vec2 vTextureCoord;
varying vec2 vResolution;
varying vec4 vPosition;

void main(void) {
    gl_Position = vec4(-1. + (((2. * uPosition.x) + (uPosition.z * (aVertexPosition.x + 1.))) / uResolution.x),
                        1. - (((2. * uPosition.y) - (uPosition.w * (aVertexPosition.y - 1.))) / uResolution.y),
                        0.0,
                        1.0);

    vTextureCoord = aTextureCoord;
    vResolution = uResolution;
    vPosition = uPosition;
}
"""

let spriteFragmentShader = """
precision mediump float;

varying vec2 vTextureCoord;
uniform sampler2D uTexture;
uniform vec2 uFrame;

void main() {
    vec4 color = texture2D(uTexture, vec2(vTextureCoord.x, vTextureCoord.y));
    gl_FragColor = color;
}
    """

let buildSpriteRenderer gl = 
    let program = createShaderProgram gl spriteVertexShader spriteFragmentShader
    useProgram gl program

    let vertexBuffer = createVector2Buffer gl spriteVertexArray
    let fragBuffer = createVector2Buffer gl spriteFragmentArray

    let vertexPosition = buildVector2ArrayAttribute gl program "aVertexPosition"
    let textureCoordAttribute = buildVector2ArrayAttribute gl program "aTextureCoord"

    let textureUniform = buildUniformTexture gl program "uTexture" 0
    let resolutionUniform = intsToFloat2 >> buildUniformVec2 gl program "uResolution"
    let positionUniform = (fun ((x,y),(w,h)) -> x,y,w,h) >> intsToFloat4 >> buildUniformVec4 gl program "uPosition"
         
    fun resolution position texture ->
        useProgram gl program

        vertexPosition vertexBuffer
        textureCoordAttribute fragBuffer

        defaultBlend gl
        resolutionUniform resolution

        positionUniform position
        textureUniform texture

        draw gl

 
let create gl (resources : GameResources) = 

    let spriteRenderer = buildSpriteRenderer gl

    fun (state : GameState) ->

    clear gl state.Resolution

    let rec renderTextures() = 
        match resources.TextureQueue.TryDequeue() with
        | Some (size, textureData, callback) ->
            createTexture gl size textureData |> callback
            renderTextures()
        | _ -> ignore()

    renderTextures()

    state.Players
    |> Array.iter(fun player ->
        match player.RenderItem.Object with
        | Some (texture) -> 
            spriteRenderer state.Resolution ((player.X, player.Y), player.RenderItem.Element.Size) texture
        | _ -> ignore()
    )

    ignore()