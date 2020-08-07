// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Dashboard.Home
{
    public abstract class DrawableBeatmapList : CompositeDrawable
    {
        private readonly List<BeatmapSetInfo> beatmaps;

        protected DrawableBeatmapList(List<BeatmapSetInfo> beatmaps)
        {
            this.beatmaps = beatmaps;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            FillFlowContainer flow;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                        Colour = colourProvider.Light1,
                        Text = Title
                    }
                }
            };

            flow.AddRange(beatmaps.Select(CreateBeatmapPanel));
        }

        protected abstract string Title { get; }

        protected abstract DashboardBeatmapPanel CreateBeatmapPanel(BeatmapSetInfo setInfo);
    }
}
