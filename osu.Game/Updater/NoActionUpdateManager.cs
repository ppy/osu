// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Online.API;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.
    /// This is a case where updates are handled externally by a package manager or other means, so no action is performed on clicking the notification.
    /// </summary>
    public partial class NoActionUpdateManager : UpdateManager
    {
        public override ReleaseStream? FixedReleaseStream => externalReleaseStream;

        private static ReleaseStream? externalReleaseStream => Enum.TryParse(Environment.GetEnvironmentVariable("OSU_EXTERNAL_UPDATE_STREAM"), true, out ReleaseStream stream) ? stream : null;

        private string version = string.Empty;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            version = game.Version.Split('-').First();
        }

        protected override async Task<bool> PerformUpdateCheck(CancellationToken cancellationToken)
        {
            try
            {
                ReleaseStream stream = externalReleaseStream ?? ReleaseStream.Value;
                bool includePrerelease = stream == Configuration.ReleaseStream.Tachyon;

                OsuJsonWebRequest<GitHubRelease[]> releasesRequest = new OsuJsonWebRequest<GitHubRelease[]>("https://api.github.com/repos/ppy/osu/releases?per_page=10&page=1");
                await releasesRequest.PerformAsync(cancellationToken).ConfigureAwait(false);

                GitHubRelease[] releases = releasesRequest.ResponseObject;
                GitHubRelease? latest = releases.OrderByDescending(r => r.PublishedAt).FirstOrDefault(r => includePrerelease || !r.Prerelease);

                if (latest == null)
                    return false;

                string latestTagName = latest.TagName.Split('-').First();

                if (latestTagName != version)
                {
                    Notifications.Post(new UpdateAvailableNotification(cancellationToken)
                    {
                        Text = $"A newer release of osu! has been found ({version} → {latestTagName}).\n\n"
                               + "Check with your package manager / provider to bring osu! up-to-date!",
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
    }
}
