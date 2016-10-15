//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.GameModes.Play.Osu
{
    /// <summary>
    /// Uses the 'x' symbol and has a pop-out effect while rolling over. Used in osu! standard.
    /// </summary>
    public class OsuComboCounter : ComboCounter
    {
        protected uint ScheduledPopOutCurrentId = 0;

        protected virtual float PopOutSmallScale => 1.1f;
        protected virtual bool CanPopOutWhileRolling => false;
        

        public Vector2 InnerCountPosition
        {
            get
            {
                return CountSpriteText.Position;
            }
            set
            {
                CountSpriteText.Position = value;
            }
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            PopOutSpriteText.Origin = this.Origin;
            PopOutSpriteText.Anchor = this.Anchor;

            Add(PopOutSpriteText);
        }

        protected override string FormatCount(ulong count)
        {
            return $@"{count:#,0}x";
        }

        protected virtual void transformPopOut(ulong currentValue, ulong newValue)
        {
            PopOutSpriteText.Text = FormatCount(newValue);
            CountSpriteText.Text = FormatCount(currentValue);

            PopOutSpriteText.ScaleTo(PopOutScale);
            PopOutSpriteText.FadeTo(PopOutInitialAlpha);
            PopOutSpriteText.MoveTo(Vector2.Zero);

            PopOutSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
            PopOutSpriteText.FadeOut(PopOutDuration, PopOutEasing);
            PopOutSpriteText.MoveTo(CountSpriteText.Position, PopOutDuration, PopOutEasing);

            ScheduledPopOutCurrentId++;
            uint newTaskId = ScheduledPopOutCurrentId;
            Scheduler.AddDelayed(delegate
            {
                scheduledPopOutSmall(newTaskId, newValue);
            }, PopOutDuration);
        }

        protected virtual void transformNoPopOut(ulong newValue)
        {
            ScheduledPopOutCurrentId++;
            CountSpriteText.Text = FormatCount(newValue);
            CountSpriteText.ScaleTo(1);
        }

        protected virtual void transformPopOutSmall(ulong newValue)
        {
            CountSpriteText.Text = FormatCount(newValue);
            CountSpriteText.ScaleTo(PopOutSmallScale);
            CountSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
        }

        protected virtual void scheduledPopOutSmall(uint id, ulong newValue)
        {
            // Too late; scheduled task invalidated
            if (id != ScheduledPopOutCurrentId)
                return;

            transformPopOutSmall(newValue);
        }

        protected override void OnCountRolling(ulong currentValue, ulong newValue)
        {
            if (newValue == 0)
                CountSpriteText.FadeOut(PopOutDuration);
            else
                CountSpriteText.Show();

            if (CanPopOutWhileRolling)
                transformPopOut(currentValue, newValue);
            else
                transformNoPopOut(newValue);
        }

        protected override void OnCountChange(ulong newValue)
        {
            CountSpriteText.FadeTo(newValue == 0 ? 0 : 1);

            transformNoPopOut(newValue);
        }

        protected override void OnCountIncrement(ulong newValue)
        {
            CountSpriteText.Show();

            transformPopOut(newValue - 1, newValue);
        }
    }
}
