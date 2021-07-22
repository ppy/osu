// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps
{
    [LocalisableEnum(typeof(BeatmapSetOnlineStatusEnumLocalisationMapper))]
    public enum BeatmapSetOnlineStatus
    {
        None = -3,
        Graveyard = -2,
        WIP = -1,
        Pending = 0,
        Ranked = 1,
        Approved = 2,
        Qualified = 3,
        Loved = 4,
    }

    public static class BeatmapSetOnlineStatusExtensions
    {
        public static bool GrantsPerformancePoints(this BeatmapSetOnlineStatus status)
            => status == BeatmapSetOnlineStatus.Ranked || status == BeatmapSetOnlineStatus.Approved;
    }

    public class BeatmapSetOnlineStatusEnumLocalisationMapper : EnumLocalisationMapper<BeatmapSetOnlineStatus>
    {
        public override LocalisableString Map(BeatmapSetOnlineStatus value)
        {
            switch (value)
            {
                case BeatmapSetOnlineStatus.None:
                    return string.Empty;

                case BeatmapSetOnlineStatus.Graveyard:
                    return BeatmapsetsStrings.ShowStatusGraveyard;

                case BeatmapSetOnlineStatus.WIP:
                    return BeatmapsetsStrings.ShowStatusWip;

                case BeatmapSetOnlineStatus.Pending:
                    return BeatmapsetsStrings.ShowStatusPending;

                case BeatmapSetOnlineStatus.Ranked:
                    return BeatmapsetsStrings.ShowStatusRanked;

                case BeatmapSetOnlineStatus.Approved:
                    return BeatmapsetsStrings.ShowStatusApproved;

                case BeatmapSetOnlineStatus.Qualified:
                    return BeatmapsetsStrings.ShowStatusQualified;

                case BeatmapSetOnlineStatus.Loved:
                    return BeatmapsetsStrings.ShowStatusLoved;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
