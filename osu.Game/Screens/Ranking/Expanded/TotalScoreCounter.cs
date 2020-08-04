// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded
{
    /// <summary>
    /// A counter for the player's total score to be displayed in the <see cref="ExpandedPanelMiddleContent"/>.
    /// </summary>
    public class TotalScoreCounter : RollingCounter<long>
    {
        protected override double RollingDuration => AccuracyCircle.ACCURACY_TRANSFORM_DURATION;

        protected override Easing RollingEasing => AccuracyCircle.ACCURACY_TRANSFORM_EASING;

        public TotalScoreCounter()
        {
            // Todo: AutoSize X removed here due to https://github.com/ppy/osu-framework/issues/3369
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            DisplayedCountSpriteText.Anchor = Anchor.TopCentre;
            DisplayedCountSpriteText.Origin = Anchor.TopCentre;

            DisplayedCountSpriteText.Font = OsuFont.Torus.With(size: 60, weight: FontWeight.Light, fixedWidth: true);
            DisplayedCountSpriteText.Spacing = new Vector2(-5, 0);
        }

        protected override string FormatCount(long count) => count.ToString("N0");

        public override void Increment(long amount)
            => Current.Value += amount;
    }
}
