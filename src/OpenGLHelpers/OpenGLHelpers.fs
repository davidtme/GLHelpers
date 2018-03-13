module GLHelpers

open OpenTK
open OpenTK.Graphics.ES20

type TextureBitmap = byte []
type TextureData = byte []
type Texture = int

let inline glFloat f = float32 f

let createVector2Buffer _ (items : Vector2 []) =
    let mutable buffer = 0
    GL.GenBuffers(1, &buffer)
    GL.BindBuffer(BufferTarget.ArrayBuffer, buffer)

#if WINDOWS
    GL.BufferData(BufferTarget.ArrayBuffer, nativeint(items.Length * Vector2.SizeInBytes), items, BufferUsageHint.StaticDraw)
#endif
#if ANDROID
    GL.BufferData(BufferTarget.ArrayBuffer, nativeint(items.Length * Vector2.SizeInBytes), items, BufferUsage.StaticDraw)
#endif

    buffer

let buildVector2ArrayAttribute _ (program : int) (name) =
    let attributeLocation = GL.GetAttribLocation(program, name)
    GL.EnableVertexAttribArray(attributeLocation)

    fun (buffer : int) ->
        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer)
        GL.VertexAttribPointer(attributeLocation, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0)


let buildUniformVec2 _ (program : int) (name) =
    let location = GL.GetUniformLocation (program, name)
    fun (x, y) ->
        GL.Uniform2 (location, Vector2(x, y))

let buildUniformVec3 _ (program : int) (name) =
    let location = GL.GetUniformLocation (program, name)
    fun (x, y, z) ->
        GL.Uniform3 (location, Vector3(x, y, z))

let buildUniformVec4 _ (program : int) (name) =
    let location = GL.GetUniformLocation (program, name)
    fun (x, y, z, w) ->
        GL.Uniform4 (location, Vector4(x, y, z, w))

let createShaderProgram _ vertex fragment =
    let vertexShader = GL.CreateShader(ShaderType.VertexShader)
    GL.ShaderSource(vertexShader, vertex)
    GL.CompileShader(vertexShader)

    let mutable compileResult1 = 0
    GL.GetShader(vertexShader, ShaderParameter.CompileStatus, &compileResult1)
    if compileResult1 <> 1 then
        invalidOp "Error"

    let fragShader = GL.CreateShader(ShaderType.FragmentShader)
    GL.ShaderSource(fragShader, fragment)
    GL.CompileShader(fragShader)

    let mutable compileResult2 = 0
    GL.GetShader(fragShader, ShaderParameter.CompileStatus, &compileResult2)
    if compileResult2 <> 1 then
        invalidOp "Error"

    let program = GL.CreateProgram()
    GL.AttachShader(program, vertexShader)
    GL.AttachShader(program, fragShader)
    GL.LinkProgram(program)

    GL.UseProgram(program)

    program

let useProgram _ (program : int) =
    GL.UseProgram(program)

let defaultBlend _ =
    GL.Enable(EnableCap.Blend)
    GL.BlendEquation(BlendEquationMode.FuncAdd)
    GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.One);

let buildUniformTexture _ (program : int) (name : string) (number : int) =
    let location = GL.GetUniformLocation (program, name)
    fun (texture : int) ->
        match number with
        | 1 -> GL.ActiveTexture (TextureUnit.Texture1)
        | 2 -> GL.ActiveTexture (TextureUnit.Texture2)
        | _ -> GL.ActiveTexture (TextureUnit.Texture0)
        
        GL.BindTexture (TextureTarget.Texture2D, texture)
        GL.Uniform1 (location, number)

let draw _ =
#if WINDOWS
    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4)
#endif
#if ANDROID
    GL.DrawArrays(BeginMode.TriangleStrip, 0, 4)
#endif

let clear _ (width, height) =
    GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f)
    GL.Viewport(0, 0, width, height)
    GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

let createTexture _ (width, height) (image : byte[])  =
    let texture  : Texture = GL.GenTexture()

    GL.BindTexture(TextureTarget.Texture2D, texture)

    if width = 512 && height = 512 then
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.TextureWrapS)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.TextureWrapT)
    else
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge)

    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest)

#if WINDOWS
    GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image)
#endif
#if ANDROID
    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image)
#endif

    texture

module Templates = 
    let spriteVertexArray =
        [| Vector2(-1.0f,  1.0f) // bottom left
           Vector2(-1.0f, -1.0f) // top left
           Vector2( 1.0f,  1.0f) // bottom right
           Vector2( 1.0f, -1.0f) // top right
        |]

    let spriteFragmentArray =
        [| Vector2(0.0f, 0.0f) // bottom left
           Vector2(0.0f, 1.0f) // top left
           Vector2(1.0f, 0.0f) // bottom right
           Vector2(1.0f, 1.0f) // top right
        |]