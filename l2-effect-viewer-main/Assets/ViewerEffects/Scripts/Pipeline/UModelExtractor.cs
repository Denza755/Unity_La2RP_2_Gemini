using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

public static class UModelExtractor
{
    static string SilentWorkDir => Path.Combine(ViewerPaths.ViewerCacheDir, "umodel-work");

    public static bool IsAvailable
    {
        get
        {
            ViewerPaths.EnsureLoaded();
            return !string.IsNullOrEmpty(ViewerPaths.Config.umodelExe)
                   && File.Exists(ViewerPaths.Config.umodelExe);
        }
    }

    public static string ExtractObject(string packagePath, string objectName, string classHint, out string log)
    {
        log = "";
        ViewerPaths.EnsureLoaded();
        if (!IsAvailable || string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath))
            return null;

        string outDir = Path.Combine(
            ViewerPaths.ExtractDir,
            Path.GetFileNameWithoutExtension(packagePath),
            Sanitize(objectName));
        Directory.CreateDirectory(outDir);
        EnsureSilentConfig();

        var classes = BuildClassOrder(classHint);
        var combined = new StringBuilder();
        foreach (string cls in classes)
        {
            int code = Run(packagePath, objectName, cls, outDir, combined);
            string found = FindExported(outDir, objectName);
            if (found != null)
            {
                log = combined.ToString();
                File.WriteAllText(Path.Combine(outDir, objectName + ".umodel.log.txt"), log);
                return outDir;
            }
            combined.AppendLine($"// class={cls} exit={code}");
        }

        log = combined.ToString();
        return Directory.Exists(outDir) ? outDir : null;
    }

    static List<string> BuildClassOrder(string classHint)
    {
        var list = new List<string>();
        void add(string c)
        {
            if (!string.IsNullOrEmpty(c) && !list.Contains(c))
                list.Add(c);
        }
        add(classHint);
        if (string.Equals(classHint, "Texture", StringComparison.OrdinalIgnoreCase))
            add("Texture");
        else
        {
            add("StaticMesh");
            add("VertMesh");
            add("SkeletalMesh");
        }
        add(null);
        return list;
    }

    static int Run(string packagePath, string objectName, string classHint, string outDir, StringBuilder log)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ViewerPaths.Config.umodelExe,
            WorkingDirectory = SilentWorkDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        psi.ArgumentList.Add("-export");
        psi.ArgumentList.Add("-png");
        psi.ArgumentList.Add("-psk");
        psi.ArgumentList.Add("-game=l2");
        // umodel keeps only the last -path. Mesh .usx/.ukx is passed by full path;
        // texture dir must be the search path so StaticMesh materials resolve (fx_m_t*).
        string searchPath = UModelSearchPath(packagePath);
        if (!string.IsNullOrEmpty(searchPath))
            psi.ArgumentList.Add("-path=" + searchPath);
        psi.ArgumentList.Add("-out=" + outDir);
        psi.ArgumentList.Add(packagePath);
        psi.ArgumentList.Add(objectName);
        if (!string.IsNullOrEmpty(classHint))
            psi.ArgumentList.Add(classHint);

        using var process = Process.Start(psi);
        if (process == null)
        {
            log.AppendLine("failed to start umodel");
            return -1;
        }

        log.Append(process.StandardOutput.ReadToEnd());
        log.Append(process.StandardError.ReadToEnd());
        if (!process.WaitForExit(180000))
        {
            try { process.Kill(); } catch { }
            log.AppendLine("umodel timed out");
            return -2;
        }
        return process.ExitCode;
    }

    static string UModelSearchPath(string packagePath)
    {
        ViewerPaths.EnsureLoaded();
        string texDir = ViewerPaths.Config.texturePackageDir;
        if (!string.IsNullOrEmpty(texDir) && Directory.Exists(texDir))
            return texDir;
        string packageDir = Path.GetDirectoryName(packagePath);
        return Directory.Exists(packageDir) ? packageDir : "";
    }

    static string FindExported(string dir, string objectName)
    {
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            return null;
        string[] exts = { ".png", ".tga", ".psk", ".pskx" };
        foreach (string ext in exts)
        {
            string[] hits = Directory.GetFiles(dir, objectName + ext, SearchOption.AllDirectories);
            if (hits.Length > 0)
                return hits[0];
        }
        return null;
    }

    static void EnsureSilentConfig()
    {
        Directory.CreateDirectory(SilentWorkDir);
        string cfg = Path.Combine(SilentWorkDir, "umodel.cfg");
        if (File.Exists(cfg))
            return;
        File.WriteAllText(cfg,
            "Export =\n{\n    ExportDdsTexture = false\n    TextureFormat = 2\n    DontOverwriteFiles = false\n}\n" +
            "bShowExportOptions = false\n" +
            "bShowSaveOptions = false\n");
    }

    static string Sanitize(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
