//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Uses the 'x' symbol and has a pop-out effect while rolling over. Used in osu! standard.
    /// </summary>
    public class StandardComboCounter : ULongCounter
    {
        public SpriteText popOutSpriteText;

        public ulong PopOutDuration = 0;
        public float PopOutBigScale = 2.0f;
        public float PopOutSmallScale = 1.2f;
        public EasingTypes PopOutEasing = EasingTypes.None;
        public bool CanPopOutWhenBackwards = false;
        public float PopOutInitialAlpha = 0.75f;

        public StandardComboCounter() : base()
        {
            IsRollingContinuous = false;
        }

        public override void Load()
        {
            base.Load();
            countSpriteText.Alpha = 0;
            Add(popOutSpriteText = new SpriteText
            {
                Text = formatCount(Count),
                Origin = this.Origin,
                Anchor = this.Anchor,
                TextSize = this.TextSize,
                Alpha = 0,
            });
        }

        protected override void updateTextSize()
        {
            base.updateTextSize();
            if (popOutSpriteText != null)
                popOutSpriteText.TextSize = this.TextSize;
        }


        protected override void transformCount(ulong currentValue, ulong newValue)
        {
            // Animate rollover only when going backwards
            if (newValue > currentValue)
            {
                updateTransforms(typeof(TransformULongCounter));
                removeTransforms(typeof(TransformULongCounter));
                VisibleCount = newValue;
            }
            else
                transformCount(new TransformULongCounter(Clock), currentValue, newValue);
        }

        protected override ulong getProportionalDuration(ulong currentValue, ulong newValue)
        {
            ulong difference = currentValue > newValue ? currentValue - newValue : currentValue - newValue;
            return difference * RollingDuration;
        }

        protected override string formatCount(ulong count)
        {
            return count.ToString("#,0") + "x";
        }

        protected virtual void transformPopOut()
        {
            countSpriteText.ScaleTo(PopOutSmallScale);
            countSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);

            popOutSpriteText.ScaleTo(PopOutBigScale);
            popOutSpriteText.FadeTo(PopOutInitialAlpha);
            popOutSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
            popOutSpriteText.FadeOut(PopOutDuration, PopOutEasing);
        }

        protected override void transformVisibleCount(ulong currentValue, ulong newValue)
        {
            if (countSpriteText != null && popOutSpriteText != null)
            {
                countSpriteText.Text = popOutSpriteText.Text = formatCount(newValue);
                if (newValue == 0)
                {
                    countSpriteText.FadeOut(PopOutDuration);
                }
                else
                {
                    countSpriteText.Show();
                    if (newValue > currentValue || CanPopOutWhenBackwards)
                        transformPopOut();
                }
            }
        }
    }
}
