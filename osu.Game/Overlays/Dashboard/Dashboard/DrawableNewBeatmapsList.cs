// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace osu.Game.Overlays.Dashboard.Dashboard
{
    public class DrawableNewBeatmapsList : DrawableBeatmapsList
    {
        public DrawableNewBeatmapsList(List<BeatmapSetInfo> beatmaps)
            : base(beatmaps)
        {
        }

        protected override DashboardBeatmapPanel CreateBeatmapPanel(BeatmapSetInfo setInfo) => new DashboardNewBeatmapPanel(setInfo);

        protected override string CreateTitle() => "New Ranked Beatmaps";
    }
}
