GL Helpers
==========

This project has OpenGL (via OpenTK) and WebGL F# functions that are closely aligned to each other so the same shared code can be reused in a Windows and Fable project (Android will also be included at some point).

Shared Render Code:
```
let buildSpriteRenderer gl = 
    let program = createShaderProgram gl spriteVertexShader spriteFragmentShader
    useProgram gl program
    program

```

_For the fable/web project the `gl` paramter is the `HTMLCanvasContext`_

Fable:
```
let context = canvas.``getContext_experimental-webgl``()
let program = spriteRenderProgram context

```

_For the windows project the `gl` parameter is thrown away so can be anything such as a `unit`_

Windows:
```
type GameWindow(width, height) =
    inherit OpenTK.GameWindow(width, height)
    let mutable program = 0

    override x.OnLoad(e : EventArgs) =
        program <- spriteRenderProgram ()
```


Setup 
-----

Add the following to your `paket.dependencies`

```
nuget OpenTK
nuget Fable.Import.Browser

github davidtme/GLHelpers src/OpenGLHelpers/OpenGLHelpers.fs
github davidtme/GLHelpers src/WebGLHelpers/WebGLHelpers.fs
```

### Fable/Web Project

Add the following to you fable projects `paket.references`

```
Fable.Import.Browser
File: WebGLHelpers.fs
```

### Windows Project

Add the following to you windows project `paket.references`

```
OpenTK
File: OpenGLHelpers.fs
```


Demo Code
---------

The project contains test projects which demos loading and displayer a single sprite in both a Windows and Fable/Web project.

First clone the repo then run:

`build RunClientWeb` to start the website and go to http://localhost:8081

`build RunClientWindows` to start the windows app 