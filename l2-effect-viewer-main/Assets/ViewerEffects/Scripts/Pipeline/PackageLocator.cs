using System;
using System.Collections.Generic;
using System.IO;

public readonly struct UnrealAssetRef
{
    public readonly string Package;
    public readonly string Group;
    public readonly string Name;

    public UnrealAssetRef(string package, string group, string name)
    {
        Package = package ?? "";
        Group = group ?? "";
        Name = name ?? "";
    }

    public static bool TryParse(string raw, out UnrealAssetRef asset)
    {
        asset = default;
        if (string.IsNullOrWhiteSpace(raw))
            return false;
        string[] parts = raw.Split('.');
        if (parts.Length == 1)
        {
            asset = new UnrealAssetRef("", "", parts[0]);
            return true;
        }
        if (parts.Length == 2)
        {
            asset = new UnrealAssetRef(parts[0], "", parts[1]);
            return true;
        }
        asset = new UnrealAssetRef(parts[0], parts[1], parts[parts.Length - 1]);
        return true;
    }
}

public static class PackageLocator
{
    static readonly string[] MeshExt = { ".usx", ".ukx", ".u" };
    static readonly string[] TexExt = { ".utx", ".u" };

    public static string FindPackageFile(string packageName, bool texture)
    {
        foreach (string file in FindPackageFiles(packageName, texture))
            return file;
        return null;
    }

    public static List<string> FindPackageFiles(string packageName, bool texture)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(packageName))
            return result;

        string[] exts = texture ? TexExt : MeshExt;
        foreach (string dir in ViewerPaths.PackageSearchDirs())
        {
            foreach (string name in PackageNameCandidates(packageName))
            {
                foreach (string ext in exts)
                {
                    foreach (string file in SafeGetFiles(dir, name + ext))
                        AddUnique(result, file);
                    foreach (string file in SafeGetFiles(dir, name + "*" + ext))
                        AddUnique(result, file);
                }
            }
        }
        return result;
    }

    static IEnumerable<string> PackageNameCandidates(string packageName)
    {
        yield return packageName;
        if (packageName.Equals("LineageEffectsStaticmeshes", StringComparison.OrdinalIgnoreCase))
        {
            yield return "LineageEffectsStaticmeshes2";
            yield return "LineageEffectsStaticmeshes3";
            yield return "LineageItemStaticMeshs";
        }
        if (packageName.Equals("LineageEffectMeshes", StringComparison.OrdinalIgnoreCase))
        {
            yield return "LineageEffectMeshes2";
            yield return "LineageEffectMeshes3";
        }
    }

    static void AddUnique(List<string> list, string file)
    {
        foreach (string existing in list)
        {
            if (string.Equals(existing, file, StringComparison.OrdinalIgnoreCase))
                return;
        }
        list.Add(file);
    }

    public static string FindUnpackedMesh(string objectName)
    {
        return FindUnpacked(objectName, new[] { ".psk", ".pskx" });
    }

    public static string FindUnpackedTexture(string objectName)
    {
        string png = FindUnpacked(objectName, new[] { ".png", ".tga" });
        if (png != null)
            return png;
        return FindUnpacked(objectName + "_sp", new[] { ".png" })
               ?? FindUnpacked(objectName + "_ori", new[] { ".png" });
    }

    static string FindUnpacked(string objectName, string[] exts)
    {
        string root = ViewerPaths.Config.unpackFallbackDir;
        if (string.IsNullOrEmpty(objectName) || !Directory.Exists(root))
            return null;

        foreach (string ext in exts)
        {
            string[] hits = Directory.GetFiles(root, objectName + ext, SearchOption.AllDirectories);
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
        try
        {
            files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
        }
        catch
        {
            yield break;
        }
        foreach (string file in files)
            yield return file;
    }
}
