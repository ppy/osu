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
        protected SpriteText popOutSpriteText;

        protected volatile int scheduledPopOutCurrentId = 0;

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
            else if (currentValue != 0)
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

        protected virtual void transformPopOut(ulong currentValue, ulong newValue)
        {
            popOutSpriteText.Text = formatCount(newValue);
            countSpriteText.Text = formatCount(currentValue);

            popOutSpriteText.ScaleTo(PopOutBigScale);
            popOutSpriteText.FadeTo(PopOutInitialAlpha);
            popOutSpriteText.MoveTo(Vector2.Zero);

            popOutSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
            popOutSpriteText.FadeOut(PopOutDuration, PopOutEasing);
            popOutSpriteText.MoveTo(countSpriteText.Position, PopOutDuration, PopOutEasing);

            scheduledPopOutCurrentId++;
            int newTaskId = scheduledPopOutCurrentId;
            Scheduler.AddDelayed(delegate
            {
                scheduledPopOutSmall(newTaskId, newValue);
            }, PopOutDuration);
        }

        protected virtual void transformNoPopOut(ulong newValue)
        {
            scheduledPopOutCurrentId++;
            countSpriteText.Text = formatCount(newValue);
            countSpriteText.ScaleTo(1);
        }

        protected virtual void transformPopOutSmall(ulong newValue)
        {
            countSpriteText.Text = formatCount(newValue);
            countSpriteText.ScaleTo(PopOutSmallScale);
            countSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
        }

        protected virtual void scheduledPopOutSmall(int id, ulong newValue)
        {
            // Too late; scheduled task invalidated
            if (id != scheduledPopOutCurrentId)
                return;

            transformPopOutSmall(newValue);
        }

        protected override void transformVisibleCount(ulong currentValue, ulong newValue)
        {
            if (newValue == 0)
                countSpriteText.FadeOut(PopOutDuration);
            else
                countSpriteText.Show();

            if (newValue > currentValue || CanPopOutWhenBackwards)
                transformPopOut(currentValue, newValue);
            else
                transformNoPopOut(newValue);
        }
    }
}
