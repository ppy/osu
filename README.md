<p align="center">
  <img width="500" alt="osu! logo" src="assets/lazer.png">
</p>

# osu!

[![Build status](https://github.com/ppy/osu/actions/workflows/ci.yml/badge.svg?branch=master\&event=push)](https://github.com/ppy/osu/actions/workflows/ci.yml)
[![GitHub release](https://img.shields.io/github/release/ppy/osu.svg)](https://github.com/ppy/osu/releases/latest)
[![CodeFactor](https://www.codefactor.io/repository/github/ppy/osu/badge)](https://www.codefactor.io/repository/github/ppy/osu)
[![dev chat](https://discord.com/api/guilds/188630481301012481/widget.png?style=shield)](https://discord.gg/ppy)
[![Crowdin](https://d322cqt584bo4o.cloudfront.net/osu-web/localized.svg)](https://crowdin.com/project/osu-web)

A free-to-play rhythm game. Rhythm is just a *click* away!

This is the next—and final—iteration of the [osu!](https://osu.ppy.sh) client, currently released under the codename **"lazer"**. Lazer represents the evolution of osu!, sharper than ever and fully open-source.

---

## Status

*osu!* is under continuous development. We strive to maintain stability, and players are encouraged to install Lazer alongside their stable *osu!* client. Over time, Lazer aims to become the preferred client for most users.

Here are some resources to help you get started with the project:

* [Detailed release changelogs](https://osu.ppy.sh/home/changelog/lazer)
* [Project management overview](https://github.com/ppy/osu/wiki/Project-management)
* [Current development efforts](https://github.com/orgs/ppy/projects/7/views/6)

---

## Running osu!

### Latest release

You can download the latest release for your platform:

| Windows 10+ (x64)                                                           | macOS 12+                                                                                                                                                                       | Linux (x64)                                                                  | iOS 13.4+                                        | Android 5+                                                                          |
| --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- | ------------------------------------------------ | ----------------------------------------------------------------------------------- |
| [Download](https://github.com/ppy/osu/releases/latest/download/install.exe) | [Intel](https://github.com/ppy/osu/releases/latest/download/osu.app.Intel.zip) / [Apple Silicon](https://github.com/ppy/osu/releases/latest/download/osu.app.Apple.Silicon.zip) | [Download](https://github.com/ppy/osu/releases/latest/download/osu.AppImage) | [TestFlight](https://osu.ppy.sh/home/testflight) | [Download](https://github.com/ppy/osu/releases/latest/download/sh.ppy.osulazer.apk) |

You can also visit the [osu! download page](https://osu.ppy.sh/home/download) for your device.

**iOS users:** The TestFlight link is limited to 10,000 users and fills up quickly. Please check back regularly or follow [peppy on Twitter](https://twitter.com/ppy) for updates. We plan to release the game on mobile app stores soon.

---

## Developing a Custom Ruleset

osu! supports user-created gameplay variations called **rulesets**. Custom rulesets allow developers to use the osu! beatmap library, game engine, and UX for new gameplay styles.

* [Ruleset templates](https://github.com/ppy/osu/tree/master/Templates)
* [Example custom rulesets](https://github.com/ppy/osu/discussions/13096)

---

## Developing osu!

### Prerequisites

* Desktop platform with [.NET 8.0 SDK](https://dotnet.microsoft.com/download) installed
* Recommended IDEs: [Visual Studio](https://visualstudio.microsoft.com/vs/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/) with [EditorConfig](https://marketplace.visualstudio.com/items?itemName=EditorConfig.EditorConfig) and [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

### Downloading the source code

```bash
git clone https://github.com/ppy/osu
cd osu
git pull
```

### Building

#### From an IDE

Open one of the platform-specific `.slnf` files instead of the main `.sln` file. This reduces unnecessary dependencies.

Valid `.slnf` files:

* `osu.Desktop.slnf` (most common)
* `osu.Android.slnf`
* `osu.iOS.slnf`

Run configurations are included. For testing or developing new components, use the `osu! (Tests)` project/configuration.

**Mobile platforms:** If building for mobile, run:

```bash
sudo dotnet workload restore
```

This installs Android/iOS tooling required to complete the build.

#### From CLI

```bash
dotnet run --project osu.Desktop
```

For performance testing, use:

```bash
dotnet run --project osu.Desktop -c Release
```

If the build fails, restore NuGet packages:

```bash
dotnet restore
```

---

### Testing with Local Framework or Resource Changes

Sometimes you may need to test changes in [osu-resources](https://github.com/ppy/osu-resources) or [osu-framework](https://github.com/ppy/osu-framework).

**Windows (PowerShell):**

```powershell
.\UseLocalFramework.ps1
.\UseLocalResources.ps1
```

**macOS / Linux:**

```bash
./UseLocalFramework.sh
./UseLocalResources.sh
```

> ⚠️ These scripts assume you have the relevant projects checked out in adjacent directories:

```
|- osu            // this repository
|- osu-framework
|- osu-resources
```

---

### Code Analysis

Before committing, run a code formatter:

```bash
dotnet format
```

You can also use IDE commands or ReSharper InspectCode:

```powershell
.\InspectCode.ps1
```

Cross-platform compiler analyzers provide warnings during editing/building.

---

## Contributing

You can contribute by reporting issues or submitting pull requests. Please read our [contributing guidelines](CONTRIBUTING.md).

* Help with localisation via [Crowdin](https://crowdin.com/project/osu-web)
* Large or regular contributions can be rewarded via [Open Collective expenses](https://opencollective.com/ppy/expenses/new)
* Questions? Reach out to [peppy](mailto:pe@ppy.sh)

---

## License

osu!’s code and framework are licensed under the [MIT License](https://opensource.org/licenses/MIT). See the [license file](LICENCE) for details.

> TL;DR: You can do almost anything with the code, as long as you include the original copyright and license notice.

**Notes:**

* The "osu!" and "ppy" branding are trademarked and not covered by this license.
* Game resources are under a separate license: see [ppy/osu-resources](https://github.com/ppy/osu-resources).
