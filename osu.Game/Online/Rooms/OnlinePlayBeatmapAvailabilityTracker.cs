// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Beatmaps;

namespace osu.Game.Online.Rooms
{
    /// <summary>
    /// Represent a checksum-verifying beatmap availability tracker usable for online play screens.
    ///
    /// This differs from a regular download tracking composite as this accounts for the
    /// databased beatmap set's checksum, to disallow from playing with an altered version of the beatmap.
    /// </summary>
    public sealed class OnlinePlayBeatmapAvailabilityTracker : CompositeDrawable
    {
        public readonly IBindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        // Required to allow child components to update. Can potentially be replaced with a `CompositeComponent` class if or when we make one.
        protected override bool RequiresChildrenUpdate => true;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        /// <summary>
        /// The availability state of the currently selected playlist item.
        /// </summary>
        public IBindable<BeatmapAvailability> Availability => availability;

        private readonly Bindable<BeatmapAvailability> availability = new Bindable<BeatmapAvailability>(BeatmapAvailability.NotDownloaded());

        private ScheduledDelegate progressUpdate;

        private BeatmapDownloadTracker downloadTracker;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(item =>
            {
                // the underlying playlist is regularly cleared for maintenance purposes (things which probably need to be fixed eventually).
                // to avoid exposing a state change when there may actually be none, ignore all nulls for now.
                if (item.NewValue == null)
                    return;

                downloadTracker?.RemoveAndDisposeImmediately();

                Debug.Assert(item.NewValue.Beatmap.Value.BeatmapSet != null);

                downloadTracker = new BeatmapDownloadTracker(item.NewValue.Beatmap.Value.BeatmapSet);

                AddInternal(downloadTracker);

                downloadTracker.State.BindValueChanged(_ => updateAvailability(), true);
                downloadTracker.Progress.BindValueChanged(_ =>
                {
                    if (downloadTracker.State.Value != DownloadState.Downloading)
                        return;

                    // incoming progress changes are going to be at a very high rate.
                    // we don't want to flood the network with this, so rate limit how often we send progress updates.
                    if (progressUpdate?.Completed != false)
                        progressUpdate = Scheduler.AddDelayed(updateAvailability, progressUpdate == null ? 0 : 500);
                }, true);
            }, true);
        }

        private void updateAvailability()
        {
            if (downloadTracker == null)
                return;

            switch (downloadTracker.State.Value)
            {
                case DownloadState.NotDownloaded:
                    availability.Value = BeatmapAvailability.NotDownloaded();
                    break;

                case DownloadState.Downloading:
                    availability.Value = BeatmapAvailability.Downloading((float)downloadTracker.Progress.Value);
                    break;

                case DownloadState.Importing:
                    availability.Value = BeatmapAvailability.Importing();
                    break;

                case DownloadState.LocallyAvailable:
                    bool hashMatches = checkHashValidity();

                    availability.Value = hashMatches ? BeatmapAvailability.LocallyAvailable() : BeatmapAvailability.NotDownloaded();

                    // only display a message to the user if a download seems to have just completed.
                    if (!hashMatches && downloadTracker.Progress.Value == 1)
                        Logger.Log("The imported beatmap set does not match the online version.", LoggingTarget.Runtime, LogLevel.Important);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool checkHashValidity()
        {
            int onlineId = SelectedItem.Value.Beatmap.Value.OnlineID;
            string checksum = SelectedItem.Value.Beatmap.Value.MD5Hash;

            return beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == onlineId && b.MD5Hash == checksum && !b.BeatmapSet.DeletePending) != null;
        }
    }
}
