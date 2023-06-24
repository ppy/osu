// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps
{
    public enum BeatmapOnlineStatus
    {
        /// <summary>
        /// This is a special status given when local changes are made via the editor.
        /// Once in this state, online status changes should be ignored unless the beatmap is reverted or submitted.
        /// </summary>
        [Description("Local")]
        [LocalisableDescription(typeof(SongSelectStrings), nameof(SongSelectStrings.LocallyModified))]
        LocallyModified = -4,

        None = -3,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowStatusGraveyard))]
        Graveyard = -2,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowStatusWip))]
        WIP = -1,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowStatusPending))]
        Pending = 0,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowStatusRanked))]
        Ranked = 1,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowStatusApproved))]
        Approved = 2,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowStatusQualified))]
        Qualified = 3,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowStatusLoved))]
        Loved = 4,
    }

    public static class BeatmapSetOnlineStatusExtensions
    {
        public static bool GrantsPerformancePoints(this BeatmapOnlineStatus status)
            => status == BeatmapOnlineStatus.Ranked || status == BeatmapOnlineStatus.Approved;
    }
}
