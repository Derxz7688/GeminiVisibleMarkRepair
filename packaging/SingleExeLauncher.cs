using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

internal static class SingleExeLauncher
{
    private const string Marker = "GVMR_PAYLOAD_V1";
    private const string ProductDir = "GeminiVisibleMarkRepair";

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            string appDir = ExtractPayload();
            if (HasArg(args, "--extract-only")) return 0;

            string appExe = Path.Combine(appDir, "GeminiVisibleMarkRepair.exe");
            if (!File.Exists(appExe)) throw new FileNotFoundException("Inner application is missing.", appExe);

            string selfPath = Assembly.GetExecutingAssembly().Location;
            string launcherDir = Path.GetDirectoryName(selfPath);
            if (string.IsNullOrEmpty(launcherDir)) launcherDir = Environment.CurrentDirectory;

            var startInfo = new ProcessStartInfo(appExe);
            startInfo.WorkingDirectory = appDir;
            startInfo.UseShellExecute = false;
            startInfo.EnvironmentVariables["GVMR_LAUNCHER_EXE"] = selfPath;
            startInfo.EnvironmentVariables["GVMR_OUTPUT_DIR"] = Path.Combine(launcherDir, "\u8f93\u51fa");
            Process.Start(startInfo);
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "\u542f\u52a8\u5931\u8d25\uff1a" + ex.Message,
                "\u0047\u0065\u006d\u0069\u006e\u0069\u0020\u53ef\u89c1\u89d2\u6807\u4fee\u590d\u5de5\u5177",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return 1;
        }
    }

    private static bool HasArg(string[] args, string value)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], value, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static string ExtractPayload()
    {
        string self = Assembly.GetExecutingAssembly().Location;
        string tempZip = Path.Combine(Path.GetTempPath(), "gvmr-payload-" + Process.GetCurrentProcess().Id + ".zip");
        string hash;
        CopyPayloadToTempZip(self, tempZip, out hash);

        string prefix = hash.Substring(0, 12).ToLowerInvariant();
        string baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProductDir,
            "bundle-" + prefix);
        string markerFile = Path.Combine(baseDir, ".payload-sha256");
        string appExe = Path.Combine(baseDir, "GeminiVisibleMarkRepair.exe");

        if (File.Exists(appExe) && File.Exists(markerFile))
        {
            string existing = File.ReadAllText(markerFile).Trim();
            if (string.Equals(existing, hash, StringComparison.OrdinalIgnoreCase))
            {
                TryDeleteFile(tempZip);
                return baseDir;
            }
        }

        string tempDir = baseDir + ".tmp-" + Process.GetCurrentProcess().Id;
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);
        ZipFile.ExtractToDirectory(tempZip, tempDir);
        File.WriteAllText(Path.Combine(tempDir, ".payload-sha256"), hash, Encoding.ASCII);

        if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
        Directory.Move(tempDir, baseDir);
        TryDeleteFile(tempZip);
        return baseDir;
    }

    private static void CopyPayloadToTempZip(string selfPath, string tempZip, out string hash)
    {
        byte[] marker = Encoding.ASCII.GetBytes(Marker);
        int footerLength = marker.Length + 8;
        using (var input = new FileStream(selfPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            if (input.Length < footerLength) throw new InvalidDataException("The single EXE payload is missing.");

            byte[] markerRead = new byte[marker.Length];
            input.Seek(input.Length - marker.Length, SeekOrigin.Begin);
            ReadExactly(input, markerRead, markerRead.Length);
            for (int i = 0; i < marker.Length; i++)
            {
                if (markerRead[i] != marker[i]) throw new InvalidDataException("The single EXE payload marker is invalid.");
            }

            byte[] lengthBytes = new byte[8];
            input.Seek(input.Length - footerLength, SeekOrigin.Begin);
            ReadExactly(input, lengthBytes, lengthBytes.Length);
            long payloadLength = BitConverter.ToInt64(lengthBytes, 0);
            if (payloadLength <= 0 || payloadLength > input.Length - footerLength)
            {
                throw new InvalidDataException("The single EXE payload length is invalid.");
            }

            long payloadStart = input.Length - footerLength - payloadLength;
            input.Seek(payloadStart, SeekOrigin.Begin);

            using (var output = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var sha = SHA256.Create())
            {
                byte[] buffer = new byte[1024 * 1024];
                long remaining = payloadLength;
                while (remaining > 0)
                {
                    int wanted = remaining > buffer.Length ? buffer.Length : (int)remaining;
                    int read = input.Read(buffer, 0, wanted);
                    if (read <= 0) throw new EndOfStreamException("Unexpected end of payload.");
                    sha.TransformBlock(buffer, 0, read, null, 0);
                    output.Write(buffer, 0, read);
                    remaining -= read;
                }
                sha.TransformFinalBlock(new byte[0], 0, 0);
                hash = ToHex(sha.Hash);
            }
        }
    }

    private static void ReadExactly(Stream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = stream.Read(buffer, offset, count - offset);
            if (read <= 0) throw new EndOfStreamException();
            offset += read;
        }
    }

    private static string ToHex(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
        return sb.ToString();
    }

    private static void TryDeleteFile(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
