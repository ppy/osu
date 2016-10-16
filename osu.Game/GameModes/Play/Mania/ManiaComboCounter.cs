//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Game.GameModes.Play.Taiko;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.GameModes.Play.Mania
{
    /// <summary>
    /// Similar to osu!taiko, with a pop-out animation when failing (rolling). Used in osu!mania.
    /// </summary>
    public class ManiaComboCounter : TaikoComboCounter
    {
        protected Color4 OriginalColour;

        protected Color4 TintColour => Color4.OrangeRed;
        protected Color4 PopOutColor => Color4.Red;
        protected override float PopOutInitialAlpha => 1.0f;
        protected override ulong PopOutDuration => 300;

        public override void Load(BaseGame game)
        {
            base.Load(game);

            PopOutSpriteText.Anchor = Anchor.BottomCentre;
            PopOutSpriteText.Origin = Anchor.Centre;
            PopOutSpriteText.FadeColour(PopOutColor, 0);
            OriginalColour = Colour;
        }

        protected override void OnCountRolling(ulong currentValue, ulong newValue)
        {
            if (!IsRolling && newValue < currentValue)
            {
                PopOutSpriteText.Text = FormatCount(currentValue);

                PopOutSpriteText.FadeTo(PopOutInitialAlpha);
                PopOutSpriteText.ScaleTo(1.0f);

                PopOutSpriteText.FadeOut(PopOutDuration, PopOutEasing);
                PopOutSpriteText.ScaleTo(PopOutScale, PopOutDuration, PopOutEasing);
            }

            base.OnCountRolling(currentValue, newValue);
        }

        protected override void transformAnimate(ulong newValue)
        {
            base.transformAnimate(newValue);
            DisplayedCountSpriteText.FadeColour(TintColour, 0);
            DisplayedCountSpriteText.FadeColour(OriginalColour, AnimationDuration, AnimationEasing);
        }
    }
}
