// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;

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

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 6,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = ColourInfo.GradientVertical(
                            OsuColour.Gray(0.25f),
                            OsuColour.Gray(0.18f)
                        ),
                        Alpha = 0.95f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(5),
                        Children = new[]
                        {
                            LocalisableString.IsNullOrEmpty(item.Name)
                                ? Empty()
                                : new StatisticItemHeader { Text = item.Name },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding(10) { Top = 30 },
                                Child = item.CreateContent()
                            }
                        }
                    },
                }
            };
        }
    }
}
