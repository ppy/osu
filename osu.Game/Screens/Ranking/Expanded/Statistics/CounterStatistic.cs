// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
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

        protected RollingCounter<int> Counter { get; private set; }

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
            Counter.Current.Value = count;
        }

        protected override Drawable CreateContent() => Counter = new StatisticCounter();

        private class StatisticCounter : RollingCounter<int>
        {
            protected override double RollingDuration => AccuracyCircle.ACCURACY_TRANSFORM_DURATION;

            protected override Easing RollingEasing => AccuracyCircle.ACCURACY_TRANSFORM_EASING;

            protected override OsuSpriteText CreateSpriteText() => base.CreateSpriteText().With(s =>
            {
                s.Font = OsuFont.Torus.With(size: 20, fixedWidth: true);
                s.Spacing = new Vector2(-2, 0);
            });
        }
    }
}
