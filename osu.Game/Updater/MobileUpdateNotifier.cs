// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Online.API;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected for mobile platforms.
    /// Installation is left up to the user.
    /// </summary>
    public partial class MobileUpdateNotifier : UpdateManager
    {
        public override ReleaseStream? FixedReleaseStream => stream;

        private string version = null!;
        private ReleaseStream stream;

        [Resolved]
        private GameHost host { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            version = game.Version.Split('-').First();
            stream = Enum.TryParse(game.Version.Split('-').Last(), true, out ReleaseStream s) ? s : Configuration.ReleaseStream.Lazer;
        }

        protected override async Task<bool> PerformUpdateCheck(CancellationToken cancellationToken)
        {
            try
            {
                bool includePrerelease = stream == Configuration.ReleaseStream.Tachyon;

                OsuJsonWebRequest<GitHubRelease[]> releasesRequest = new OsuJsonWebRequest<GitHubRelease[]>("https://api.github.com/repos/ppy/osu/releases?per_page=10&page=1");
                await releasesRequest.PerformAsync(cancellationToken).ConfigureAwait(false);

                GitHubRelease[] releases = releasesRequest.ResponseObject;
                GitHubRelease? latest = releases.OrderByDescending(r => r.PublishedAt).FirstOrDefault(r => includePrerelease || !r.Prerelease);

                if (latest == null)
                    return false;

                string latestTagName = latest.TagName.Split('-').First();

                if (latestTagName != version && tryGetBestUrl(latest, out string? url))
                {
                    Notifications.Post(new UpdateAvailableNotification(cancellationToken)
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
                case RuntimeInfo.Platform.iOS:
                    if (release.Assets?.Exists(f => f.Name.EndsWith(".ipa", StringComparison.Ordinal)) == true)
                        // iOS releases are available via testflight. this link seems to work well enough for now.
                        // see https://stackoverflow.com/a/32960501
                        url = "itms-beta://beta.itunes.apple.com/v1/app/1447765923";

                    break;

                case RuntimeInfo.Platform.Android:
                    if (release.Assets?.Exists(f => f.Name.EndsWith(".apk", StringComparison.Ordinal)) == true)
                        // on our testing device using the .apk URL causes the download to magically disappear.
                        url = release.HtmlUrl;

                    break;
            }

            url ??= bestAsset?.BrowserDownloadUrl;
            return url != null;
        }
    }
}
