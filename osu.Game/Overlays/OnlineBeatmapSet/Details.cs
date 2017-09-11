// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.OnlineBeatmapSet
{
    public class Details : FillFlowContainer
    {
        public Details()
        {
            Width = OnlineBeatmapSetOverlay.RIGHT_WIDTH;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(1f);

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
