// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using osu.Framework;

namespace osu.Game.Updater
{
    public class OsuGitHubUpdateChecker : GitHubUpdateChecker
    {
        protected override string GitHubUrl => @"https://api.github.com/repos/ppy/osu/releases/latest";

        private readonly string currentVersion;

        public OsuGitHubUpdateChecker(OsuGameBase osuGameBase)
        {
            currentVersion = osuGameBase.Version.Split('-').First();
        }

        protected override UpdateInfo? CreateUpdateInfo(GitHubRelease release)
        {
            string latestTagName = release.TagName.Split('-').First();

            if (latestTagName == currentVersion)
                return null;

            return new UpdateInfo("osu!", getBestUrl(release), currentVersion, latestTagName);
        }

        private string getBestUrl(GitHubRelease release)
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
