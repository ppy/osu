// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
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
    /// <remarks>
    /// Implementers are expected to set <see cref="Version"/> to something that matches their release versioning/tagging system.
    /// </remarks>
    public abstract partial class SimpleUpdateManager : UpdateManager
    {
        protected string Version { get; set; } = "0.0.0";

        protected abstract string GitHubUrl { get; }

        // For lack of a better term
        protected abstract string UpdatedComponentName { get; }

        protected virtual string DownloadMessage { get; } = "Click here to download the new version, which can be installed over the top of your existing installation";

        [Resolved]
        private GameHost host { get; set; } = null!;

        protected override async Task<bool> PerformUpdateCheck()
        {
            try
            {
                var releases = new OsuJsonWebRequest<GitHubRelease>(GitHubUrl);

                await releases.PerformAsync().ConfigureAwait(false);

                var latest = releases.ResponseObject;

                // avoid any discrepancies due to build suffixes for now.
                // eventually we will want to support release streams and consider these.
                Version = Version.Split('-').First();
                string latestTagName = latest.TagName.Split('-').First();

                if (latestTagName != Version)
                {
                    Notifications.Post(new SimpleNotification
                    {
                        Text = $"A newer release of {UpdatedComponentName} has been found ({Version} → {latestTagName}).\n\n"
                               + DownloadMessage,
                        Icon = FontAwesome.Solid.Download,
                        Activated = () =>
                        {
                            UpdateActionOnClick(latest);
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

        protected virtual void UpdateActionOnClick(GitHubRelease release) => host.OpenUrlExternally(GetBestURL(release));

        protected virtual string GetBestURL(GitHubRelease release) => release.HtmlUrl;
    }
}
