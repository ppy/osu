# osu! [![Build status](https://ci.appveyor.com/api/projects/status/u2p01nx7l6og8buh?svg=true)](https://ci.appveyor.com/project/peppy/osu)  [![CodeFactor](https://www.codefactor.io/repository/github/ppy/osu/badge)](https://www.codefactor.io/repository/github/ppy/osu) [![dev chat](https://discordapp.com/api/guilds/188630481301012481/widget.png?style=shield)](https://discord.gg/ppy)

Rhythm is just a *click* away. The future of [osu!](https://osu.ppy.sh) and the beginning of an open era! Commonly known by the codename "osu!lazer". Pew pew.

# Status

This project is still heavily under development, but is in a state where users are encouraged to try it out and keep it installed alongside the stable osu! client. It will continue to evolve over the coming months and hopefully bring some new unique features to the table.

We are accepting bug reports (please report with as much detail as possible). Feature requests are welcome as long as you read and understand the contribution guidelines listed below.

# Requirements

- A desktop platform with the [.NET Core SDK 2.1](https://www.microsoft.com/net/learn/get-started) or higher installed.
- When working with the codebase, we recommend using an IDE with intellisense and syntax highlighting, such as [Visual Studio Community Edition](https://www.visualstudio.com/) (Windows), [Visual Studio Code](https://code.visualstudio.com/) (with the C# plugin installed) or [Jetbrains Rider](https://www.jetbrains.com/rider/) (commercial).

# Building and running

If you are not interested in developing the game, please head over to the [releases](https://github.com/ppy/osu/releases) to download a precompiled build with automatic updating enabled (download and run the install executable for your platform).

Clone the repository including submodules

`git clone --recurse-submodules https://github.com/ppy/osu`

Build and run

- Using Visual Studio 2017, Rider or Visual Studio Code (configurations are included)
- From command line using `dotnet run --project osu.Desktop --framework netcoreapp2.1`

If you run into issues building you may need to restore nuget packages (commonly via `dotnet restore`). Visual Studio Code users must run `Restore` task from debug tab before attempt to build.

# Contributing

We welcome all contributions, but keep in mind that we already have a lot of the UI designed. If you wish to work on something with the intention on having it included in the official distribution, please open an issue for discussion and we will give you what you need from a design perspective to proceed. If you want to make *changes* to the design, we recommend you open an issue with your intentions before spending too much time, to ensure no effort is wasted.

Please make sure you are familiar with the [development and testing](https://github.com/ppy/osu-framework/wiki/Development-and-Testing) procedure we have set up. New component development, and where possible, bug fixing and debugging existing components **should always be done under VisualTests**.

Contributions can be made via pull requests to this repository. We hope to credit and reward larger contributions via a [bounty system](https://www.bountysource.com/teams/ppy). If you're unsure of what you can help with, check out the [list of open issues](https://github.com/ppy/osu/issues).

Note that while we already have certain standards in place, nothing is set in stone. If you have an issue with the way code is structured; with any libraries we are using; with any processes involved with contributing, *please* bring it up. I welcome all feedback so we can make contributing to this project as pain-free as possible.

# Licence

The osu! client code and framework are licensed under the [MIT licence](https://opensource.org/licenses/MIT). Please see [the licence file](LICENCE) for more information. [tl;dr](https://tldrlegal.com/license/mit-license) you can do whatever you want as long as you include the original copyright and license notice in any copy of the software/source.

Please note that this *does not cover* the usage of the "osu!" or "ppy" branding in any software, resources, advertising or promotion, as this is protected by trademark law.

Please also note that game resources are covered by a separate licence. Please see the [ppy/osu-resources](https://github.com/ppy/osu-resources) repository for clarifications.
