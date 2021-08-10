// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Overlays.Settings.Sections.BeatmapDownloader
{
    public class BeatmapDownloaderButtons : SettingsSubsection
    {

        [Resolved(CanBeNull = true)]
        private NotificationOverlay notifications { get; set; }

        protected override LocalisableString Header => "Downloader Buttons";

        private SettingsButton downloadBeatmapsButton;

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, Beatmaps.BeatmapDownloader beatmapDownloader)
        {
            Add(downloadBeatmapsButton = new SettingsButton
            {
                Text = "Download Beatmaps now",
                Action = () =>
                {
                    downloadBeatmapsButton.Enabled.Value = false;
                    Task.Run(beatmapDownloader.DownloadBeatmaps).ContinueWith(t => Schedule(() =>
                    {
                        if (t.Result.Length == 0)
                        {
                            notifications?.Post(new SimpleNotification
                            {
                                Text = BeatmapDownloaderStrings.FinishedDownloadingNewBeatmaps.ToString(),
                                Icon = FontAwesome.Solid.Check,
                            });
                        }
                        else
                        {
                            notifications?.Post(new SimpleNotification
                            {
                                Text = BeatmapDownloaderStrings.AnErrorHasOccuredWhileDownloadingTheBeatmaps(t.Result).ToString(),
                                Icon = FontAwesome.Solid.Cross,
                            });
                        }
                        downloadBeatmapsButton.Enabled.Value = true;
                    }));
                }
            });

            Add(new SettingsButton
            {
                Text = "Stop the Downloader",
                Action = () =>
                {
                    beatmapDownloader.ForceStop();
                }
            });

            Add(new SettingsButton
            {
                Text = "Set Time to now",
                Action = () =>
                {
                    config.GetBindable<DateTime>(OsuSetting.BeatmapDownloadLastTime).Value = DateTime.Now;
                }
            });

            Add(new SettingsButton
            {
                Text = "Set Time to 1 week ago",
                Action = () =>
                {
                    config.GetBindable<DateTime>(OsuSetting.BeatmapDownloadLastTime).Value = new DateTime(DateTime.Now.Ticks - TimeSpan.TicksPerDay * 7);
                }
            });
        }
    }
}
