using Xunit;
using DotnetTofu;

namespace DotnetTofu.Tests;

public class ParseArgumentsTests
{
    [Fact]
    public void NoArgs_ReturnsDefaultVersionAndEmptyPassArgs()
    {
        var (version, passArgs) = TofuRunner.ParseArguments([]);

        Assert.Equal(TofuRunner.DefaultTofuVersion, version);
        Assert.Empty(passArgs);
    }

    [Fact]
    public void PassThroughArgs_ArePreserved()
    {
        var (version, passArgs) = TofuRunner.ParseArguments(["init", "-backend=false"]);

        Assert.Equal(TofuRunner.DefaultTofuVersion, version);
        Assert.Equal(["init", "-backend=false"], passArgs);
    }

    [Fact]
    public void TofuVersionFlag_OverridesVersion()
    {
        var (version, passArgs) = TofuRunner.ParseArguments(["--tofu-version", "1.8.0", "plan"]);

        Assert.Equal("1.8.0", version);
        Assert.Equal(["plan"], passArgs);
    }

    [Fact]
    public void TofuVersionFlag_AtEnd_WithoutValue_IsTreatedAsPassThrough()
    {
        var (version, passArgs) = TofuRunner.ParseArguments(["plan", "--tofu-version"]);

        Assert.Equal(TofuRunner.DefaultTofuVersion, version);
        Assert.Equal(["plan", "--tofu-version"], passArgs);
    }

    [Fact]
    public void TofuVersionFlag_InMiddle_ExtractsVersionAndPreservesRest()
    {
        var (version, passArgs) = TofuRunner.ParseArguments(
            ["apply", "--tofu-version", "1.7.0", "-auto-approve"]);

        Assert.Equal("1.7.0", version);
        Assert.Equal(["apply", "-auto-approve"], passArgs);
    }
}

public class PlatformTests
{
    [Fact]
    public void GetOs_ReturnsKnownValue()
    {
        var os = TofuRunner.GetOs();
        Assert.Contains(os, new[] { "windows", "linux", "darwin" });
    }

    [Fact]
    public void GetArch_ReturnsKnownValue()
    {
        var arch = TofuRunner.GetArch();
        Assert.Contains(arch, new[] { "amd64", "arm64", "386" });
    }
}

public class BinaryNameTests
{
    [Theory]
    [InlineData("windows", "tofu.exe")]
    [InlineData("linux", "tofu")]
    [InlineData("darwin", "tofu")]
    public void GetBinaryName_ReturnsCorrectName(string os, string expected)
    {
        Assert.Equal(expected, TofuRunner.GetBinaryName(os));
    }
}

public class DownloadUrlTests
{
    [Fact]
    public void GetDownloadUrl_FormatsCorrectly()
    {
        var url = TofuRunner.GetDownloadUrl("1.9.0", "linux", "amd64");

        Assert.Equal(
            "https://github.com/opentofu/opentofu/releases/download/v1.9.0/tofu_1.9.0_linux_amd64.zip",
            url);
    }

    [Fact]
    public void GetDownloadUrl_HandlesWindowsArm64()
    {
        var url = TofuRunner.GetDownloadUrl("1.8.0", "windows", "arm64");

        Assert.Equal(
            "https://github.com/opentofu/opentofu/releases/download/v1.8.0/tofu_1.8.0_windows_arm64.zip",
            url);
    }
}

public class BinaryPathTests
{
    [Fact]
    public void GetBinaryPath_ConstructsCorrectPath()
    {
        var path = TofuRunner.GetBinaryPath("/projects/myapp", "1.9.0", "linux");

        Assert.Equal(
            Path.Combine("/projects/myapp", ".tofu", "1.9.0", "tofu"),
            path);
    }

    [Fact]
    public void GetBinaryPath_UsesExeOnWindows()
    {
        var path = TofuRunner.GetBinaryPath("/projects/myapp", "1.9.0", "windows");

        Assert.Equal(
            Path.Combine("/projects/myapp", ".tofu", "1.9.0", "tofu.exe"),
            path);
    }
}
