// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    /// <summary>
    /// A <see cref="StatisticDisplay"/> to display the player's accuracy.
    /// </summary>
    public class AccuracyStatistic : StatisticDisplay
    {
        private readonly double accuracy;

        private RollingCounter<double> counter;

        /// <summary>
        /// Creates a new <see cref="AccuracyStatistic"/>.
        /// </summary>
        /// <param name="accuracy">The accuracy to display.</param>
        public AccuracyStatistic(double accuracy)
            : base("accuracy")
        {
            this.accuracy = accuracy;
        }

        public override void Appear()
        {
            base.Appear();
            counter.Current.Value = accuracy;
        }

        protected override Drawable CreateContent() => counter = new Counter();

        private class Counter : RollingCounter<double>
        {
            protected override double RollingDuration => AccuracyCircle.ACCURACY_TRANSFORM_DURATION;

            protected override Easing RollingEasing => AccuracyCircle.ACCURACY_TRANSFORM_EASING;

            public Counter()
            {
                DisplayedCountSpriteText.Font = OsuFont.Torus.With(size: 20, fixedWidth: true);
                DisplayedCountSpriteText.Spacing = new Vector2(-2, 0);
            }

            protected override string FormatCount(double count) => count.FormatAccuracy();

            public override void Increment(double amount)
                => Current.Value += amount;
        }
    }
}
