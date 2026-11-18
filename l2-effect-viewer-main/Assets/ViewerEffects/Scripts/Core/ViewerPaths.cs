using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ViewerConfigData
{
    public string ucEffectsDir;
    public string meshPackageDir;
    public string texturePackageDir;
    public string unpackFallbackDir;
    public string[] extraPackageDirs;
    public string blenderExe;
    public string umodelExe;
    public float worldCalibrationMesh = 1.8f;
    // Same split as l2-unity L2FxCoreGeometryTest usage: mesh K=1.8, sprite K=1.1.
    // UC DrawScale is ignored (not applied to transforms).
    public float worldCalibrationSprite = 1.1f;
    // BeamEmitter has separate verified geometry; do not include it in the sprite K test.
    public float worldCalibrationBeam = 1.4f;
}

public static class ViewerPaths
{
    public static ViewerConfigData Config { get; private set; }

    public static string ProjectRoot => Directory.GetParent(Application.dataPath)!.FullName;
    public static string ToolsDir => Path.Combine(ProjectRoot, "Tools");
    public static string UModelDir => Path.Combine(ToolsDir, "UModel");
    public static string BlenderToolsDir => Path.Combine(ToolsDir, "Blender");
    public static string ViewerCacheDir => Path.Combine(ProjectRoot, "ViewerCache");
    public static string DecryptedDir => Path.Combine(ViewerCacheDir, "decrypted");
    public static string ExtractDir => Path.Combine(ViewerCacheDir, "extract");
    public static string AssetsCacheDir => Path.Combine(Application.dataPath, "ViewerEffects", "Cache");
    public static string PskAddonPath => Path.Combine(BlenderToolsDir, "io_import_scene_unreal_psa_psk_280.py");
    public static string PskToFbxScript => Path.Combine(BlenderToolsDir, "psk_to_fbx.py");
    public static string ViewerDataDir => Path.Combine(Application.streamingAssetsPath, "ViewerData");

    public static void EnsureLoaded()
    {
        if (Config != null)
            return;

        string jsonPath = Path.Combine(Application.streamingAssetsPath, "ViewerConfig.json");
        if (File.Exists(jsonPath))
            Config = JsonUtility.FromJson<ViewerConfigData>(File.ReadAllText(jsonPath));
        Config ??= new ViewerConfigData();

        Config.ucEffectsDir = FirstExistingDir(
            Config.ucEffectsDir,
            Path.Combine(Application.streamingAssetsPath, "UcEffects", "Classes"));
        Config.meshPackageDir = FirstExistingDir(
            Config.meshPackageDir,
            Path.Combine(ViewerDataDir, "Meshes"));
        Config.texturePackageDir = FirstExistingDir(
            Config.texturePackageDir,
            Path.Combine(ViewerDataDir, "Textures"));
        Config.unpackFallbackDir = FirstExistingDir(Config.unpackFallbackDir);
        Config.blenderExe = FirstExistingFile(Config.blenderExe);
        Config.umodelExe = FirstExistingFile(
            Config.umodelExe,
            Path.Combine(UModelDir, "umodel_64.exe"),
            Path.Combine(UModelDir, "umodel.exe"));

        if (Config.extraPackageDirs != null)
        {
            for (int i = 0; i < Config.extraPackageDirs.Length; i++)
                Config.extraPackageDirs[i] = ResolvePath(Config.extraPackageDirs[i]);
        }

        Directory.CreateDirectory(ViewerCacheDir);
        Directory.CreateDirectory(DecryptedDir);
        Directory.CreateDirectory(ExtractDir);
        Directory.CreateDirectory(AssetsCacheDir);
    }

    public static IEnumerable<string> PackageSearchDirs()
    {
        EnsureLoaded();
        if (Directory.Exists(Config.meshPackageDir))
            yield return Config.meshPackageDir;
        if (Directory.Exists(Config.texturePackageDir))
            yield return Config.texturePackageDir;
        if (Config.extraPackageDirs != null)
        {
            foreach (string dir in Config.extraPackageDirs)
            {
                if (Directory.Exists(dir))
                    yield return dir;
            }
        }
    }

    static string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "";
        if (Path.IsPathRooted(path))
            return path;
        return Path.GetFullPath(Path.Combine(ProjectRoot, path.Replace('/', Path.DirectorySeparatorChar)));
    }

    static string FirstExistingDir(params string[] paths)
    {
        foreach (string path in paths)
        {
            string resolved = ResolvePath(path);
            if (!string.IsNullOrWhiteSpace(resolved) && Directory.Exists(resolved))
                return resolved;
        }
        return ResolvePath(paths.Length > 0 ? paths[0] : "");
    }

    static string FirstExistingFile(params string[] paths)
    {
        foreach (string path in paths)
        {
            string resolved = ResolvePath(path);
            if (!string.IsNullOrWhiteSpace(resolved) && File.Exists(resolved))
                return resolved;
        }
        return "";
    }
}
