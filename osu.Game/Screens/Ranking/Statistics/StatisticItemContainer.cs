// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// Wraps a <see cref="StatisticItem"/> to add a header and suitable layout for use in <see cref="ResultsScreen"/>.
    /// </summary>
    internal partial class StatisticItemContainer : CompositeDrawable
    {
        /// <summary>
        /// Creates a new <see cref="StatisticItemContainer"/>.
        /// </summary>
        /// <param name="item">The <see cref="StatisticItem"/> to display.</param>
        public StatisticItemContainer(StatisticItem item)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding(5);
            Width = item.FullWidth ? 1 : 0.5f;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 20,
                CornerExponent = 2.5f,
                Children = new[]
                {
                    new Box
                    {
                        Colour = ColourInfo.GradientVertical(
                            OsuColour.Gray(0.25f).Opacity(0.8f),
                            OsuColour.Gray(0.18f).Opacity(0.95f)
                        ),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(2),
                        Children = new[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding(15) { Top = 40 },
                                Child = item.CreateContent()
                            },
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = ColourInfo.GradientVertical(
                                    OsuColour.Gray(0.25f),
                                    OsuColour.Gray(0.25f).Opacity(0)
                                ),
                                RelativeSizeAxes = Axes.X,
                                Height = 30,
                            },
                            new Box
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Colour = ColourInfo.GradientVertical(
                                    OsuColour.Gray(0.18f).Opacity(0),
                                    OsuColour.Gray(0.18f)
                                ),
                                RelativeSizeAxes = Axes.X,
                                Height = 20,
                            },
                        }
                    },
                    LocalisableString.IsNullOrEmpty(item.Name)
                        ? Empty()
                        : new StatisticItemHeader
                        {
                            Text = item.Name
                        },
                }
            };

            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 20,
                CornerExponent = 2.5f,
                EdgeEffect = new EdgeEffectParameters
                {
                    Radius = 2,
                    Hollow = true,
                    Colour = OsuColour.Gray(0.18f),
                    Type = EdgeEffectType.Shadow,
                },
                Children = new[]
                {
                    new Box
                    {
                        Colour = Color4.Transparent,
                        RelativeSizeAxes = Axes.Both,
                    },
                }
            });
        }
    }
}
