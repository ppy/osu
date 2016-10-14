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
    /// Allows tint and vertical scaling animation. Used in osu!taiko and osu!mania.
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

        public override void Roll(ulong newValue = 0)
        {
            if (!IsRolling)
            {
                PopOutSpriteText.Text = FormatCount(VisibleCount);

                PopOutSpriteText.FadeTo(PopOutInitialAlpha);
                PopOutSpriteText.ScaleTo(1.0f);

                PopOutSpriteText.FadeOut(PopOutDuration, PopOutEasing);
                PopOutSpriteText.ScaleTo(PopOutScale, PopOutDuration, PopOutEasing);
            }

            base.Roll(newValue);
        }

        protected override void transformAnimate(ulong newValue)
        {
            base.transformAnimate(newValue);
            CountSpriteText.FadeColour(TintColour, 0);
            CountSpriteText.FadeColour(OriginalColour, AnimationDuration, AnimationEasing);
        }
    }
}
