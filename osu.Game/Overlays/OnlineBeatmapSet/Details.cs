// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Details;

namespace osu.Game.Overlays.OnlineBeatmapSet
{
    public class Details : FillFlowContainer
    {
        private readonly BasicStats basic;
        private readonly AdvancedStats advanced;
        private readonly UserRatings ratings;

        private BeatmapInfo beatmap;
        public BeatmapInfo Beatmap
        {
            get { return beatmap; }
            set
            {
                if (value == beatmap) return;
                beatmap = value;

                basic.Beatmap = advanced.Beatmap = Beatmap;
                ratings.Metrics = Beatmap.Metrics;
            }
        }

        public Details(BeatmapSetInfo set)
        {
            Width = OnlineBeatmapSetOverlay.RIGHT_WIDTH;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(1f);

            Children = new Drawable[]
            {
                new AsyncLoadWrapper(new PreviewButton(set)
                {
                    RelativeSizeAxes = Axes.X,
                })
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
                new DetailBox
                {
                    Child = basic = new BasicStats(set)
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Vertical = 10 },
                    },
                },
                new DetailBox
                {
                    Child = advanced = new AdvancedStats
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Vertical = 7.5f },
                    },
                },
                new DetailBox
                {
                    Child = ratings = new UserRatings
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 95,
                        Margin = new MarginPadding { Top = 10 },
                    },
                },
            };
        }

        private class DetailBox : Container
        {
            private readonly Container content;
            protected override Container<Drawable> Content => content;

            public DetailBox()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.5f),
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = 15 },
                    },
                };
            }
        }
    }
}
