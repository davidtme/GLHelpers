module GLHelpers

open Fable.Import
open Fable.Core.JsInterop

type GL = Browser.WebGLRenderingContext

type TextureBitmap = Browser.HTMLImageElement
type TextureData = Browser.ImageData
type Texture = Browser.WebGLTexture

let inline glFloat f = float f

let createVector2Buffer (gl : GL) (items : JS.Float32Array) =
    let buffer = gl.createBuffer()
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer)
    gl.bufferData(gl.ARRAY_BUFFER, unbox items, gl.STATIC_DRAW)
    buffer

let buildVector2ArrayAttribute (GL : GL) (program) (name) =
    let attributeLocation = GL.getAttribLocation(program, name)
    GL.enableVertexAttribArray(attributeLocation)

    fun buffer -> 
        GL.bindBuffer(GL.ARRAY_BUFFER, buffer)
        GL.vertexAttribPointer(attributeLocation, 2., GL.FLOAT, false, 0., 0.)
        
let buildUniformVec2 (GL : GL) (program) (name) =
    let location = GL.getUniformLocation (program, name)

    fun (x, y) ->
        GL.uniform2f (location, x, y)

let buildUniformVec3 (GL : GL) (program) (name) =
    let location = GL.getUniformLocation (program, name)

    fun (x, y, z) ->
        GL.uniform3f (location, x, y, z)

let buildUniformVec4 (GL : GL) (program) (name) =
    let location = GL.getUniformLocation (program, name)

    fun (x, y, z, w) ->
        GL.uniform4f (location, x, y, z, w)

let createShaderProgram (gl : GL) vertex fragment =
    let vertexShader = gl.createShader(gl.VERTEX_SHADER)
    gl.shaderSource(vertexShader, vertex)
    gl.compileShader(vertexShader)

    let fragShader = gl.createShader(gl.FRAGMENT_SHADER)
    gl.shaderSource(fragShader, fragment)
    gl.compileShader(fragShader)

    let program = gl.createProgram()
    gl.attachShader(program, vertexShader)
    gl.attachShader(program, fragShader)
    gl.linkProgram(program)
        
    program

let buildUniformTexture (GL : GL) (program) (name : string) (number : int) =
    let location = GL.getUniformLocation (program, name)
    fun (texture:Texture) ->
        match number with
        | 0 -> GL.activeTexture (GL.TEXTURE0)
        | 1 -> GL.activeTexture (GL.TEXTURE1)
        | 2 -> GL.activeTexture (GL.TEXTURE2)
        | 3 -> GL.activeTexture (GL.TEXTURE3)
        | 4 -> GL.activeTexture (GL.TEXTURE4)
        | 5 -> GL.activeTexture (GL.TEXTURE5)
        | _ -> invalidOp "Known texture number"

        GL.bindTexture (GL.TEXTURE_2D, texture)
        GL.uniform1i (location, float number)


let useProgram (gl : GL) program =
    gl.useProgram (program)

let defaultBlend (gl : GL) =
    gl.enable(gl.BLEND)
    gl.blendEquation(gl.FUNC_ADD)
    gl.blendFuncSeparate(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA, gl.ONE, gl.ONE)

let draw (gl : GL) =
    gl.drawArrays(gl.TRIANGLE_STRIP, 0., 4.)
    
let clear (gl : GL) (width, height) =
    gl.clearColor(0.0, 0.0, 0.0, 1.0)
    gl.viewport(0., 0., float width, float height)
    gl.clear(float (int gl.COLOR_BUFFER_BIT ||| int gl.DEPTH_BUFFER_BIT))
    
let createTexture (gl : GL) size (image : TextureData) =
    let texture = gl.createTexture()

    gl.bindTexture(gl.TEXTURE_2D, texture)

    gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, 1.)
    gl.texImage2D(gl.TEXTURE_2D, 0., gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image)

    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST)
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST)

    texture

module Templates =
    let spriteVertexArray : JS.Float32Array =
        [| -1.0f;  1.0f; // bottom left
            -1.0f; -1.0f; // top left
            1.0f;  1.0f; // bottom right
            1.0f; -1.0f; // top right
        |]
        |> createNew JS.Float32Array
        |> unbox


    let spriteFragmentArray : JS.Float32Array =
        [| 0.0f; 0.0f; // bottom left
            0.0f; 1.0f; // top left
            1.0f; 0.0f; // bottom right
            1.0f; 1.0f; // top right
        |]
        |> createNew JS.Float32Array
        |> unbox