using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public sealed class ViewerResolvedAssets
{
    public Mesh Mesh;
    public Texture Texture;
    public string MeshSource;
    public string TextureSource;
    public string FbxPath;
    public string UModelLog;
}

public static class ViewerAssetResolver
{
    static readonly Regex TextureImport = new Regex(
        @"Texture'([^']+)'",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static ViewerResolvedAssets Resolve(ViewerEffectData effect, ViewerEmitterData emitter)
    {
        var result = new ViewerResolvedAssets();
        string cache = Path.Combine(ViewerPaths.AssetsCacheDir, Sanitize(effect.ClassName), Sanitize(emitter.ObjectName));
        Directory.CreateDirectory(cache);

        if (emitter.Kind == ViewerEmitterKind.Mesh)
            result.Mesh = ResolveMesh(emitter, cache, result);

        string textureRef = emitter.Texture;
        if (string.IsNullOrEmpty(textureRef))
            textureRef = FirstMeshTextureHint(result, cache);

        result.Texture = ResolveTextureRef(textureRef, cache, result) ?? RuntimeTextureLoader.White();
        if (!string.IsNullOrEmpty(result.TextureSource))
            Debug.Log("[ViewerEffects] texture " + emitter.DisplayName + " ← " + result.TextureSource);
        else if (emitter.Kind == ViewerEmitterKind.Mesh)
            Debug.LogWarning("[ViewerEffects] mesh " + emitter.DisplayName + " has no texture (UC Texture empty, umodel material unresolved)");

        ViewerMaterialFactory.WriteSidecarMat(
            Path.Combine(cache, Sanitize(emitter.DisplayName) + ".mat.txt"),
            emitter,
            result.TextureSource);
        return result;
    }

    static Mesh ResolveMesh(ViewerEmitterData emitter, string cache, ViewerResolvedAssets result)
    {
        if (!UnrealAssetRef.TryParse(emitter.StaticMesh, out UnrealAssetRef meshRef))
            return CreateFallbackQuad();

        string psk = FindFirst(cache, meshRef.Name, ".psk", ".pskx");
        string extractLog = ReadCacheLog(cache, meshRef.Name);
        bool cacheHasPng = HasPng(cache);
        // Old cache had mesh but no material PNG (umodel path missed .utx) → solid red/white.
        if (psk != null && !cacheHasPng && string.IsNullOrEmpty(emitter.Texture))
            psk = null;
        string extractDir = null;
        if (psk == null && UModelExtractor.IsAvailable)
        {
            foreach (string pkg in PackageLocator.FindPackageFiles(meshRef.Package, texture: false))
            {
                ViewerStatus.Instance.Set("создание", "UModel mesh " + meshRef.Name + " / " + Path.GetFileName(pkg));
                extractDir = UModelExtractor.ExtractObject(pkg, meshRef.Name, "StaticMesh", out extractLog);
                psk = FindFirst(extractDir, meshRef.Name, ".psk", ".pskx");
                if (psk != null)
                    break;
            }
        }
        if (psk == null)
            psk = PackageLocator.FindUnpackedMesh(meshRef.Name);

        if (psk == null)
            throw new FileNotFoundException(
                "Mesh not found: " + emitter.StaticMesh +
                " (put LineageEffectsStaticmeshes package in meshe_essend, or keep unpack fallback). " + extractLog);

        result.MeshSource = psk;
        result.UModelLog = extractLog;
        string destPsk = Path.Combine(cache, Path.GetFileName(psk));
        CopyIfNeeded(psk, destPsk);
        if (!string.IsNullOrEmpty(extractLog))
            File.WriteAllText(Path.Combine(cache, meshRef.Name + ".umodel.log.txt"), extractLog);

        CopySidecarProps(psk, cache);
        if (!string.IsNullOrEmpty(extractDir))
            CopyPngsFromExtract(extractDir, cache);

        string fbx = Path.Combine(cache, meshRef.Name + ".fbx");
        try
        {
            ViewerStatus.Instance.Set("конвертация", "PSK→FBX " + meshRef.Name);
            PskToFbxConverter.Convert(destPsk, fbx, out string blenderLog);
            result.FbxPath = fbx;
            File.WriteAllText(Path.Combine(cache, meshRef.Name + ".fbx.log.txt"), blenderLog);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ViewerEffects] Blender convert skipped: " + ex.Message);
        }

        return PskMeshLoader.Load(destPsk);
    }

