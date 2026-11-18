using System.IO;
using UnityEngine;

public static class RuntimeTextureLoader
{
    public static Texture2D LoadPng(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return Texture2D.whiteTexture;

        byte[] bytes = File.ReadAllBytes(path);
        var decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
        if (!decoded.LoadImage(bytes, false))
            return Texture2D.whiteTexture;

        // LoadImage creates an sRGB texture. FX atlases must sample authored
        // bytes as Linear (Inspector sRGB = OFF). Copy the raw 8-bit texels
        // into a linear Texture2D so URP does not decode them again.
        var tex = new Texture2D(decoded.width, decoded.height, TextureFormat.RGBA32, false, true);
        tex.name = Path.GetFileNameWithoutExtension(path);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels32(decoded.GetPixels32());
        tex.Apply(false, false);
        UnityEngine.Object.Destroy(decoded);
        return tex;
    }

    public static Texture2D White()
    {
        return Texture2D.whiteTexture;
    }
}
