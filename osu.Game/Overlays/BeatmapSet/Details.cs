﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Details;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class Details : FillFlowContainer
    {
        private readonly PreviewButton preview;
        private readonly BasicStats basic;
        private readonly AdvancedStats advanced;
        private readonly UserRatings ratings;

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                basic.BeatmapSet = preview.BeatmapSet = BeatmapSet;
            }
        }

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

        public Details()
        {
            Width = BeatmapSetOverlay.RIGHT_WIDTH;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(1f);

            Children = new Drawable[]
            {
                preview = new PreviewButton
                {
                    RelativeSizeAxes = Axes.X,
                },
                new DetailBox
                {
                    Child = basic = new BasicStats
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

        public void StopPreview() => preview.Playing.Value = false;

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
