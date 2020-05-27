// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Updating
{
    /// <summary>
    /// An updater that shows notifications if a newer release is detected.
    /// Installation is left up to the user.
    /// </summary>
    public class SimpleUpdater : Updater
    {
        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private NotificationOverlay notifications { get; set; }

        public override async Task<bool> CheckAndPrepareAsync()
        {
            try
            {
                var request = new OsuJsonWebRequest<GitHubRelease>("https://api.github.com/repos/ppy/osu/releases/latest");

                await request.PerformAsync();

                var latest = request.ResponseObject;

                if (latest.TagName == game.Version)
                    // no newer releases found, return.
                    return false;

                notifications.Post(new SimpleNotification
                {
                    Text = $"A newer release of osu! has been found ({game.Version} → {latest.TagName}).\n\n"
                           + "Click here to download the new version, which can be installed over the top of your existing installation",
                    Icon = FontAwesome.Solid.Upload,
                    Activated = () =>
                    {
                        host.OpenUrlExternally(getBestUrl(latest));
                        return true;
                    }
                });

                return true;
            }
            catch
            {
                // we shouldn't crash on a web failure. or any failure for the matter.
                return false;
            }
        }

        private string getBestUrl(GitHubRelease release)
        {
            GitHubAsset bestAsset = null;

            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".exe"));
                    break;

                case RuntimeInfo.Platform.MacOsx:
                    bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".app.zip"));
                    break;

                case RuntimeInfo.Platform.Linux:
                    bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".AppImage"));
                    break;

                case RuntimeInfo.Platform.Android:
                    // on our testing device this causes the download to magically disappear.
                    //bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".apk"));
                    break;
            }

            return bestAsset?.BrowserDownloadUrl ?? release.HtmlUrl;
        }

        private class GitHubRelease
        {
            [JsonProperty("html_url")]
            public string HtmlUrl { get; set; }

            [JsonProperty("tag_name")]
            public string TagName { get; set; }

            [JsonProperty("assets")]
            public List<GitHubAsset> Assets { get; set; }
        }

        private class GitHubAsset
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }
        }
    }
}
