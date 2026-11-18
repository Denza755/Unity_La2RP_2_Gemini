using System;
using System.Collections.Generic;
using System.IO;

public static class UcEffectCatalog
{
    public static readonly List<string> Names = new List<string>();
    static readonly Dictionary<string, string> Paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static void Refresh()
    {
        Names.Clear();
        Paths.Clear();
        ViewerPaths.EnsureLoaded();
        string dir = ViewerPaths.Config.ucEffectsDir;
        if (!Directory.Exists(dir))
            return;

        foreach (string file in Directory.GetFiles(dir, "*.uc"))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (string.IsNullOrEmpty(name))
                continue;
            Names.Add(name);
            Paths[name] = file;
        }
        Names.Sort(StringComparer.OrdinalIgnoreCase);
    }

    public static bool TryGetPath(string name, out string path)
    {
        return Paths.TryGetValue(name, out path);
    }
}
