// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Beatmaps
{
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
}
