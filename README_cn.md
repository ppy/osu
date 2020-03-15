仍在翻译中，请以最终文案为准。
<p align="center">
  <img width="500px" src="assets/lazer.png">
</p>

# osu!

[![Build status](https://ci.appveyor.com/api/projects/status/u2p01nx7l6og8buh?svg=true)](https://ci.appveyor.com/project/peppy/osu)
[![GitHub release](https://img.shields.io/github/release/ppy/osu.svg)]()
[![CodeFactor](https://www.codefactor.io/repository/github/ppy/osu/badge)](https://www.codefactor.io/repository/github/ppy/osu)
[![dev chat](https://discordapp.com/api/guilds/188630481301012481/widget.png?style=shield)](https://discord.gg/ppy)

节奏一*触*即发。 osu!的未来和开放时代的开始！Commonly known by the codename *osu!lazer*. Pew pew.

## 状态

该项目目前仍处于开发阶段, 但已经足够稳定。欢迎各大osu玩家们前来尝试，并和*osu!*stable客户端一起安装。它将继续发展下去，直到可以通过一次更新取代现有stable客户端的安装。

如果你在体验时遇到了bug，欢迎前来提交 (请尽可能详细地汇报细节，并跟随现有的issue汇报模板)。功能性请求也同样受到欢迎，但请注意：我们目前主要会将注意力放在完成游戏已有功能上，因此请求的新功能可能不会很快地得到实现。下列资料可以帮助您作为参与和理解该项目的起点：

- 详细更新日志可以通过[osu!官方网站](https://osu.ppy.sh/home/changelog/lazer)查看。
- 您可以详细了解我们[管理该项目](https://github.com/ppy/osu/wiki/Project-management)的方法。
- 阅读peppy的[最新博客](https://blog.ppy.sh/a-definitive-lazer-faq/) 来获知当前lazer的开发状态以及将来前进的方向

## 运行osu！

如果您正在寻找一个可以不用搭建开发环境即可安装/测试osu!，您可以下载我们的[二进制发行版](https://github.com/ppy/osu/releases)。
下面的链接可以帮助您在您的操作系统上获取osu!的最新版本：

**最新构建：**

| [Windows (x64)](https://github.com/ppy/osu/releases/latest/download/install.exe)  | [macOS 10.12+](https://github.com/ppy/osu/releases/latest/download/osu.app.zip) | [iOS(iOS 10+)](https://osu.ppy.sh/home/testflight) | [Android (5+)](https://github.com/ppy/osu/releases/latest/download/sh.ppy.osulazer.apk) | [Linux (x64)](https://github.com/ppy/osu/releases/latest/download/osu.AppImage)
| ------------- | ------------- | ------------- | ------------- |

- 如果您在 Windows 7 或 8.1　上运行, 您也许需要下载 **[额外依赖](https://docs.microsoft.com/en-us/dotnet/core/install/dependencies?tabs=netcore31&pivots=os-windows)** 来运行 .NET Core 应用程序。

如果您的平台没有被列在上面，那么您仍可以尝试通过下面给出的操作手动构建。

## 开发或调试

请确保您准备好了下列依赖:

- 装有[.NET Core 3.1 SDK](https://dotnet.microsoft.com/download)或更高版本的桌面平台。
- 若要开发移动平台的osu！，您需要安装[Xamarin](https://docs.microsoft.com/en-us/xamarin/)，Xamarin通常会作为 Visual Studio 或 [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/)的一个安装选项来附带。
- 当编写代码时，我们推荐您使用带有自动补全和语法高亮的IDE来进行，如：[Visual Studio 2019+](https://visualstudio.microsoft.com/vs/), [JetBrains Rider](https://www.jetbrains.com/rider/) 或 [Visual Studio Code](https://code.visualstudio.com/).
- 当在Linux上运行时，osu！需要一个系统范围的FFmpeg安装，否则视频播放将无法使用。
* 译者注：您可以尝试将原本AppImage中的`AppRun`脚本替换为[这个pr](https://github.com/ppy/osu-deploy/pull/44/files)中提供的`AppRun`，如果在替换后视频播放正常，则代表您的发行版没有正确创建相关软链。

### 下载源码（需要git）

克隆项目解决方案:

```shell
git clone https://github.com/ppy/osu
cd osu
```

若要将源码更新至最后一次提交(commit)，在`osu`目录下执行以下指令：

```shell
git pull
```

### 构建

Build configurations for the recommended IDEs (listed above) are included. You should use the provided Build/Run functionality of your IDE to get things going. When testing or building new components, it's highly encouraged you use the `VisualTests` project/configuration. More information on this is provided [below](#contributing).

- Visual Studio / Rider users should load the project via one of the platform-specific `.slnf` files, rather than the main `.sln.` This will allow access to template run configurations.
- Visual Studio Code users must run the `Restore` task before any build attempt.

You can also build and run *osu!* from the command-line with a single command:

```shell
dotnet run --project osu.Desktop
```

If you are not interested in debugging *osu!*, you can add `-c Release` to gain performance. In this case, you must replace `Debug` with `Release` in any commands mentioned in this document.

If the build fails, try to restore NuGet packages with `dotnet restore`.

_Due to a historical feature gap between .NET Core and Xamarin, running `dotnet` CLI from the root directory will not work for most commands. This can be resolved by specifying a target `.csproj` or the helper project at `build/Desktop.proj`. Configurations have been provided to work around this issue for all supported IDEs mentioned above._

### Testing with resource/framework modifications

Sometimes it may be necessary to cross-test changes in [osu-resources](https://github.com/ppy/osu-resources) or [osu-framework](https://github.com/ppy/osu-framework). This can be achieved by running some commands as documented on the [osu-resources](https://github.com/ppy/osu-resources/wiki/Testing-local-resources-checkout-with-other-projects) and [osu-framework](https://github.com/ppy/osu-framework/wiki/Testing-local-framework-checkout-with-other-projects) wiki pages.

### Code analysis

Before committing your code, please run a code formatter. This can be achieved by running `dotnet format` in the command line, or using the `Format code` command in your IDE.

We have adopted some cross-platform, compiler integrated analyzers. They can provide warnings when you are editing, building inside IDE or from command line, as-if they are provided by the compiler itself.

JetBrains ReSharper InspectCode is also used for wider rule sets. You can run it from PowerShell with `.\InspectCode.ps1`, which is [only supported on Windows](https://youtrack.jetbrains.com/issue/RSRP-410004). Alternatively, you can install ReSharper or use Rider to get inline support in your IDE of choice.

## Contributing

We welcome all contributions, but keep in mind that we already have a lot of the UI designed. If you wish to work on something with the intention of having it included in the official distribution, please open an issue for discussion and we will give you what you need from a design perspective to proceed. If you want to make *changes* to the design, we recommend you open an issue with your intentions before spending too much time to ensure no effort is wasted.

If you're unsure of what you can help with, check out the [list of open issues](https://github.com/ppy/osu/issues) (especially those with the ["good first issue"](https://github.com/ppy/osu/issues?q=is%3Aissue+is%3Aopen+sort%3Aupdated-desc+label%3A%22good+first+issue%22) label).

Before starting, please make sure you are familiar with the [development and testing](https://github.com/ppy/osu-framework/wiki/Development-and-Testing) procedure we have set up. New component development, and where possible, bug fixing and debugging existing components **should always be done under VisualTests**.

Note that while we already have certain standards in place, nothing is set in stone. If you have an issue with the way code is structured, with any libraries we are using, or with any processes involved with contributing, *please* bring it up. We welcome all feedback so we can make contributing to this project as painless as possible.

For those interested, we love to reward quality contributions via [bounties](https://docs.google.com/spreadsheets/d/1jNXfj_S3Pb5PErA-czDdC9DUu4IgUbe1Lt8E7CYUJuE/view?&rm=minimal#gid=523803337), paid out via PayPal or osu!supporter tags. Don't hesitate to [request a bounty](https://docs.google.com/forms/d/e/1FAIpQLSet_8iFAgPMG526pBZ2Kic6HSh7XPM3fE8xPcnWNkMzINDdYg/viewform) for your work on this project.

## Licence

*osu!*'s code and framework are licensed under the [MIT licence](https://opensource.org/licenses/MIT). Please see [the licence file](LICENCE) for more information. [tl;dr](https://tldrlegal.com/license/mit-license) you can do whatever you want as long as you include the original copyright and license notice in any copy of the software/source.

Please note that this *does not cover* the usage of the "osu!" or "ppy" branding in any software, resources, advertising or promotion, as this is protected by trademark law.

Please also note that game resources are covered by a separate licence. Please see the [ppy/osu-resources](https://github.com/ppy/osu-resources) repository for clarifications.