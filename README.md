# dotnet-tool-tofu
[![NuGet version (dotnet-tool-tofu)](https://img.shields.io/nuget/v/dotnet-tool-tofu?style=flat-square)](https://www.nuget.org/packages/dotnet-tool-tofu/)

Inspired by [dotnet-terraform](https://github.com/phillipsj/dotnet-terraform), this project provides a .NET tool interface for interacting with [OpenTofu](https://opentofu.org/).

## Quick Start

```
dotnet new tool-manifest
dotnet tool install dotnet-tool-tofu
dotnet tofu version
```

## Goals

This tool was created in support of single-entry repositories (repositories where all developer workflow tasks can be done with a single build tool entrypoint) in the .NET ecosystem.

While MSBuild is not flexible enough to create custom solution level tasks, being able to manage the version at the project level as a dependency and not have devs manage the tool themselves is a bonus.

## Options

By default this tool downloads OpenTofu v1.9.0. To use a different version, pass `--tofu-version`:

```
dotnet tofu --tofu-version 1.8.0 plan
```

All other arguments are forwarded directly to the OpenTofu binary.
