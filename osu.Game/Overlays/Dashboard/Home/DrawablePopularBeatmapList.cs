// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Dashboard.Home
{
    public class DrawablePopularBeatmapList : DrawableBeatmapList
    {
        public DrawablePopularBeatmapList(List<APIBeatmapSet> beatmapSets)
            : base(beatmapSets)
        {
        }

        protected override DashboardBeatmapPanel CreateBeatmapPanel(APIBeatmapSet beatmapSet) => new DashboardPopularBeatmapPanel(beatmapSet);

        protected override string Title => "Popular Beatmaps";
    }
}