    static Texture ResolveTextureRef(string rawRef, string cache, ViewerResolvedAssets result)
    {
        if (!UnrealAssetRef.TryParse(rawRef, out UnrealAssetRef texRef) || string.IsNullOrEmpty(texRef.Name))
            return null;

        string png = FindFirst(cache, texRef.Name, ".png", ".tga");
        string extractLog = "";
        if (png == null && UModelExtractor.IsAvailable)
        {
            string pkg = PackageLocator.FindPackageFile(
                             string.IsNullOrEmpty(texRef.Package) ? "LineageEffectsTextures" : texRef.Package,
                             texture: true)
                         ?? PackageLocator.FindPackageFile("LineageEffectsTextures", texture: true);
            if (pkg != null)
            {
                ViewerStatus.Instance.Set("создание", "UModel texture " + texRef.Name);
                string outDir = UModelExtractor.ExtractObject(pkg, texRef.Name, "Texture", out extractLog);
                png = FindFirst(outDir, texRef.Name, ".png", ".tga");
            }
        }
        if (png == null)
            png = PackageLocator.FindUnpackedTexture(texRef.Name);

        if (png == null)
        {
            Debug.LogWarning("[ViewerEffects] Texture not found: " + rawRef + " " + extractLog);
            return null;
        }

        result.TextureSource = png;
        string dest = Path.Combine(cache, Path.GetFileName(png));
        CopyIfNeeded(png, dest);
        return RuntimeTextureLoader.LoadPng(dest);
    }

    static string FirstMeshTextureHint(ViewerResolvedAssets result, string cache)
    {
        foreach (string png in SafeGetFiles(cache, "*.png"))
            return Path.GetFileNameWithoutExtension(png);

        string log = result.UModelLog ?? "";
        foreach (string file in SafeGetFiles(cache, "*.umodel.log.txt"))
            log += "\n" + File.ReadAllText(file);
        foreach (string file in SafeGetFiles(cache, "*.props.txt"))
            log += "\n" + File.ReadAllText(file);
        if (!string.IsNullOrEmpty(result.MeshSource))
        {
            string props = Path.ChangeExtension(result.MeshSource, ".props.txt");
            if (File.Exists(props))
                log += "\n" + File.ReadAllText(props);
        }

        foreach (Match match in TextureImport.Matches(log))
        {
            string raw = match.Groups[1].Value;
            if (UnrealAssetRef.TryParse(raw, out UnrealAssetRef tex) && !string.IsNullOrEmpty(tex.Name))
                return raw;
        }
        return null;
    }

    static void CopySidecarProps(string psk, string cache)
    {
        string props = Path.ChangeExtension(psk, ".props.txt");
        if (File.Exists(props))
            CopyIfNeeded(props, Path.Combine(cache, Path.GetFileName(props)));
    }

    static void CopyPngsFromExtract(string extractDir, string cache)
    {
        if (string.IsNullOrEmpty(extractDir) || !Directory.Exists(extractDir))
            return;
        foreach (string png in Directory.GetFiles(extractDir, "*.png", SearchOption.AllDirectories))
            CopyIfNeeded(png, Path.Combine(cache, Path.GetFileName(png)));
    }

    static bool HasPng(string dir)
    {
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            return false;
        return Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories).Length > 0;
    }

    static string ReadCacheLog(string cache, string meshName)
    {
        string path = Path.Combine(cache, meshName + ".umodel.log.txt");
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    static string FindFirst(string dir, string name, params string[] exts)
    {
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            return null;
        foreach (string ext in exts)
        {
            string[] hits = Directory.GetFiles(dir, name + ext, SearchOption.AllDirectories);
            if (hits.Length > 0)
                return hits[0];
        }
        return null;
    }

    static IEnumerable<string> SafeGetFiles(string dir, string pattern)
    {
        if (!Directory.Exists(dir))
            yield break;
        string[] files;
        try { files = Directory.GetFiles(dir, pattern, SearchOption.AllDirectories); }
        catch { yield break; }
        foreach (string file in files)
            yield return file;
    }

    static void CopyIfNeeded(string src, string dest)
    {
        if (string.Equals(src, dest, StringComparison.OrdinalIgnoreCase))
            return;
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(src, dest, true);
    }

    static string Sanitize(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "unnamed";
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    static Mesh CreateFallbackQuad()
    {
        var mesh = new Mesh { name = "ViewerQuad" };
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f)
        };
        mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateNormals();
        return mesh;
    }
}
