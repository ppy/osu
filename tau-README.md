[![tau](https://cdn.discordapp.com/attachments/680267083606655030/688688294329843732/the.png)](https://github.com/Altenhh/tau "tau")
# osu!tau
[![release](https://img.shields.io/badge/build-1.0.4-brightgreen?style=flat-square)](https://github.com/Altenhh/tau/releases)
[![GitHub license](https://img.shields.io/github/license/Altenhh/tau.svg?style=flat-square)](https://github.com/Altenhh/tau/LICENSE) 
[![GitHub stars](https://img.shields.io/github/stars/Altenhh/tau.svg?style=flat-square)](https://github.com/Altenhh/tau/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/Altenhh/tau.svg?style=flat-square)](https://github.com/Altenhh/tauE/network)
[![GitHub issues](https://img.shields.io/github/issues/Altenhh/tau.svg?style=flat-square)](https://github.com/Altenhh/tau/issues)

A customized [osu!](https://github.com/ppy/osu) mode surrounding a paddle and some notes. Original credit to the idea belonging to *pizzapip*.

## Running the Gamemode
We have [prebuilt libraries](https://github.com/Altenhh/tau/releases) for users looking to play the mode without creating a development environment. All releases will work on all operating systems that *osu!* supports.
### [Latest Releases](https://github.com/Altenhh/tau/releases)

### Instructions
##### Windows
On Windows, the library must be put in `%localappdata%\osulazer\app-(Current osu!lazer Version)`, inside the directory of the current osu!lazer version. osu!lazer will automatically work with the `.dll` when you open it, so nothing else needs to be done.

*If instructions for your platform isn't listed above, then it's either being written, or is unsupported. At the very time of writing this (March 15th, 2020), no operating system is known to be unsupported.*

## Development
When developing or debugging the osu!tau codebase, a few prerequisites are required as following:
* An IDE that supports the C# language in automatic completion, and syntax highlighting; examples of such being [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) and above, or [JetBrains Rider](https://www.jetbrains.com/rider/).
* The [osu!framework](https://github.com/ppy/osu-framework/tree/master/osu.Framework), and [osu!](https://github.com/ppy/osu) codebases are added as dependencies for building

### Source Code 
You are able to clone the repository over command line, or by downloading it. Updating this code to the latest commit would be done with `git pull`, inside the osu!tau directory.
```sh
git clone https://github.com/Altenhh/tau.git
cd tau/tau
```

### Building the Gamemode
[SECTION WIP]

## Contributions
All contributions are appreciated, as to improve the mode on its playability and functionality. As this gamemode isn't perfect, we would enjoy all additions to the code through bugfixing and ideas. Contributions should be done over an issue or a pull request, to give maintainers a chance to review changes to the codebase.

For new ideas and features, we would prefer for you to write an issue before trying to add it to show the maintainers.

## License
osu!tau is licenced under the [MIT](https://opensource.org/licenses/MIT) License. For licensing information, refer to the [license file](https://github.com/Altenhh/tau/blob/master/LICENSE) regarding what is permitted regarding the codebase of osu!tau.

The licensing here does not directly apply to [osu!](https://github.com/ppy/osu), as it is bound to its own licensing. What is reflected in our licensing *may* not be allowed in the [osu!](https://github.com/ppy/osu) github repository.
