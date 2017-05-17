# SteamKit2

[![Build Status (Linux)](https://img.shields.io/travis/SteamRE/SteamKit/master.svg?style=flat-square&label=Linux)](https://travis-ci.org/SteamRE/SteamKit)
[![Build Status (Windows)](https://img.shields.io/appveyor/ci/SteamRE/SteamKit/master.svg?style=flat-square&label=Windows)](https://ci.appveyor.com/project/SteamRE/SteamKit)
[![NuGet](https://img.shields.io/nuget/v/SteamKit2.svg?style=flat-square)](https://www.nuget.org/packages/SteamKit2/)
[![Code Coverage](https://img.shields.io/badge/Code-Coverage-007ec6.svg?style=flat-square)](https://codecov.io/github/SteamRE/SteamKit)


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

  - .NET 4.6, .NET Core 1.0, or [Mono ≥4.8](http://mono-project.com)

To compile SteamKit2, the following is required:

  - C# 7.0 compiler &mdash; [Visual Studio 2017](https://www.visualstudio.com/vs/whatsnew/) or [.NET Core 1.1](https://www.microsoft.com/net/core) or [Mono ≥5.0](http://mono-project.com)
  - [protobuf-net](http://code.google.com/p/protobuf-net/) ([NuGet package](http://nuget.org/packages/protobuf-net))

Note: If you're using the NuGet package, the protobuf-net dependency _should_ be resolved for you. See the [Installation Guide](https://github.com/SteamRE/SteamKit/wiki/Installation) for more information.


## Contact

IRC: [irc.gamesurge.net / #opensteamworks](irc://irc.gamesurge.net/opensteamworks)

