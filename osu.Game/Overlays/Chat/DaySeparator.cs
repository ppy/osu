// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Chat
{
    public partial class DaySeparator : Container
    {
        protected virtual float TextSize => 15;

        protected virtual float LineHeight => 2;

        protected virtual float DateAlign => 205;

        protected virtual float Spacing => 15;

        public readonly DateTimeOffset Date;

        [Resolved(CanBeNull = true)]
        private OverlayColourProvider? colourProvider { get; set; }

        public DaySeparator(DateTimeOffset date)
        {
            Date = date;
            Height = 40;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RowDimensions = new[] { new Dimension() },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, DateAlign),
                    new Dimension(GridSizeMode.Absolute, Spacing),
                    new Dimension(),
                },
                Content = new[]
                {
                    new[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[] { new Dimension() },
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, Spacing),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    new Circle
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.X,
                                        Height = LineHeight,
                                        Colour = colourProvider?.Background5 ?? Colour4.White,
                                    },
                                    Drawable.Empty(),
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Text = Date.ToLocalTime().ToLocalisableString(@"dd MMMM yyyy").ToUpper(),
                                        Font = OsuFont.Torus.With(size: TextSize, weight: FontWeight.SemiBold),
                                        Colour = colourProvider?.Content1 ?? Colour4.White,
                                    },
                                }
                            },
                        },
                        Drawable.Empty(),
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            Height = LineHeight,
                            Colour = colourProvider?.Background5 ?? Colour4.White,
                        },
                    }
                }
            };
        }
    }
}
