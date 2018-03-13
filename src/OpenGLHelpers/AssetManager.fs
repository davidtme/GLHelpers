module AssetManager

module Images =
    open System.Drawing
    open System.Drawing.Imaging
    open System.Runtime.InteropServices
    open System.IO

    let convertToTextureData (bitmap : Bitmap) =
        let bitmapData = bitmap.LockBits(Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        let size = (bitmap.Width * bitmap.Height * 4)
        let data : byte[] = Array.zeroCreate size

        Marshal.Copy(bitmapData.Scan0, data, 0, size);
        bitmap.UnlockBits(bitmapData)

        let result : byte[] = Array.copy data

        for index = 0 to (size / 4) - 1 do
            let a = data.[(index * 4) + 3]

            result.[(index * 4) + 0] <- data.[(index * 4) + 2]
            //result.[(index * 4) + 1] <- data.[(index * 4) + 1]
            result.[(index * 4) + 2] <- data.[(index * 4) + 0]
            //result.[(index * 4) + 3] <- data.[(index * 4) + 3]

        result

    let renderBitmaps (width, height) (bitmaps : byte[][]) =
        use image = new Bitmap(width, height, PixelFormat.Format32bppArgb)
        use graphics = Graphics.FromImage(image)

        bitmaps
        |> Array.iter(fun bytes ->
            use stream = new MemoryStream(bytes)
            use current = new Bitmap(stream)
            graphics.DrawImageUnscaled(current, 0, 0))

        convertToTextureData image

