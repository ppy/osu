//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.Taiko.UI;
using OpenTK.Graphics;

namespace osu.Game.Modes.Mania.UI
{
    /// <summary>
    /// Similar to osu!taiko, with a pop-out animation when failing (rolling). Used in osu!mania.
    /// </summary>
    public class ManiaComboCounter : TaikoComboCounter
    {
        protected ushort KeysHeld = 0;

        protected Color4 OriginalColour;

        protected Color4 TintColour => Color4.Orange;
        protected EasingTypes TintEasing => EasingTypes.None;
        protected int TintDuration => 500;

        protected Color4 PopOutColor => Color4.Red;
        protected override float PopOutInitialAlpha => 1.0f;
        protected override double PopOutDuration => 300;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PopOutCount.Anchor = Anchor.BottomCentre;
            PopOutCount.Origin = Anchor.Centre;
            PopOutCount.FadeColour(PopOutColor, 0);
            OriginalColour = DisplayedCountSpriteText.Colour;
        }

        protected override void OnCountRolling(ulong currentValue, ulong newValue)
        {
            if (!IsRolling && newValue < currentValue)
            {
                PopOutCount.Text = FormatCount(currentValue);

                PopOutCount.FadeTo(PopOutInitialAlpha);
                PopOutCount.ScaleTo(1.0f);

                PopOutCount.FadeOut(PopOutDuration, PopOutEasing);
                PopOutCount.ScaleTo(PopOutScale, PopOutDuration, PopOutEasing);
            }

            base.OnCountRolling(currentValue, newValue);
        }

        /// <summary>
        /// Tints text while holding a key.
        /// </summary>
        /// <remarks>
        /// Does not alter combo. This has to be done depending of the scoring system.
        /// (i.e. v1 = each period of time; v2 = when starting and ending a key hold)
        /// </remarks>
        public void HoldStart()
        {
            if (KeysHeld == 0)
                DisplayedCountSpriteText.FadeColour(TintColour, TintDuration, TintEasing);
            KeysHeld++;
        }

        /// <summary>
        /// Ends tinting.
        /// </summary>
        public void HoldEnd()
        {
            KeysHeld--;
            if (KeysHeld == 0)
                DisplayedCountSpriteText.FadeColour(OriginalColour, TintDuration, TintEasing);
        }
    }
}
