// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    /// <summary>
    /// Displays contents in a "pill".
    /// </summary>
    public partial class PillContainer : Container
    {
        private const float padding = 8;

        public readonly Drawable Background;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public PillContainer()
        {
            AutoSizeAxes = Axes.X;
            Height = 16;

            InternalChild = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Masking = true,
                Children = new[]
                {
                    Background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f
                    },
                    new GridContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = padding },
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize, minSize: 80 - 2 * padding)
                        },
                        Content = new[]
                        {
                            new[]
                            {
                                new Container
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Padding = new MarginPadding { Bottom = 2 },
                                    Child = content = new Container
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
