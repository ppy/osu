<p align="center">
  <img width="500px" src="assets/lazer.png">
</p>

# osu!

[![Build status](https://ci.appveyor.com/api/projects/status/u2p01nx7l6og8buh?svg=true)](https://ci.appveyor.com/project/peppy/osu)  [![CodeFactor](https://www.codefactor.io/repository/github/ppy/osu/badge)](https://www.codefactor.io/repository/github/ppy/osu) [![dev chat](https://discordapp.com/api/guilds/188630481301012481/widget.png?style=shield)](https://discord.gg/ppy)

Rhythm is just a *click* away. The future of [osu!](https://osu.ppy.sh) and the beginning of an open era! Commonly known by the codename "osu!lazer". Pew pew.

## Status

This project is still heavily under development, but is in a state where users are encouraged to try it out and keep it installed alongside the stable osu! client. It will continue to evolve over the coming months and hopefully bring some new unique features to the table.

We are accepting bug reports (please report with as much detail as possible). Feature requests are welcome as long as you read and understand the contribution guidelines listed below.

Detailed changelogs are published on the [official osu! site](https://osu.ppy.sh/home/changelog).

## Requirements

- A desktop platform with the [.NET Core SDK 3.0](https://www.microsoft.com/net/learn/get-started) or higher installed.
- When running on linux, please have a system-wide ffmpeg installation available to support video decoding.
- When running on Windows 7 or 8.1, **[additional prerequisites](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x)** may be required to correctly run .NET Core applications if your operating system is not up-to-date with the latest service packs.
- When working with the codebase, we recommend using an IDE with intellisense and syntax highlighting, such as [Visual Studio 2017+](https://visualstudio.microsoft.com/vs/), [Jetbrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio Code](https://code.visualstudio.com/).

## Running osu!

### Releases

If you are not interested in developing the game, you can still consume our [binary releases](https://github.com/ppy/osu/releases).

**Latest build:**

| [Windows (x64)](https://github.com/ppy/osu/releases/latest/download/install.exe)  | [macOS 10.12+](https://github.com/ppy/osu/releases/latest/download/osu.app.zip) | [iOS(iOS 10+)](https://testflight.apple.com/join/2tLcjWlF) | [Android (5+)](https://github.com/ppy/osu/releases/latest/download/sh.ppy.osulazer.apk)
| ------------- | ------------- | ------------- | ------------- |

- **Linux** users are recommended to self-compile until we have official deployment in place.

If your platform is not listed above, there is still a chance you can manually build it by following the instructions below.

### Downloading the source code

Clone the repository:

```shell
git clone https://github.com/ppy/osu
cd osu
```

To update the source code to the latest commit, run the following command inside the `osu` directory:

```shell
git pull
```

### Building

Build configurations for the recommended IDEs (listed above) are included. You should use the provided Build/Run functionality of your IDE to get things going. When testing or building new components, it's highly encouraged you use the `VisualTests` project/configuration. More information on this provided [below](#contributing).

> Visual Studio Code users must run the `Restore` task before any build attempt.

You can also build and run osu! from the command-line with a single command:

```shell
dotnet run --project osu.Desktop
```

If you are not interested in debugging osu!, you can add `-c Release` to gain performance. In this case, you must replace `Debug` with `Release` in any commands mentioned in this document.

If the build fails, try to restore nuget packages with `dotnet restore`.

#### A note for Linux users

On Linux, the environment variable `LD_LIBRARY_PATH` must point to the build directory, located at `osu.Desktop/bin/Debug/$NETCORE_VERSION`.

`$NETCORE_VERSION` is the version of the targeted .NET Core SDK. You can check it by running `grep TargetFramework osu.Desktop/osu.Desktop.csproj | sed -r 's/.*>(.*)<\/.*/\1/'`.

For example, you can run osu! with the following command:

```shell
LD_LIBRARY_PATH="$(pwd)/osu.Desktop/bin/Debug/netcoreapp3.0" dotnet run --project osu.Desktop
```

### Testing with resource/framework modifications

Sometimes it may be necessary to cross-test changes in [osu-resources](https://github.com/ppy/osu-resources) or [osu-framework](https://github.com/ppy/osu-framework). This can be achieved by running some commands as documented on the [osu-resources](https://github.com/ppy/osu-resources/wiki/Testing-local-resources-checkout-with-other-projects) and [osu-framework](https://github.com/ppy/osu-framework/wiki/Testing-local-framework-checkout-with-other-projects) wiki pages.

### Code analysis

Code analysis can be run with `powershell ./build.ps1` or `build.sh`. This is currently only supported under windows due to [resharper cli shortcomings](https://youtrack.jetbrains.com/issue/RSRP-410004). Alternatively, you can install resharper or use rider to get inline support in your IDE of choice.

## Contributing

We welcome all contributions, but keep in mind that we already have a lot of the UI designed. If you wish to work on something with the intention on having it included in the official distribution, please open an issue for discussion and we will give you what you need from a design perspective to proceed. If you want to make *changes* to the design, we recommend you open an issue with your intentions before spending too much time, to ensure no effort is wasted.

If you're unsure of what you can help with, check out the [list of open issues](https://github.com/ppy/osu/issues) (especially those with the ["good first issue"](https://github.com/ppy/osu/issues?q=is%3Aissue+is%3Aopen+sort%3Aupdated-desc+label%3A%22good+first+issue%22) label).

Before starting, please make sure you are familiar with the [development and testing](https://github.com/ppy/osu-framework/wiki/Development-and-Testing) procedure we have set up. New component development, and where possible, bug fixing and debugging existing components **should always be done under VisualTests**.

Note that while we already have certain standards in place, nothing is set in stone. If you have an issue with the way code is structured; with any libraries we are using; with any processes involved with contributing, *please* bring it up. We welcome all feedback so we can make contributing to this project as pain-free as possible.

For those interested, we love to reward quality contributions via [bounties](https://docs.google.com/spreadsheets/d/1jNXfj_S3Pb5PErA-czDdC9DUu4IgUbe1Lt8E7CYUJuE/view?&rm=minimal#gid=523803337), paid out via paypal or osu! supporter tags. Don't hesitate to [request a bounty](https://docs.google.com/forms/d/e/1FAIpQLSet_8iFAgPMG526pBZ2Kic6HSh7XPM3fE8xPcnWNkMzINDdYg/viewform) for your work on this project.

## Licence

The osu! client code and framework are licensed under the [MIT licence](https://opensource.org/licenses/MIT). Please see [the licence file](LICENCE) for more information. [tl;dr](https://tldrlegal.com/license/mit-license) you can do whatever you want as long as you include the original copyright and license notice in any copy of the software/source.

Please note that this *does not cover* the usage of the "osu!" or "ppy" branding in any software, resources, advertising or promotion, as this is protected by trademark law.

Please also note that game resources are covered by a separate licence. Please see the [ppy/osu-resources](https://github.com/ppy/osu-resources) repository for clarifications.
