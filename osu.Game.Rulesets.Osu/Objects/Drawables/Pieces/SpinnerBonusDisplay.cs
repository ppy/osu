// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    /// <summary>
    /// Shows incremental bonus score achieved for a spinner.
    /// </summary>
    public class SpinnerBonusDisplay : CompositeDrawable
    {
        private readonly OsuSpriteText bonusCounter;

        public SpinnerBonusDisplay()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = bonusCounter = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Numeric.With(size: 24),
                Alpha = 0,
            };
        }

        private int displayedCount;

        public void SetBonusCount(int count)
        {
            if (displayedCount == count)
                return;

            displayedCount = count;
            bonusCounter.Text = $"{SpinnerBonusTick.SCORE_PER_TICK * count}";
            bonusCounter.FadeOutFromOne(1500);
            bonusCounter.ScaleTo(1.5f).Then().ScaleTo(1f, 1000, Easing.OutQuint);
        }
    }
}
