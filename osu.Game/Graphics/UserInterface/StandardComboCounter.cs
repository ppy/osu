//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Timing;
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
        public float PopOutSmallScale = 1.1f;
        public EasingTypes PopOutEasing = EasingTypes.None;
        public bool CanPopOutWhenBackwards = false;
        public float PopOutInitialAlpha = 0.75f;

        public Vector2 InnerCountPosition
        {
            get
            {
                return countSpriteText.Position;
            }
            set
            {
                countSpriteText.Position = value;
            }
        }

        public StandardComboCounter() : base()
        {
            IsRollingContinuous = false;

            countSpriteText.Alpha = 0;

            popOutSpriteText = new SpriteText
            {
                Origin = this.Origin,
                Anchor = this.Anchor,
                TextSize = this.TextSize,
                Alpha = 0,
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            popOutSpriteText.Origin = this.Origin;
            popOutSpriteText.Anchor = this.Anchor;

            Add(popOutSpriteText);
        }

        protected override void updateTextSize()
        {
            base.updateTextSize();

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

        protected virtual void transformPopOut(ulong newValue)
        {
            popOutSpriteText.ScaleTo(PopOutBigScale);
            popOutSpriteText.FadeTo(PopOutInitialAlpha);
            popOutSpriteText.Position = Vector2.Zero;

            popOutSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
            popOutSpriteText.FadeOut(PopOutDuration, PopOutEasing);
            popOutSpriteText.MoveTo(countSpriteText.Position, PopOutDuration, PopOutEasing);

            Scheduler.AddDelayed(delegate
            {
                transformPopOutNew(newValue);
            }, PopOutDuration);
        }

        protected virtual void transformPopOutNew(ulong newValue)
        {   
            // Too late; scheduled task invalidated
            if (newValue != VisibleCount)
                return;

            countSpriteText.Text = formatCount(newValue);
            countSpriteText.ScaleTo(PopOutSmallScale);
            countSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
        }

        protected override void transformVisibleCount(ulong currentValue, ulong newValue)
        {
            popOutSpriteText.Text = formatCount(newValue);
            if (newValue > currentValue)
            {
                countSpriteText.Text = formatCount(newValue - 1);
                
            }
            else
            {
                countSpriteText.Text = formatCount(newValue);
            }
            if (newValue == 0)
            {
                countSpriteText.FadeOut(PopOutDuration);
            }
            else
            {
                countSpriteText.Show();
                if (newValue > currentValue || CanPopOutWhenBackwards)
                    transformPopOut(newValue);
            }
        }
    }
}
