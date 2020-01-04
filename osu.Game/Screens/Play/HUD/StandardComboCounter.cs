// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Uses the 'x' symbol and has a pop-out effect while rolling over.
    /// </summary>
    public class StandardComboCounter : ComboCounter
    {
        protected uint ScheduledPopOutCurrentId;

        protected virtual float PopOutSmallScale => 1.1f;
        protected virtual bool CanPopOutWhileRolling => false;

        public new Vector2 PopOutScale = new Vector2(1.6f);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PopOutCount.Origin = Origin;
            PopOutCount.Anchor = Anchor;
        }

        protected override string FormatCount(int count)
        {
            return $@"{count}x";
        }

        protected virtual void TransformPopOut(int newValue)
        {
            PopOutCount.Text = FormatCount(newValue);

            PopOutCount.ScaleTo(PopOutScale);
            PopOutCount.FadeTo(PopOutInitialAlpha);
            PopOutCount.MoveTo(Vector2.Zero);

            PopOutCount.ScaleTo(1, PopOutDuration, PopOutEasing);
            PopOutCount.FadeOut(PopOutDuration, PopOutEasing);
            PopOutCount.MoveTo(DisplayedCountSpriteText.Position, PopOutDuration, PopOutEasing);
        }

        protected virtual void TransformPopOutRolling(int newValue)
        {
            TransformPopOut(newValue);
            TransformPopOutSmall(newValue);
        }

        protected virtual void TransformNoPopOut(int newValue)
        {
            DisplayedCountSpriteText.Text = FormatCount(newValue);
            DisplayedCountSpriteText.ScaleTo(1);
        }

        protected virtual void TransformPopOutSmall(int newValue)
        {
            DisplayedCountSpriteText.Text = FormatCount(newValue);
            DisplayedCountSpriteText.ScaleTo(PopOutSmallScale);
            DisplayedCountSpriteText.ScaleTo(1, PopOutDuration, PopOutEasing);
        }

        protected virtual void ScheduledPopOutSmall(uint id)
        {
            // Too late; scheduled task invalidated
            if (id != ScheduledPopOutCurrentId)
                return;

            DisplayedCount++;
        }

        protected override void OnCountRolling(int currentValue, int newValue)
        {
            ScheduledPopOutCurrentId++;

            // Hides displayed count if was increasing from 0 to 1 but didn't finish
            if (currentValue == 0 && newValue == 0)
                DisplayedCountSpriteText.FadeOut(FadeOutDuration);

            base.OnCountRolling(currentValue, newValue);
        }

        protected override void OnCountIncrement(int currentValue, int newValue)
        {
            ScheduledPopOutCurrentId++;

            if (DisplayedCount < currentValue)
                DisplayedCount++;

            DisplayedCountSpriteText.Show();

            TransformPopOut(newValue);

            uint newTaskId = ScheduledPopOutCurrentId;
            Scheduler.AddDelayed(delegate
            {
                ScheduledPopOutSmall(newTaskId);
            }, PopOutDuration);
        }

        protected override void OnCountChange(int currentValue, int newValue)
        {
            ScheduledPopOutCurrentId++;

            if (newValue == 0)
                DisplayedCountSpriteText.FadeOut();

            base.OnCountChange(currentValue, newValue);
        }

        protected override void OnDisplayedCountRolling(int currentValue, int newValue)
        {
            if (newValue == 0)
                DisplayedCountSpriteText.FadeOut(FadeOutDuration);
            else
                DisplayedCountSpriteText.Show();

            if (CanPopOutWhileRolling)
                TransformPopOutRolling(newValue);
            else
                TransformNoPopOut(newValue);
        }

        protected override void OnDisplayedCountChange(int newValue)
        {
            DisplayedCountSpriteText.FadeTo(newValue == 0 ? 0 : 1);

            TransformNoPopOut(newValue);
        }

        protected override void OnDisplayedCountIncrement(int newValue)
        {
            DisplayedCountSpriteText.Show();

            TransformPopOutSmall(newValue);
        }
    }
}
