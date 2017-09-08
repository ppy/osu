// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Overlays.OnlineBeatmapSet
{
    public class Header : Container
    {
        public Header(BeatmapSetInfo set)
        {
            RelativeSizeAxes = Axes.X;
            Height = 400;
            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.25f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                        },
                        new DelayedLoadWrapper(new BeatmapSetCover(set)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fill,
                            OnLoadComplete = d =>
                            {
                                d.FadeInFromZero(400, Easing.Out);
                            },
                        })
                        {
                            RelativeSizeAxes = Axes.Both,
                            TimeBeforeLoad = 300
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.3f), Color4.Black.Opacity(0.8f)),
                        },
                    },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Width = OnlineBeatmapSetOverlay.RIGHT_WIDTH,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Right = OnlineBeatmapSetOverlay.X_PADDING },
                    Spacing = new Vector2(1f),
                    Children = new Drawable[]
                    {
                        new DetailBox
                        {
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 42,
                            },
                        },
                        new DetailBox
                        {
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 35,
                            },
                        },
                        new DetailBox
                        {
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 110,
                            },
                        },
                        new DetailBox
                        {
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 115,
                            },
                        },
                    },
                },
            };
        }

        private class DetailBox : Container
        {
            private Container content;
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
