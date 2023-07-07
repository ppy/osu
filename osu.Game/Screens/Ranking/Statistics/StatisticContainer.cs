// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// Wraps a <see cref="StatisticItem"/> to add a header and suitable layout for use in <see cref="ResultsScreen"/>.
    /// </summary>
    internal partial class StatisticContainer : CompositeDrawable
    {
        /// <summary>
        /// Creates a new <see cref="StatisticContainer"/>.
        /// </summary>
        /// <param name="item">The <see cref="StatisticItem"/> to display.</param>
        public StatisticContainer(StatisticItem item)
        {
            RelativeSizeAxes = Axes.Both;
            Size = item.RelativeSize;

            InternalChildren = new[]
            {
                createHeader(item),
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 15 },
                    Masking = true,
                    Child = item.CreateContent().With(d => d.RelativeSizeAxes = Axes.Both)
                }
            };
        }

        private static Drawable createHeader(StatisticItem item)
        {
            if (LocalisableString.IsNullOrEmpty(item.Name))
                return Empty();

            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = 15,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    new Circle
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Height = 9,
                        Width = 4,
                        Colour = Color4Extensions.FromHex("#00FFAA")
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = item.Name,
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                    }
                }
            };
        }
    }
}
