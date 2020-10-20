// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace osu.Game.Overlays.Dashboard.Home
{
    public class DrawablePopularBeatmapList : DrawableBeatmapList
    {
        public DrawablePopularBeatmapList(List<BeatmapSetInfo> beatmaps)
            : base(beatmaps)
        {
        }

        protected override DashboardBeatmapPanel CreateBeatmapPanel(BeatmapSetInfo setInfo) => new DashboardPopularBeatmapPanel(setInfo);

        protected override string Title => "Popular Beatmaps";
    }
}
