# SteamKit2
[![Build Status (CI/CD)](https://github.com/SteamRE/SteamKit/workflows/CI/CD/badge.svg?branch=master&event=push)](https://github.com/SteamRE/SteamKit/actions?query=workflow%3ACI%2FCD)
[![NuGet](https://img.shields.io/nuget/v/SteamKit2.svg)](https://www.nuget.org/packages/SteamKit2/)


SteamKit2 is a .NET library designed to interoperate with Valve's [Steam network](http://store.steampowered.com/about). It aims to provide a simple, yet extensible, interface to perform various actions on the network.


## Getting Binaries


### Visual Studio

Starting with version 1.2.2, SteamKit2 is distributed as a [NuGet package](http://nuget.org/packages/steamkit2).

Simply install SteamKit2 using the package manager in Visual Studio, and NuGet will add all the required dependencies and references to your project.  
  
### Other

We additionally distribute binaries on our [releases page](https://github.com/SteamRE/SteamKit/releases).

For more information on installing SteamKit2, please refer to the [Installation Guide](https://github.com/SteamRE/SteamKit/wiki/Installation) on the wiki.


## Documentation

Documentation consists primarily of XML code documentation provided with the binaries, and our [wiki](https://github.com/SteamRE/SteamKit/wiki).


## License

SteamKit2 is released under the [LGPL-2.1 license](http://www.tldrlegal.com/license/gnu-lesser-general-public-license-v2.1-%28lgpl-2.1%29).


## Dependencies

In order to use SteamKit2 at runtime, the following dependencies are required:

  - A [.NET Standard 2.0](https://github.com/dotnet/standard/blob/master/docs/versions.md)-compatible runtime. At the time of writing, this is:
      - .NET Framework 4.6.1
      - .NET Core 2.0
      - [Mono ≥5.4](http://mono-project.com)

To compile SteamKit2, the following is required:

  - [.NET 6 SDK](https://dot.net/).
      - On Windows, [Visual Studio 2022 (≥17.0)](https://www.visualstudio.com/vs/whatsnew/) is optional but recommended.
      - On macOS, [Visual Studio 2022 for Mac](https://www.visualstudio.com/vs/visual-studio-mac/) may be helpful.
  - [protobuf-net](http://code.google.com/p/protobuf-net/) ([NuGet package](http://nuget.org/packages/protobuf-net))

Note: If you're using the NuGet package, the protobuf-net dependency _should_ be resolved for you. See the [Installation Guide](https://github.com/SteamRE/SteamKit/wiki/Installation) for more information.


## Contact

IRC: irc.libera.chat / #steamre ([join via webchat](https://web.libera.chat/#steamre))

