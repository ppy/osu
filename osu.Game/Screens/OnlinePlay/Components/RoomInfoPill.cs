// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Components
{
    /// <summary>
    /// Abstract class for "pill" components displayed as part of <see cref="DrawableRoom"/>s.
    /// </summary>
    public abstract class RoomInfoPill : OnlinePlayComposite
    {
        private const float padding = 8;

        protected Drawable Background { get; private set; }

        protected RoomInfoPill()
        {
            AutoSizeAxes = Axes.X;
            Height = 16;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = padding },
                        Child = new GridContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize, minSize: 80 - 2 * padding)
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    CreateContent().With(d =>
                                    {
                                        d.Anchor = Anchor.Centre;
                                        d.Origin = Anchor.Centre;
                                    })
                                }
                            }
                        }
                    }
                }
            };
        }

        protected abstract Drawable CreateContent();
    }
}
