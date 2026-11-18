using System;
using System.Diagnostics;
using System.IO;
using System.Text;

public static class PskToFbxConverter
{
    public static string Convert(string pskPath, string destFbx, out string log)
    {
        log = "";
        ViewerPaths.EnsureLoaded();
        string blender = ViewerPaths.Config.blenderExe;
        if (string.IsNullOrEmpty(blender) || !File.Exists(blender))
            throw new FileNotFoundException("Blender not found", blender);
        if (!File.Exists(ViewerPaths.PskAddonPath))
            throw new FileNotFoundException("PSK addon not found", ViewerPaths.PskAddonPath);

        Directory.CreateDirectory(Path.GetDirectoryName(destFbx)!);
        var psi = new ProcessStartInfo
        {
            FileName = blender,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        psi.ArgumentList.Add("-b");
        psi.ArgumentList.Add("-P");
        psi.ArgumentList.Add(ViewerPaths.PskToFbxScript);
        psi.ArgumentList.Add("--");
        psi.ArgumentList.Add(pskPath);
        psi.ArgumentList.Add(destFbx);
        psi.ArgumentList.Add(ViewerPaths.PskAddonPath);

        var output = new StringBuilder();
        using (var process = Process.Start(psi))
        {
            output.Append(process.StandardOutput.ReadToEnd());
            output.Append(process.StandardError.ReadToEnd());
            if (!process.WaitForExit(180000))
            {
                try { process.Kill(); } catch { }
                throw new TimeoutException("Blender PSK convert timed out: " + pskPath);
            }
            log = output.ToString();
            if (process.ExitCode != 0 || !File.Exists(destFbx))
                throw new InvalidOperationException("Blender convert failed:\n" + log);
        }

        return destFbx;
    }
}
