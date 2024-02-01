// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.
    /// Installation is left up to the user.
    /// </summary>
    public partial class SimpleUpdateManager : UpdateManager
    {
        private string version = null!;

        [Resolved]
        private GameHost host { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            version = game.Version;
        }

        protected override async Task<bool> PerformUpdateCheck()
        {
            try
            {
                var releases = new OsuJsonWebRequest<GitHubRelease>("https://api.github.com/repos/ppy/osu/releases/latest");

                await releases.PerformAsync().ConfigureAwait(false);

                var latest = releases.ResponseObject;

                // avoid any discrepancies due to build suffixes for now.
                // eventually we will want to support release streams and consider these.
                version = version.Split('-').First();
                string latestTagName = latest.TagName.Split('-').First();

                if (latestTagName != version && tryGetBestUrl(latest, out string? url))
                {
                    Notifications.Post(new SimpleNotification
                    {
                        Text = $"A newer release of osu! has been found ({version} → {latestTagName}).\n\n"
                               + "Click here to download the new version, which can be installed over the top of your existing installation",
                        Icon = FontAwesome.Solid.Download,
                        Activated = () =>
                        {
                            host.OpenUrlExternally(url);
                            return true;
                        }
                    });

                    return true;
                }
            }
            catch
            {
                // we shouldn't crash on a web failure. or any failure for the matter.
                return true;
            }

            return false;
        }

        private bool tryGetBestUrl(GitHubRelease release, [NotNullWhen(true)] out string? url)
        {
            url = null;
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
                    url = "itms-beta://beta.itunes.apple.com/v1/app/1447765923";
                    break;

                case RuntimeInfo.Platform.Android:
                    // on our testing device using the .apk URL causes the download to magically disappear.
                    //bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".apk"));
                    url = release.HtmlUrl;
                    break;
            }

            url ??= bestAsset?.BrowserDownloadUrl;
            return url != null;
        }
    }
}
