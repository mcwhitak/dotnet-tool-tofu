using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

const string DefaultTofuVersion = "1.9.0";

// Parse arguments, extracting --tofu-version if present
var tofuVersion = DefaultTofuVersion;
var passArgs = new List<string>();

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--tofu-version" && i + 1 < args.Length)
    {
        tofuVersion = args[++i];
    }
    else
    {
        passArgs.Add(args[i]);
    }
}

// Determine OS
string os;
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    os = "windows";
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    os = "darwin";
else
    os = "linux";

// Determine architecture
string arch = RuntimeInformation.OSArchitecture switch
{
    Architecture.X64 => "amd64",
    Architecture.Arm64 => "arm64",
    Architecture.X86 => "386",
    _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.OSArchitecture}")
};

// Resolve binary path
string binaryName = os == "windows" ? "tofu.exe" : "tofu";
string tofuDir = Path.Combine(Directory.GetCurrentDirectory(), ".tofu", tofuVersion);
string binaryPath = Path.Combine(tofuDir, binaryName);

// Download if missing
if (!File.Exists(binaryPath))
{
    Directory.CreateDirectory(tofuDir);

    string zipFileName = $"tofu_{tofuVersion}_{os}_{arch}.zip";
    string downloadUrl = $"https://github.com/opentofu/opentofu/releases/download/v{tofuVersion}/{zipFileName}";

    Console.Error.WriteLine($"Downloading OpenTofu {tofuVersion} for {os}/{arch}...");

    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("dotnet-tool-tofu/0.1.0");

    using var response = await httpClient.GetAsync(downloadUrl);
    if (!response.IsSuccessStatusCode)
    {
        Console.Error.WriteLine($"Failed to download OpenTofu: HTTP {(int)response.StatusCode}");
        Console.Error.WriteLine($"URL: {downloadUrl}");
        return 1;
    }

    string tempZip = Path.Combine(Path.GetTempPath(), zipFileName);
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

    // Mark as executable on Unix
    if (os != "windows")
    {
        var chmod = Process.Start("chmod", ["+x", binaryPath]);
        chmod?.WaitForExit();
    }

    Console.Error.WriteLine($"OpenTofu {tofuVersion} installed to {tofuDir}");
}

// Execute tofu with forwarded arguments
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
