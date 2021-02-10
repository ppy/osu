// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
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
    public class OnlinePlayBeatmapAvailablilityTracker : DownloadTrackingComposite<BeatmapSetInfo, BeatmapManager>
    {
        public readonly IBindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        /// <summary>
        /// The availability state of the currently selected playlist item.
        /// </summary>
        public IBindable<BeatmapAvailability> Availability => availability;

        private readonly Bindable<BeatmapAvailability> availability = new Bindable<BeatmapAvailability>(BeatmapAvailability.LocallyAvailable());

        private ScheduledDelegate progressUpdate;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(item =>
            {
                // the underlying playlist is regularly cleared for maintenance purposes (things which probably need to be fixed eventually).
                // to avoid exposing a state change when there may actually be none, ignore all nulls for now.
                if (item.NewValue == null)
                    return;

                Model.Value = item.NewValue.Beatmap.Value.BeatmapSet;
            }, true);

            Progress.BindValueChanged(_ =>
            {
                // incoming progress changes are going to be at a very high rate.
                // we don't want to flood the network with this, so rate limit how often we send progress updates.
                if (progressUpdate?.Completed != false)
                    progressUpdate = Scheduler.AddDelayed(updateAvailability, progressUpdate == null ? 0 : 500);
            });

            State.BindValueChanged(_ => updateAvailability(), true);
        }

        protected override bool VerifyDatabasedModel(BeatmapSetInfo databasedSet)
        {
            int? beatmapId = SelectedItem.Value.Beatmap.Value.OnlineBeatmapID;
            string checksum = SelectedItem.Value.Beatmap.Value.MD5Hash;

            var matchingBeatmap = databasedSet.Beatmaps.FirstOrDefault(b => b.OnlineBeatmapID == beatmapId && b.MD5Hash == checksum);

            if (matchingBeatmap == null)
            {
                Logger.Log("The imported beatmap set does not match the online version.", LoggingTarget.Runtime, LogLevel.Important);
                return false;
            }

            return true;
        }

        protected override bool IsModelAvailableLocally()
        {
            int? beatmapId = SelectedItem.Value.Beatmap.Value.OnlineBeatmapID;
            string checksum = SelectedItem.Value.Beatmap.Value.MD5Hash;

            var beatmap = Manager.QueryBeatmap(b => b.OnlineBeatmapID == beatmapId && b.MD5Hash == checksum);
            return beatmap?.BeatmapSet.DeletePending == false;
        }

        private void updateAvailability()
        {
            switch (State.Value)
            {
                case DownloadState.NotDownloaded:
                    availability.Value = BeatmapAvailability.NotDownloaded();
                    break;

                case DownloadState.Downloading:
                    availability.Value = BeatmapAvailability.Downloading((float)Progress.Value);
                    break;

                case DownloadState.Importing:
                    availability.Value = BeatmapAvailability.Importing();
                    break;

                case DownloadState.LocallyAvailable:
                    availability.Value = BeatmapAvailability.LocallyAvailable();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(State));
            }
        }
    }
}
