﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Network;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Desktop.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.
    /// Installation is left up to the user.
    /// </summary>
    internal class SimpleUpdateManager : CompositeDrawable
    {
        private NotificationOverlay notificationOverlay;
        private string version;
        private GameHost host;

        [BackgroundDependencyLoader]
        private void load(NotificationOverlay notification, OsuGameBase game, GameHost host)
        {
            notificationOverlay = notification;

            this.host = host;
            version = game.Version;

            if (game.IsDeployedBuild)
                Schedule(() => Task.Run(() => checkForUpdateAsync()));
        }

        private async void checkForUpdateAsync()
        {
            try
            {
                var releases = new JsonWebRequest<GitHubRelease>("https://api.github.com/repos/ppy/osu/releases/latest");

                await releases.PerformAsync();

                var latest = releases.ResponseObject;

                if (latest.TagName != version)
                {
                    notificationOverlay.Post(new SimpleNotification
                    {
                        Text = $"A newer release of osu! has been found ({version} → {latest.TagName}).\n\n"
                               + "Click here to download the new version, which can be installed over the top of your existing installation",
                        Icon = FontAwesome.fa_upload,
                        Activated = () =>
                        {
                            host.OpenUrlExternally(getBestUrl(latest));
                            return true;
                        }
                    });
                }
            }
            catch
            {
                // we shouldn't crash on a web failure. or any failure for the matter.
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
            }

            return bestAsset?.BrowserDownloadUrl ?? release.HtmlUrl;
        }

        public class GitHubRelease
        {
            [JsonProperty("html_url")]
            public string HtmlUrl { get; set; }

            [JsonProperty("tag_name")]
            public string TagName { get; set; }

            [JsonProperty("assets")]
            public List<GitHubAsset> Assets { get; set; }
        }

        public class GitHubAsset
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }
        }
    }
}
