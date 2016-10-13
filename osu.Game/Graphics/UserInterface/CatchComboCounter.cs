//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Similar to Standard, but without the 'x' and has tinted pop-ups. Used in osu!catch.
    /// </summary>
    public class CatchComboCounter : StandardComboCounter
    {
        public CatchComboCounter()
        {
            CanPopOutWhenBackwards = true;
        }

        protected override string formatCount(ulong count)
        {
            return count.ToString("#,0");
        }

        public override void RollBack(ulong newValue = 0)
        {
            popOutSpriteText.Colour = countSpriteText.Colour;

            base.RollBack(newValue);
        }

        /// <summary>
        /// Increaces counter and tints pop-out before animation.
        /// </summary>
        /// <param name="colour">Last grabbed fruit colour.</param>
        public void CatchFruit(Color4 colour)
        {
            popOutSpriteText.Colour = colour;
            Count++;
        }
    }
}
