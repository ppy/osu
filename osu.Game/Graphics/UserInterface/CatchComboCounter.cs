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

        protected override void transformCount(ulong currentValue, ulong newValue)
        {
            // Animate rollover only when going backwards
            if (newValue > currentValue)
            {
                updateTransforms(typeof(TranformULongCounter));
                removeTransforms(typeof(TranformULongCounter));
                VisibleCount = newValue;
            }
            else
            {
                // Backwards pop-up animation has no tint colour
                popOutSpriteText.Colour = countSpriteText.Colour;
                transformCount(new TranformULongCounter(Clock), currentValue, newValue);
            }
        }

        /// <summary>
        /// Tints pop-out before animation. Intended to use the last grabbed fruit colour.
        /// </summary>
        /// <param name="colour"></param>
        public void CatchFruit(Color4 colour)
        {
            popOutSpriteText.Colour = colour;
            Count++;
        }

        public override void ResetCount()
        {
            base.ResetCount();
        }
    }
}
