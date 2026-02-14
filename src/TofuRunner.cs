using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace DotnetTofu;

public class TofuRunner
{
    public const string DefaultTofuVersion = "1.9.0";

    public static (string Version, List<string> PassArgs) ParseArguments(string[] args)
    {
        var version = DefaultTofuVersion;
        var passArgs = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--tofu-version" && i + 1 < args.Length)
            {
                version = args[++i];
            }
            else
            {
                passArgs.Add(args[i]);
            }
        }

        return (version, passArgs);
    }

    public static string GetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "darwin";
        return "linux";
    }

    public static string GetArch()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "amd64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "386",
            _ => throw new PlatformNotSupportedException(
                $"Unsupported architecture: {RuntimeInformation.OSArchitecture}")
        };
    }

    public static string GetBinaryName(string os) =>
        os == "windows" ? "tofu.exe" : "tofu";

    public static string GetDownloadUrl(string version, string os, string arch) =>
        $"https://github.com/opentofu/opentofu/releases/download/v{version}/tofu_{version}_{os}_{arch}.zip";

    public static string GetBinaryPath(string baseDir, string version, string os) =>
        Path.Combine(baseDir, ".tofu", version, GetBinaryName(os));

    public static async Task<int> EnsureDownloaded(string version, string os, string arch, string baseDir)
    {
        var tofuDir = Path.Combine(baseDir, ".tofu", version);
        var binaryPath = Path.Combine(tofuDir, GetBinaryName(os));

        if (File.Exists(binaryPath))
            return 0;

        Directory.CreateDirectory(tofuDir);

        var zipFileName = $"tofu_{version}_{os}_{arch}.zip";
        var downloadUrl = GetDownloadUrl(version, os, arch);

        Console.Error.WriteLine($"Downloading OpenTofu {version} for {os}/{arch}...");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("dotnet-tool-tofu/0.1.0");

        using var response = await httpClient.GetAsync(downloadUrl);
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"Failed to download OpenTofu: HTTP {(int)response.StatusCode}");
            Console.Error.WriteLine($"URL: {downloadUrl}");
            return 1;
        }

        var tempZip = Path.Combine(Path.GetTempPath(), zipFileName);
        try
        {
            using (var fs = File.Create(tempZip))
            {
                await response.Content.CopyToAsync(fs);
            }

            ZipFile.ExtractToDirectory(tempZip, tofuDir, overwriteFiles: true);
        }
        finally
        {
            if (File.Exists(tempZip))
                File.Delete(tempZip);
        }

        if (os != "windows")
        {
            var chmod = Process.Start("chmod", ["+x", binaryPath]);
            chmod?.WaitForExit();
        }

        Console.Error.WriteLine($"OpenTofu {version} installed to {tofuDir}");
        return 0;
    }

    public static async Task<int> Execute(string binaryPath, List<string> passArgs)
    {
        var psi = new ProcessStartInfo
        {
            FileName = binaryPath,
            UseShellExecute = false,
        };

        foreach (var arg in passArgs)
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = Process.Start(psi);
        if (process is null)
        {
            Console.Error.WriteLine("Failed to start OpenTofu process.");
            return 1;
        }

        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}
