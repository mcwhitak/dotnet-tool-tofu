# dotnet-tool-tofu

Inspired by [dotnet-terraform](https://github.com/phillipsj/dotnet-terraform), this project provides a .NET tool interface for interacting with [OpenTofu](https://opentofu.org/).

## Quick Start

```
dotnet new tool-manifest
dotnet tool install dotnet-tool-tofu
dotnet tofu version
```

## Goals

This tool was created in support of single-entry repositories (repositories where all developer workflow tasks can be done with a single build tool entrypoint) in the .NET ecosystem.

Single entry repositories follow these basic principles:
1. Only a single top level tool must be used (in this case `dotnet`) for all actions
2. When actions would require another dependency on the system the single entry tool should download them dynamically if necessary and install them in a project-local way
3. CI operations should simply call basic tasks on the single top level tool

As part of this, if you are writing a monorepo that contains service code and the infrastructure deployment associated with it, you should be able to validate, format and run checks via the same workflow and tools that you use at the top of your repo.

As an example, if `dotnet build` throws warnings on stylistic errors in your .NET code, it should also run `tofu fmt -check` to validate your infrastructure.

This allows polyglot repos to be introduced more naturally and treats the workflow of the repository as a first class citizen.

## Options

By default this tool downloads OpenTofu v1.9.0. To use a different version, pass `--tofu-version`:

```
dotnet tofu --tofu-version 1.8.0 plan
```

All other arguments are forwarded directly to the OpenTofu binary.
