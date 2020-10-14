// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Uses the 'x' symbol and has a pop-out effect while rolling over.
    /// </summary>
    public class LegacyComboCounter : CompositeDrawable, IComboCounter
    {
        protected uint ScheduledPopOutCurrentId;

        protected virtual float PopOutSmallScale => 1.1f;
        protected virtual bool CanPopOutWhileRolling => false;

        protected Drawable PopOutCount;
        protected Drawable DisplayedCountSpriteText;
        private int previousValue;
        private int displayedCount;

        public LegacyComboCounter()
        {
            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Margin = new MarginPadding { Bottom = 20, Left = 20 };

            Scale = new Vector2(1.6f);
        }

        [Resolved]
        private ISkinSource skin { get; set; }

        public Bindable<int> Current { get; } = new BindableInt
        {
            MinValue = 0,
        };

        public bool IsRolling { get; protected set; }
        protected virtual double PopOutDuration => 150;
        protected virtual float PopOutScale => 1.6f;
        protected virtual Easing PopOutEasing => Easing.None;
        protected virtual float PopOutInitialAlpha => 0.75f;
        protected virtual double FadeOutDuration => 100;

        /// <summary>
        /// Duration in milliseconds for the counter roll-up animation for each element.
        /// </summary>
        protected virtual double RollingDuration => 20;

        /// <summary>
        /// Easing for the counter rollover animation.
        /// </summary>
        protected Easing RollingEasing => Easing.None;

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual int DisplayedCount
        {
            get => displayedCount;
            protected set
            {
                if (displayedCount.Equals(value))
                    return;

                updateDisplayedCount(displayedCount, value, IsRolling);
            }
        }

        protected Drawable CreateSpriteText()
        {
            return skin?.GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.ScoreText)) ?? new OsuSpriteText
            {
                Font = OsuFont.Numeric.With(size: 40),
                UseFullGlyphHeight = false,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                DisplayedCountSpriteText = CreateSpriteText().With(s =>
                {
                    s.Alpha = 0;
                }),
                PopOutCount = CreateSpriteText().With(s =>
                {
                    s.Alpha = 0;
                    s.Margin = new MarginPadding(0.05f);
                })
            };

            Current.ValueChanged += combo => updateCount(combo.NewValue == 0);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ((IHasText)DisplayedCountSpriteText).Text = FormatCount(Current.Value);

            DisplayedCountSpriteText.Anchor = Anchor;
            DisplayedCountSpriteText.Origin = Origin;
            PopOutCount.Origin = Origin;
            PopOutCount.Anchor = Anchor;

            StopRolling();
        }

        protected virtual void TransformPopOut(int newValue)
        {
            ((IHasText)PopOutCount).Text = FormatCount(newValue);

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
            ((IHasText)DisplayedCountSpriteText).Text = FormatCount(newValue);
            DisplayedCountSpriteText.ScaleTo(1);
        }

        protected virtual void TransformPopOutSmall(int newValue)
        {
            ((IHasText)DisplayedCountSpriteText).Text = FormatCount(newValue);
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

        protected void OnCountRolling(int currentValue, int newValue)
        {
            ScheduledPopOutCurrentId++;

            // Hides displayed count if was increasing from 0 to 1 but didn't finish
            if (currentValue == 0 && newValue == 0)
                DisplayedCountSpriteText.FadeOut(FadeOutDuration);

            transformRoll(currentValue, newValue);
        }

        protected void OnCountIncrement(int currentValue, int newValue)
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

        protected void OnCountChange(int currentValue, int newValue)
        {
            ScheduledPopOutCurrentId++;

            if (newValue == 0)
                DisplayedCountSpriteText.FadeOut();

            DisplayedCount = newValue;
        }

        protected void OnDisplayedCountRolling(int currentValue, int newValue)
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

        protected void OnDisplayedCountChange(int newValue)
        {
            DisplayedCountSpriteText.FadeTo(newValue == 0 ? 0 : 1);

            TransformNoPopOut(newValue);
        }

        protected void OnDisplayedCountIncrement(int newValue)
        {
            DisplayedCountSpriteText.Show();

            TransformPopOutSmall(newValue);
        }

        /// <summary>
        /// Increments the combo by an amount.
        /// </summary>
        /// <param name="amount"></param>
        public void Increment(int amount = 1)
        {
            Current.Value += amount;
        }

        /// <summary>
        /// Stops rollover animation, forcing the displayed count to be the actual count.
        /// </summary>
        public void StopRolling()
        {
            updateCount(false);
        }

        protected string FormatCount(int count)
        {
            return $@"{count}x";
        }

        private double getProportionalDuration(int currentValue, int newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * RollingDuration;
        }

        private void updateDisplayedCount(int currentValue, int newValue, bool rolling)
        {
            displayedCount = newValue;
            if (rolling)
                OnDisplayedCountRolling(currentValue, newValue);
            else if (currentValue + 1 == newValue)
                OnDisplayedCountIncrement(newValue);
            else
                OnDisplayedCountChange(newValue);
        }

        private void updateCount(bool rolling)
        {
            int prev = previousValue;
            previousValue = Current.Value;

            if (!IsLoaded)
                return;

            if (!rolling)
            {
                FinishTransforms(false, nameof(DisplayedCount));
                IsRolling = false;
                DisplayedCount = prev;

                if (prev + 1 == Current.Value)
                    OnCountIncrement(prev, Current.Value);
                else
                    OnCountChange(prev, Current.Value);
            }
            else
            {
                OnCountRolling(displayedCount, Current.Value);
                IsRolling = true;
            }
        }

        private void transformRoll(int currentValue, int newValue)
        {
            this.TransformTo<LegacyComboCounter, int>(nameof(DisplayedCount), newValue, getProportionalDuration(currentValue, newValue), RollingEasing);
        }
    }
}
