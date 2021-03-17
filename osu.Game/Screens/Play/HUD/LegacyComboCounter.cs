// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
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
        public Bindable<int> Current { get; } = new BindableInt { MinValue = 0, };

        private uint scheduledPopOutCurrentId;

        private const double pop_out_duration = 150;

        private const Easing pop_out_easing = Easing.None;

        private const double fade_out_duration = 100;

        /// <summary>
        /// Duration in milliseconds for the counter roll-up animation for each element.
        /// </summary>
        private const double rolling_duration = 20;

        private Drawable popOutCount;

        private Drawable displayedCountSpriteText;

        private int previousValue;

        private int displayedCount;

        private bool isRolling;

        [Resolved]
        private ISkinSource skin { get; set; }

        public LegacyComboCounter()
        {
            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Margin = new MarginPadding(10);

            Scale = new Vector2(1.2f);
        }

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual int DisplayedCount
        {
            get => displayedCount;
            private set
            {
                if (displayedCount.Equals(value))
                    return;

                if (isRolling)
                    onDisplayedCountRolling(displayedCount, value);
                else if (displayedCount + 1 == value)
                    onDisplayedCountIncrement(value);
                else
                    onDisplayedCountChange(value);

                displayedCount = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new[]
            {
                popOutCount = createSpriteText().With(s =>
                {
                    s.Alpha = 0;
                    s.Margin = new MarginPadding(0.05f);
                    s.Blending = BlendingParameters.Additive;
                }),
                displayedCountSpriteText = createSpriteText().With(s =>
                {
                    s.Alpha = 0;
                })
            };

            Current.ValueChanged += combo => updateCount(combo.NewValue == 0);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ((IHasText)displayedCountSpriteText).Text = formatCount(Current.Value);

            displayedCountSpriteText.Anchor = Anchor;
            displayedCountSpriteText.Origin = Origin;
            popOutCount.Origin = Origin;
            popOutCount.Anchor = Anchor;

            updateCount(false);
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
                isRolling = false;
                DisplayedCount = prev;

                if (prev + 1 == Current.Value)
                    onCountIncrement(prev, Current.Value);
                else
                    onCountChange(prev, Current.Value);
            }
            else
            {
                onCountRolling(displayedCount, Current.Value);
                isRolling = true;
            }
        }

        private void transformPopOut(int newValue)
        {
            ((IHasText)popOutCount).Text = formatCount(newValue);

            popOutCount.ScaleTo(1.6f);
            popOutCount.FadeTo(0.75f);
            popOutCount.MoveTo(Vector2.Zero);

            popOutCount.ScaleTo(1, pop_out_duration, pop_out_easing);
            popOutCount.FadeOut(pop_out_duration, pop_out_easing);
            popOutCount.MoveTo(displayedCountSpriteText.Position, pop_out_duration, pop_out_easing);
        }

        private void transformNoPopOut(int newValue)
        {
            ((IHasText)displayedCountSpriteText).Text = formatCount(newValue);

            displayedCountSpriteText.ScaleTo(1);
        }

        private void transformPopOutSmall(int newValue)
        {
            ((IHasText)displayedCountSpriteText).Text = formatCount(newValue);
            displayedCountSpriteText.ScaleTo(1.1f);
            displayedCountSpriteText.ScaleTo(1, pop_out_duration, pop_out_easing);
        }

        private void scheduledPopOutSmall(uint id)
        {
            // Too late; scheduled task invalidated
            if (id != scheduledPopOutCurrentId)
                return;

            DisplayedCount++;
        }

        private void onCountIncrement(int currentValue, int newValue)
        {
            scheduledPopOutCurrentId++;

            if (DisplayedCount < currentValue)
                DisplayedCount++;

            displayedCountSpriteText.Show();

            transformPopOut(newValue);

            uint newTaskId = scheduledPopOutCurrentId;

            Scheduler.AddDelayed(delegate
            {
                scheduledPopOutSmall(newTaskId);
            }, pop_out_duration);
        }

        private void onCountRolling(int currentValue, int newValue)
        {
            scheduledPopOutCurrentId++;

            // Hides displayed count if was increasing from 0 to 1 but didn't finish
            if (currentValue == 0 && newValue == 0)
                displayedCountSpriteText.FadeOut(fade_out_duration);

            transformRoll(currentValue, newValue);
        }

        private void onCountChange(int currentValue, int newValue)
        {
            scheduledPopOutCurrentId++;

            if (newValue == 0)
                displayedCountSpriteText.FadeOut();

            DisplayedCount = newValue;
        }

        private void onDisplayedCountRolling(int currentValue, int newValue)
        {
            if (newValue == 0)
                displayedCountSpriteText.FadeOut(fade_out_duration);
            else
                displayedCountSpriteText.Show();

            transformNoPopOut(newValue);
        }

        private void onDisplayedCountChange(int newValue)
        {
            displayedCountSpriteText.FadeTo(newValue == 0 ? 0 : 1);
            transformNoPopOut(newValue);
        }

        private void onDisplayedCountIncrement(int newValue)
        {
            displayedCountSpriteText.Show();
            transformPopOutSmall(newValue);
        }

        private void transformRoll(int currentValue, int newValue) =>
            this.TransformTo(nameof(DisplayedCount), newValue, getProportionalDuration(currentValue, newValue), Easing.None);

        private string formatCount(int count) => $@"{count}x";

        private double getProportionalDuration(int currentValue, int newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * rolling_duration;
        }

        private OsuSpriteText createSpriteText() => (OsuSpriteText)skin.GetDrawableComponent(new HUDSkinComponent(HUDSkinComponents.ComboText));
    }
}
