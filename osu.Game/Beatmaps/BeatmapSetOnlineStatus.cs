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
        [Description("未知")]
        None = -3,
        [Description("坟图")]
        Graveyard = -2,
        [Description("制作中")]
        WIP = -1,
        [Description("审核中")]
        Pending = 0,
        [Description("计入排名")]
        Ranked = 1,
        [Description("已改进")]
        Approved = 2,
        [Description("质量合格")]
        Qualified = 3,
        [Description("Loved")]
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
