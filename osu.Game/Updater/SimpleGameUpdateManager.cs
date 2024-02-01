// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework;
using System.Runtime.InteropServices;

namespace osu.Game.Updater
{
    /// <summary>
    /// A simple update manager that is tailored for osu!'s update process.
    /// </summary>
    public partial class SimpleGameUpdateManager : SimpleUpdateManager
    {
        protected override string UpdatedComponentName => "osu!";
        protected override string GitHubUrl => "https://api.github.com/repos/ppy/osu/releases/latest";

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            Version = game.Version;

            // This will ensure that the version gets stored to the config as expected
            AddInternal(new GameVersionUpdater());
        }

        protected override string GetBestURL(GitHubRelease release)
        {
            GitHubAsset? bestAsset = null;

            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".exe", StringComparison.Ordinal));
                    break;

                case RuntimeInfo.Platform.macOS:
                    string arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "Apple.Silicon" : "Intel";
                    bestAsset = release.Assets?.Find(f => f.Name.EndsWith($".app.{arch}.zip", StringComparison.Ordinal));
                    break;

                case RuntimeInfo.Platform.Linux:
                    bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".AppImage", StringComparison.Ordinal));
                    break;

                case RuntimeInfo.Platform.iOS:
                    // iOS releases are available via testflight. this link seems to work well enough for now.
                    // see https://stackoverflow.com/a/32960501
                    return "itms-beta://beta.itunes.apple.com/v1/app/1447765923";

                case RuntimeInfo.Platform.Android:
                    // on our testing device this causes the download to magically disappear.
                    //bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".apk"));
                    break;
            }

            return bestAsset?.BrowserDownloadUrl ?? release.HtmlUrl;
        }
    }
}
