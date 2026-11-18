using System;
using System.IO;
using System.Text;

public static class L2PackageDecryptor
{
    public static string DecryptToCache(string inputPath)
    {
        if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
            throw new FileNotFoundException("Package not found", inputPath);

        string dest = Path.Combine(ViewerPaths.DecryptedDir, Path.GetFileName(inputPath));
        if (File.Exists(dest) && File.GetLastWriteTimeUtc(dest) >= File.GetLastWriteTimeUtc(inputPath))
            return dest;

        byte[] bytes = File.ReadAllBytes(inputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        if (bytes.Length < 28)
        {
            File.WriteAllBytes(dest, bytes);
            return dest;
        }

        string header = Encoding.Unicode.GetString(bytes, 0, 28);
        if (header.Length < 14 || !header.StartsWith("Lineage2Ver", StringComparison.Ordinal)
            || !int.TryParse(header.AsSpan(11, 3), out int version))
        {
            File.Copy(inputPath, dest, true);
            return dest;
        }

        int key = version switch
        {
            111 => 0xAC,
            121 => XorKey121(Path.GetFileName(inputPath)),
            _ => -1
        };
        if (key < 0)
            throw new NotSupportedException("Lineage2Ver" + version + " needs Blowfish/RSA (Java engine).");

        byte[] body = new byte[bytes.Length - 28];
        for (int i = 0; i < body.Length; i++)
            body[i] = (byte)(bytes[i + 28] ^ key);
        File.WriteAllBytes(dest, body);
        return dest;
    }

    static int XorKey121(string fileName)
    {
        int sum = 0;
        foreach (char c in fileName.ToLowerInvariant())
            sum += c;
        return sum & 0xFF;
    }
}
