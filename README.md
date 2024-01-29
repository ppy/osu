<p align="center">
  <img width="500" alt="osu! logo" src="assets/lazer.png">
</p>

# osu!

[![Build status](https://github.com/ppy/osu/actions/workflows/ci.yml/badge.svg?branch=master&event=push)](https://github.com/ppy/osu/actions/workflows/ci.yml)
[![GitHub release](https://img.shields.io/github/release/ppy/osu.svg)](https://github.com/ppy/osu/releases/latest)
[![CodeFactor](https://www.codefactor.io/repository/github/ppy/osu/badge)](https://www.codefactor.io/repository/github/ppy/osu)
[![dev chat](https://discordapp.com/api/guilds/188630481301012481/widget.png?style=shield)](https://discord.gg/ppy)
[![Crowdin](https://d322cqt584bo4o.cloudfront.net/osu-web/localized.svg)](https://crowdin.com/project/osu-web)

A free-to-win rhythm game. Rhythm is just a *click* away!

This is the future – and final – iteration of the [osu!](https://osu.ppy.sh) game client which marks the beginning of an open era! Currently known by and released under the release codename "*lazer*". As in sharper than cutting-edge.

## Status

This project is under constant development, but we do our best to keep things in a stable state. Players are encouraged to install from a release alongside their stable *osu!* client. This project will continue to evolve until we eventually reach the point where most users prefer it over the previous "osu!stable" release.

A few resources are available as starting points to getting involved and understanding the project:

- Detailed release changelogs are available on the [official osu! site](https://osu.ppy.sh/home/changelog/lazer).
- You can learn more about our approach to [project management](https://github.com/ppy/osu/wiki/Project-management).
- Track our current efforts [towards improving the game](https://github.com/orgs/ppy/projects/7/views/6).

## Running osu!

If you are just looking to give the game a whirl, you can grab the latest release for your platform:

### Latest release:

| [Windows 8.1+ (x64)](https://github.com/ppy/osu/releases/latest/download/install.exe) | macOS 10.15+ ([Intel](https://github.com/ppy/osu/releases/latest/download/osu.app.Intel.zip), [Apple Silicon](https://github.com/ppy/osu/releases/latest/download/osu.app.Apple.Silicon.zip)) | [Linux (x64)](https://github.com/ppy/osu/releases/latest/download/osu.AppImage) | [iOS 13.4+](https://osu.ppy.sh/home/testflight) | [Android 5+](https://github.com/ppy/osu/releases/latest/download/sh.ppy.osulazer.apk) |
| ------------- | ------------- | ------------- | ------------- | ------------- |

You can also generally download a version for your current device from the [osu! site](https://osu.ppy.sh/home/download).

If your platform is not listed above, there is still a chance you can manually build it by following the instructions below.

**For iOS/iPadOS users**: The iOS testflight link fills up very fast (Apple has a hard limit of 10,000 users). We reset it occasionally. Please do not ask about this. Check back regularly for link resets or follow [peppy](https://twitter.com/ppy) on twitter for announcements. Our goal is to get the game on mobile app stores in early 2024.

## Developing a custom ruleset

osu! is designed to allow user-created gameplay variations, called "rulesets". Building one of these allows a developer to harness the power of the osu! beatmap library, game engine, and general UX for a new style of gameplay. To get started working on a ruleset, we have some templates available [here](https://github.com/ppy/osu/tree/master/Templates).

You can see some examples of custom rulesets by visiting the [custom ruleset directory](https://github.com/ppy/osu/discussions/13096).

## Developing osu!

### Prerequisites

Please make sure you have the following prerequisites:

- A desktop platform with the [.NET 6.0 SDK](https://dotnet.microsoft.com/download) installed.

When working with the codebase, we recommend using an IDE with intelligent code completion and syntax highlighting, such as the latest version of [Visual Studio](https://visualstudio.microsoft.com/vs/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [Visual Studio Code](https://code.visualstudio.com/) with the [EditorConfig](https://marketplace.visualstudio.com/items?itemName=EditorConfig.EditorConfig) and [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) plugin installed.

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

#### From an IDE

You should load the solution via one of the platform-specific `.slnf` files, rather than the main `.sln`. This will reduce dependencies and hide platforms that you don't care about. Valid `.slnf` files are:

- `osu.Desktop.slnf` (most common)
- `osu.Android.slnf`
- `osu.iOS.slnf`

Run configurations for the recommended IDEs (listed above) are included. You should use the provided Build/Run functionality of your IDE to get things going. When testing or building new components, it's highly encouraged you use the `osu! (Tests)` project/configuration. More information on this is provided [below](#contributing).

To build for mobile platforms, you will likely need to run `sudo dotnet workload restore` if you haven't done so previously. This will install Android/iOS tooling required to complete the build.

#### From CLI

You can also build and run *osu!* from the command-line with a single command:

```shell
dotnet run --project osu.Desktop
```

When running locally to do any kind of performance testing, make sure to add `-c Release` to the build command, as the overhead of running with the default `Debug` configuration can be large (especially when testing with local framework modifications as below).

If the build fails, try to restore NuGet packages with `dotnet restore`.

### Testing with resource/framework modifications

Sometimes it may be necessary to cross-test changes in [osu-resources](https://github.com/ppy/osu-resources) or [osu-framework](https://github.com/ppy/osu-framework). This can be quickly achieved using included commands:

Windows:

```ps
UseLocalFramework.ps1
UseLocalResources.ps1
```

macOS / Linux:

```ps
UseLocalFramework.sh
UseLocalResources.sh
```

Note that these commands assume you have the relevant project(s) checked out in adjacent directories:

```
|- osu            // this repository
|- osu-framework
|- osu-resources
```

### Code analysis

Before committing your code, please run a code formatter. This can be achieved by running `dotnet format` in the command line, or using the `Format code` command in your IDE.

We have adopted some cross-platform, compiler integrated analyzers. They can provide warnings when you are editing, building inside IDE or from command line, as-if they are provided by the compiler itself.

JetBrains ReSharper InspectCode is also used for wider rule sets. You can run it from PowerShell with `.\InspectCode.ps1`. Alternatively, you can install ReSharper or use Rider to get inline support in your IDE of choice.

## Contributing

When it comes to contributing to the project, the two main things you can do to help out are reporting issues and submitting pull requests. Please refer to the [contributing guidelines](CONTRIBUTING.md) to understand how to help in the most effective way possible.

If you wish to help with localisation efforts, head over to [crowdin](https://crowdin.com/project/osu-web).

We love to reward quality contributions. If you have made a large contribution, or are a regular contributor, you are welcome to [submit an expense via opencollective](https://opencollective.com/ppy/expenses/new). If you have any questions, feel free to [reach out to peppy](mailto:pe@ppy.sh) before doing so.

## Licence

*osu!*'s code and framework are licensed under the [MIT licence](https://opensource.org/licenses/MIT). Please see [the licence file](LICENCE) for more information. [tl;dr](https://tldrlegal.com/license/mit-license) you can do whatever you want as long as you include the original copyright and license notice in any copy of the software/source.

Please note that this *does not cover* the usage of the "osu!" or "ppy" branding in any software, resources, advertising or promotion, as this is protected by trademark law.

Please also note that game resources are covered by a separate licence. Please see the [ppy/osu-resources](https://github.com/ppy/osu-resources) repository for clarifications.
