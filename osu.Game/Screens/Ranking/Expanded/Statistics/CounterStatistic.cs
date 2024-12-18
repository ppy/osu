// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    /// <summary>
    /// A <see cref="StatisticDisplay"/> to display general numeric values.
    /// </summary>
    public partial class CounterStatistic : StatisticDisplay
    {
        private readonly int count;
        private readonly int? maxCount;

        private RollingCounter<int> counter = null!;

        /// <summary>
        /// Creates a new <see cref="CounterStatistic"/>.
        /// </summary>
        /// <param name="header">The name of the statistic.</param>
        /// <param name="count">The value to display.</param>
        /// <param name="maxCount">The maximum value of <paramref name="count"/>. Not displayed if null.</param>
        public CounterStatistic(LocalisableString header, int count, int? maxCount = null)
            : base(header)
        {
            this.count = count;
            this.maxCount = maxCount;
        }

        public override void Appear()
        {
            base.Appear();
            counter.Current.Value = count;
        }

        protected override Drawable CreateContent()
        {
            var container = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Child = counter = new StatisticCounter
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                }
            };

            if (maxCount != null)
            {
                container.Add(new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Font = OsuFont.Torus.With(size: 12, fixedWidth: true),
                    Spacing = new Vector2(-2, 0),
                    Text = $"/{maxCount}"
                });
            }

            return container;
        }
    }
}
