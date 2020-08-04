// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    /// <summary>
    /// A <see cref="StatisticDisplay"/> to display general numeric values.
    /// </summary>
    public class CounterStatistic : StatisticDisplay
    {
        private readonly int count;

        private RollingCounter<int> counter;

        /// <summary>
        /// Creates a new <see cref="CounterStatistic"/>.
        /// </summary>
        /// <param name="header">The name of the statistic.</param>
        /// <param name="count">The value to display.</param>
        public CounterStatistic(string header, int count)
            : base(header)
        {
            this.count = count;
        }

        public override void Appear()
        {
            base.Appear();
            counter.Current.Value = count;
        }

        protected override Drawable CreateContent() => counter = new Counter();

        private class Counter : RollingCounter<int>
        {
            protected override double RollingDuration => AccuracyCircle.ACCURACY_TRANSFORM_DURATION;

            protected override Easing RollingEasing => AccuracyCircle.ACCURACY_TRANSFORM_EASING;

            public Counter()
            {
                DisplayedCountSpriteText.Font = OsuFont.Torus.With(size: 20, fixedWidth: true);
                DisplayedCountSpriteText.Spacing = new Vector2(-2, 0);
            }

            public override void Increment(int amount)
                => Current.Value += amount;
        }
    }
}
