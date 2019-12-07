<p align="center">
  <img width="500px" src="assets/lazer.png">
</p>

[点击这里前往中文版简介](#中文简介)

# osu!

[![Build status](https://ci.appveyor.com/api/projects/status/u2p01nx7l6og8buh?svg=true)](https://ci.appveyor.com/project/peppy/osu)  [![CodeFactor](https://www.codefactor.io/repository/github/ppy/osu/badge)](https://www.codefactor.io/repository/github/ppy/osu) [![dev chat](https://discordapp.com/api/guilds/188630481301012481/widget.png?style=shield)](https://discord.gg/ppy)

Rhythm is just a *click* away. The future of [osu!](https://osu.ppy.sh) and the beginning of an open era! Commonly known by the codename "osu!lazer". Pew pew.

## Status

This project is still heavily under development, but is in a state where users are encouraged to try it out and keep it installed alongside the stable osu! client. It will continue to evolve over the coming months and hopefully bring some new unique features to the table.

We are accepting bug reports (please report with as much detail as possible). Feature requests are welcome as long as you read and understand the contribution guidelines listed below.

Detailed changelogs are published on the [official osu! site](https://osu.ppy.sh/home/changelog).

## Requirements

- A desktop platform with the [.NET Core SDK 3.0](https://www.microsoft.com/net/learn/get-started) or higher installed.
- When running on Linux, please have a system-wide FFmpeg installation available to support video decoding.
- When running on Windows 7 or 8.1, **[additional prerequisites](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x)** may be required to correctly run .NET Core applications if your operating system is not up-to-date with the latest service packs.
- When working with the codebase, we recommend using an IDE with intelligent code completion and syntax highlighting, such as [Visual Studio 2019+](https://visualstudio.microsoft.com/vs/), [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio Code](https://code.visualstudio.com/).

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

- Visual Studio / Rider users should load the project via one of the platform-specific .slnf files, rather than the main .sln. This will allow access to template run configurations.
- Visual Studio Code users must run the `Restore` task before any build attempt.

You can also build and run osu! from the command-line with a single command:

```shell
dotnet run --project osu.Desktop
```

If you are not interested in debugging osu!, you can add `-c Release` to gain performance. In this case, you must replace `Debug` with `Release` in any commands mentioned in this document.

If the build fails, try to restore NuGet packages with `dotnet restore`.

### Testing with resource/framework modifications

Sometimes it may be necessary to cross-test changes in [osu-resources](https://github.com/ppy/osu-resources) or [osu-framework](https://github.com/ppy/osu-framework). This can be achieved by running some commands as documented on the [osu-resources](https://github.com/ppy/osu-resources/wiki/Testing-local-resources-checkout-with-other-projects) and [osu-framework](https://github.com/ppy/osu-framework/wiki/Testing-local-framework-checkout-with-other-projects) wiki pages.

### Code analysis

Code analysis can be run with `powershell ./build.ps1` or `build.sh`. This is currently only supported under Windows due to [ReSharper CLI shortcomings](https://youtrack.jetbrains.com/issue/RSRP-410004). Alternatively, you can install ReSharper or use Rider to get inline support in your IDE of choice.

## Contributing

We welcome all contributions, but keep in mind that we already have a lot of the UI designed. If you wish to work on something with the intention of having it included in the official distribution, please open an issue for discussion and we will give you what you need from a design perspective to proceed. If you want to make *changes* to the design, we recommend you open an issue with your intentions before spending too much time, to ensure no effort is wasted.

If you're unsure of what you can help with, check out the [list of open issues](https://github.com/ppy/osu/issues) (especially those with the ["good first issue"](https://github.com/ppy/osu/issues?q=is%3Aissue+is%3Aopen+sort%3Aupdated-desc+label%3A%22good+first+issue%22) label).

Before starting, please make sure you are familiar with the [development and testing](https://github.com/ppy/osu-framework/wiki/Development-and-Testing) procedure we have set up. New component development, and where possible, bug fixing and debugging existing components **should always be done under VisualTests**.

Note that while we already have certain standards in place, nothing is set in stone. If you have an issue with the way code is structured; with any libraries we are using; with any processes involved with contributing, *please* bring it up. We welcome all feedback so we can make contributing to this project as pain-free as possible.

For those interested, we love to reward quality contributions via [bounties](https://docs.google.com/spreadsheets/d/1jNXfj_S3Pb5PErA-czDdC9DUu4IgUbe1Lt8E7CYUJuE/view?&rm=minimal#gid=523803337), paid out via PayPal or osu!supporter tags. Don't hesitate to [request a bounty](https://docs.google.com/forms/d/e/1FAIpQLSet_8iFAgPMG526pBZ2Kic6HSh7XPM3fE8xPcnWNkMzINDdYg/viewform) for your work on this project.

## Licence

The osu! client code and framework are licensed under the [MIT licence](https://opensource.org/licenses/MIT). Please see [the licence file](LICENCE) for more information. [tl;dr](https://tldrlegal.com/license/mit-license) you can do whatever you want as long as you include the original copyright and license notice in any copy of the software/source.

Please note that this *does not cover* the usage of the "osu!" or "ppy" branding in any software, resources, advertising or promotion, as this is protected by trademark law.

Please also note that game resources are covered by a separate licence. Please see the [ppy/osu-resources](https://github.com/ppy/osu-resources) repository for clarifications.

# 中文简介

**反馈翻译问题请在[这里](https://github.com/MATRIX-feather/osu/issues)提出Issue**

**反馈游戏bug前,请在[这里](https://github.com/ppy/osu/releases)下载最新版osu!lazer后,若问题依旧,再前往[osu官方项目地址](https://github.com/ppy/osu/issues)提出Issue**

# osu!

节奏一*触*即发. [osu!](https://osu.ppy.sh) 的未来以及开放时代的开始! 俗称代号 "osu!lazer" (￣▽￣)~*.

## Status

*后续补全*

## 需求

- 需要预先安装 [.NET Core SDK 3.0<sup>*</sup>](https://www.microsoft.com/net/learn/get-started)或者更高的版本.
- 如果在Linux上运行, 请先确保安装了全局 FFmpeg 以支持视频解码.
- 在Windows 7 或 8.1上运行时, 也许需要安装**[前置需求](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x)** 来正确的运行 .NET Core 应用如果你的系统并没有更新至最新的service packs(sp1,sp2什么的).
- 当在开发环境下时, 我们建议使用带有语法高亮的IDE, 例如 [Visual Studio 2019+](https://visualstudio.microsoft.com/vs/), [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio Code](https://code.visualstudio.com/).
- *:.NET Core SDK 3.0为编译所需要的库,若只是运行游戏,则只安装 .NET Core Runtime 3.0即可,这里是[下载链接](https://dotnet.microsoft.com/download)


## 运行 osu!

### 发行版

**以下为官方(英文版)下载链接,目前只编译了Linux版本,其他平台后续补全** *(~~因为硬盘空间不够了~~)*
如果你对开发不感兴趣, 你仍然可以下载我们的[二进制发行版](https://github.com/ppy/osu/releases).

**最新构建:**


| [Windows (64位)](https://github.com/ppy/osu/releases/latest/download/install.exe)  | [macOS 10.12+](https://github.com/ppy/osu/releases/latest/download/osu.app.zip) | [iOS(iOS 10+)](https://testflight.apple.com/join/2tLcjWlF) | [Android (5+)](https://github.com/ppy/osu/releases/latest/download/sh.ppy.osulazer.apk)
| ------------- | ------------- | ------------- | ------------- |

- **Linux** 用户在我们正式部署前,建议自行编译<sup>**</sup>

如果你的平台没有列在上面, 你仍然有机会可以通过下面的指令自行编译安装.

- **:[译者个人打的Deb包qwp](https://github.com/MATRIX-feather/osulazer-package),Debian系发行版(Ubuntu等)克隆源码后运行`build-deb.sh`即可

### 下载源码

克隆该项目:

```shell
git clone https://github.com/ppy/osu
cd osu
```

要将源代码更新至最新版本,在`osu`目录下运行以下命令即可:

```shell
git pull
```

### 构建

建议使用上面列出的IDE进行构建.您应该使用IDE提供的"构建/运行"功能来进行操作. 在测试或构建新的组件时,我们强烈建议您使用VisualTests项目/配置. 更多信息请看[该节](#contributing).

- Visual Studio / Rider 用户应通过特定于平台的.slnf文件之一加载项目,而不是 .sln. 这将允许访问模板运行配置.
- Visual Studio Code用户必须在执行任何构建尝试之前运行`restore`任务。

*~~(以上两句机翻)~~*


你也可以通过以下指令构建并运行osu!:

```shell
dotnet run --project osu.Desktop
```

如果你对调试 osu! 不感兴趣, 你可以添加 `-c Release` 以增强游戏性能. 如果这样, 你需要将本文中所有命令中的 `Debug` 替换为 `Release` .

如果构建失败, 尝试通过`dotnet restore`恢复 NuGet 包.


# 更多翻译正在补全,一切请以[官方英文版](https://github.com/ppy/osu/blob/master/README.md)为准~