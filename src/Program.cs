using DotnetTofu;

var (version, passArgs) = TofuRunner.ParseArguments(args);
var os = TofuRunner.GetOs();
var arch = TofuRunner.GetArch();
var baseDir = Directory.GetCurrentDirectory();

var downloadResult = await TofuRunner.EnsureDownloaded(version, os, arch, baseDir);
if (downloadResult != 0)
    return downloadResult;

var binaryPath = TofuRunner.GetBinaryPath(baseDir, version, os);
return await TofuRunner.Execute(binaryPath, passArgs);
