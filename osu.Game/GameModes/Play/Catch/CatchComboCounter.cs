//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.GameModes.Play.Osu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.GameModes.Play.Catch
{
    /// <summary>
    /// Similar to Standard, but without the 'x' and has tinted pop-ups. Used in osu!catch.
    /// </summary>
    public class CatchComboCounter : OsuComboCounter
    {
        protected override bool CanPopOutWhileRolling => true;

        protected override string FormatCount(ulong count)
        {
            return $@"{count:#,0}x";
        }

        public override void Roll(ulong newValue = 0)
        {
            PopOutSpriteText.Colour = CountSpriteText.Colour;

            base.Roll(newValue);
        }

        /// <summary>
        /// Increaces counter and tints pop-out before animation.
        /// </summary>
        /// <param name="colour">Last grabbed fruit colour.</param>
        public void CatchFruit(Color4 colour)
        {
            PopOutSpriteText.Colour = colour;
            Count++;
        }
    }
}
