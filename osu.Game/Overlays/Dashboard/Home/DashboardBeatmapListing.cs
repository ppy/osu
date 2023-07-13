// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.Dashboard.Home
{
    public partial class DashboardBeatmapListing : CompositeDrawable
    {
        private readonly List<APIBeatmapSet> newBeatmaps;
        private readonly List<APIBeatmapSet> popularBeatmaps;

        public DashboardBeatmapListing(List<APIBeatmapSet> newBeatmaps, List<APIBeatmapSet> popularBeatmaps)
        {
            this.newBeatmaps = newBeatmaps;
            this.popularBeatmaps = popularBeatmaps;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer<DrawableBeatmapList>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new DrawableBeatmapList[]
                {
                    new DrawableNewBeatmapList(newBeatmaps),
                    new DrawablePopularBeatmapList(popularBeatmaps)
                }
            };
        }
    }
}
