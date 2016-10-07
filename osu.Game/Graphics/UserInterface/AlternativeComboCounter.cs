//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Allows tint and vertical scaling animation. Used in osu!taiko and osu!mania.
    /// </summary>
    public class AlternativeComboCounter : ULongCounter // btw, I'm terribly bad with names... OUENDAN!
    {
        public Color4 OriginalColour;
        public Color4 TintColour = Color4.OrangeRed;
        public int TintDuration = 500;
        public float ScaleFactor = 2;
        public EasingTypes TintEasing = EasingTypes.None;
        public bool CanAnimateWhenBackwards = false;

        public AlternativeComboCounter()
        {
            IsRollingContinuous = false;
        }

        public override void Load()
        {
            base.Load();
            countSpriteText.Hide();
            OriginalColour = Colour;
        }

        public override void ResetCount()
        {
            SetCountWithoutRolling(0);
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
                transformCount(new TranformULongCounter(Clock), currentValue, newValue);
        }

        protected override ulong GetProportionalDuration(ulong currentValue, ulong newValue)
        {
            ulong difference = currentValue > newValue ? currentValue - newValue : currentValue - newValue;
            return difference * RollingDuration;
        }

        protected virtual void transformAnimate()
        {
            countSpriteText.Colour = TintColour;
            countSpriteText.ScaleTo(new Vector2(1, ScaleFactor));
            countSpriteText.FadeColour(OriginalColour, TintDuration, TintEasing);
            countSpriteText.ScaleTo(new Vector2(1, 1), TintDuration, TintEasing);
        }

        protected override void transformVisibleCount(ulong currentValue, ulong newValue)
        {
            if (countSpriteText != null)
            {
                countSpriteText.Text = newValue.ToString("#,0");
                if (newValue == 0)
                {
                    countSpriteText.FadeOut(TintDuration);
                    return;
                }
                countSpriteText.Show();
                if (newValue > currentValue || CanAnimateWhenBackwards)
                {
                    transformAnimate();
                }
            }
        }
    }
}
