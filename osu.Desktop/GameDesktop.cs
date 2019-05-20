// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Desktop.Platform.Linux;
using osu.Desktop.Platform.MacOS;
using osu.Desktop.Platform.Windows;
using osu.Framework;

namespace osu.Desktop
{
    public static class GameDesktop
    {
        internal static OsuGameDesktop GetSuitableDesktopGame(string[] args = null)
        {
            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.MacOsx:
                    return new MacOSOsuGameDesktop(args);

                case RuntimeInfo.Platform.Linux:
                    return new LinuxOsuGameDesktop(args);

                case RuntimeInfo.Platform.Windows:
                    return new WindowsOsuGameDesktop(args);

                default:
                    throw new InvalidOperationException(
                        $"Could not find a suitable {nameof(OsuGameDesktop)} for the selected operating system ({Enum.GetName(typeof(RuntimeInfo.Platform), RuntimeInfo.OS)}).");
            }
        }
    }
}
