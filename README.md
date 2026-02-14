# dotnet-tool-tofu

This repo contains a .NET tool that downloads and serves as an entrypoint for running opentofu commands.


You can simply run `dotnet tool tofu` and then follow it up with any normal opentofu commands.


When running the tool the tool version implies a version of opentofu but you may also provide a version


When run the tool downloads the correct opentofu binary for your platform/arch to a project isolated directory if opentofu is not found already in that directory
