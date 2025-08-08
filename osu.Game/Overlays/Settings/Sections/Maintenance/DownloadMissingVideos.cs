// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Notifications;
using osu.Game.Utils;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class DownloadMissingVideos : Component
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        [Resolved(canBeNull: true)]
        private SettingsOverlay? settingsOverlay { get; set; }

        [Resolved(canBeNull: true)]
        private LoginOverlay? loginOverlay { get; set; }

        private readonly List<int> downloadedBeatmapSetIds = new List<int>();
        private readonly Action enableButton;
        private readonly Action<int, Action, Action> showConfirmationDialog;

        /// <summary>
        /// Creates a new instance of the <see cref="DownloadMissingVideos"/> class.
        /// </summary>
        /// <param name="enableButton">The action to enable the button after completing the process.</param>
        /// <param name="showConfirmationDialog">The action to show the confirmation dialog with parameters for count, confirm, and cancel actions.</param>
        /// <returns>A new instance of the <see cref="DownloadMissingVideos"/> class.</returns>
        internal static DownloadMissingVideos Create(Action enableButton, Action<int, Action, Action> showConfirmationDialog)
        {
            return new DownloadMissingVideos(enableButton, showConfirmationDialog);
        }

        public DownloadMissingVideos(Action enableButton, Action<int, Action, Action> showConfirmationDialog)
        {
            this.enableButton = enableButton;
            this.showConfirmationDialog = showConfirmationDialog;
        }

        internal async Task StartDownloadMissingVideos()
        {
            try
            {
                if (api.State.Value != APIState.Online)
                {
                    Schedule(() =>
                    {
                        settingsOverlay?.Hide();
                        loginOverlay?.Show();
                        enableButton();
                    });
                    return;
                }

                var candidatesWithoutVideo = new List<BeatmapSetInfo>();

                realm.Run(r =>
                {
                    var allSets = r.All<BeatmapSetInfo>().Where(static s => !s.DeletePending && !s.Protected).ToList();

                    foreach (var localSet in allSets)
                    {
                        if (localSet.OnlineID <= 0)
                            continue;

                        bool hasLocalVideo = localSet.Files.Any(static f => SupportedExtensions.VIDEO_EXTENSIONS.Any(ex => f.Filename.EndsWith(ex, StringComparison.OrdinalIgnoreCase)));

                        if (!hasLocalVideo)
                            candidatesWithoutVideo.Add(localSet.Detach());
                    }
                });

                if (candidatesWithoutVideo.Count == 0)
                {
                    Schedule(() =>
                    {
                        notificationOverlay?.Post(new ProgressCompletionNotification
                        {
                            Text = MaintenanceSettingsStrings.NoMissingVideosFound
                        });
                        enableButton();
                    });
                    return;
                }

                int actualDownloadCount = 0;
                var downloadTasks = new List<Task>();
                var candidatesWithVideos = new List<BeatmapSetInfo>();

                foreach (var localSet in candidatesWithoutVideo)
                {
                    var tcs = new TaskCompletionSource<bool>();
                    var request = new GetBeatmapSetRequest(localSet.OnlineID);

                    request.Success += response =>
                    {
                        if (response.HasVideo)
                        {
                            candidatesWithVideos.Add(localSet);
                            actualDownloadCount++;
                        }

                        tcs.SetResult(true);
                    };

                    request.Failure += _ => tcs.SetResult(false);

                    _ = api.PerformAsync(request);
                    downloadTasks.Add(tcs.Task);
                }

                await Task.WhenAll(downloadTasks).ConfigureAwait(false);

                if (actualDownloadCount == 0)
                {
                    Schedule(() =>
                    {
                        notificationOverlay?.Post(new ProgressCompletionNotification
                        {
                            Text = MaintenanceSettingsStrings.NoMissingVideosFound
                        });
                        enableButton();
                    });
                    return;
                }

                bool dialogShown = false;
                Schedule(() =>
                {
                    showConfirmationDialog(actualDownloadCount, () =>
                    {
                        Task.Run(() => downloadConfirmed(candidatesWithVideos));
                    }, () =>
                    {
                        enableButton();
                    });
                    dialogShown = true;
                });

                while (!dialogShown)
                    await Task.Delay(10).ConfigureAwait(false);
            }
            catch
            {
                Schedule(() => enableButton());
            }
        }

        private async Task downloadConfirmed(List<BeatmapSetInfo> mapsWithVideos)
        {
            int downloadCount = 0;
            downloadedBeatmapSetIds.Clear();

            foreach (var localSet in mapsWithVideos)
            {
                var request = new GetBeatmapSetRequest(localSet.OnlineID);
                var tcs = new TaskCompletionSource<bool>();

                request.Success += response =>
                {
                    if (response.HasVideo)
                    {
                        if (beatmapDownloader.Download(response, false))
                        {
                            downloadedBeatmapSetIds.Add(response.OnlineID);
                            downloadCount++;
                        }
                    }

                    tcs.SetResult(true);
                };

                request.Failure += _ => tcs.SetResult(false);

                _ = api.PerformAsync(request);
                await tcs.Task.ConfigureAwait(false);
            }

            if (downloadCount > 0)
                _ = Task.Run(monitorDownloadsCompletion);
            else
                Schedule(() => enableButton());
        }

        private async Task monitorDownloadsCompletion()
        {
            while (true)
            {
                bool allComplete = true;

                foreach (int beatmapSetId in downloadedBeatmapSetIds)
                {
                    var dummyResponse = new APIBeatmapSet { OnlineID = beatmapSetId };
                    var existingDownload = beatmapDownloader.GetExistingDownload(dummyResponse);

                    if (existingDownload != null)
                    {
                        allComplete = false;
                        break;
                    }
                }

                if (allComplete)
                {
                    Schedule(() => enableButton());
                    break;
                }

                await Task.Delay(500).ConfigureAwait(false);
            }
        }
    }
}
