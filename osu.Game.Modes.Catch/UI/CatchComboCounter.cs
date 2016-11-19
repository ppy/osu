//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Osu.UI;
using OpenTK.Graphics;

namespace osu.Game.Modes.Catch.UI
{
    /// <summary>
    /// Similar to Standard, but without the 'x' and has tinted pop-ups. Used in osu!catch.
    /// </summary>
    public class CatchComboCounter : OsuComboCounter
    {
        protected override bool CanPopOutWhileRolling => true;

        protected virtual double FadeOutDelay => 1000;
        protected override double FadeOutDuration => 300;

        protected override string FormatCount(ulong count)
        {
            return $@"{count:#,0}";
        }

        private void animateFade()
        {
            Show();
            Delay(FadeOutDelay);
            FadeOut(FadeOutDuration);
            DelayReset();
        }

        protected override void OnCountChange(ulong currentValue, ulong newValue)
        {
            if (newValue != 0)
                animateFade();
            base.OnCountChange(currentValue, newValue);
        }

        protected override void OnCountRolling(ulong currentValue, ulong newValue)
        {
            if (!IsRolling)
            {
                PopOutCount.Colour = DisplayedCountSpriteText.Colour;
                FadeOut(FadeOutDuration);
            }
            base.OnCountRolling(currentValue, newValue);
        }

        protected override void OnCountIncrement(ulong currentValue, ulong newValue)
        {
            animateFade();
            base.OnCountIncrement(currentValue, newValue);
        }

        /// <summary>
        /// Increaces counter and tints pop-out before animation.
        /// </summary>
        /// <param name="colour">Last grabbed fruit colour.</param>
        public void CatchFruit(Color4 colour)
        {
            PopOutCount.Colour = colour;
            Count++;
        }
    }
}
