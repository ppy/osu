// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A container for components displaying the current player health.
    /// Gets bound automatically to the <see cref="Rulesets.Scoring.HealthProcessor"/> when inserted to <see cref="DrawableRuleset.Overlays"/> hierarchy.
    /// </summary>
    public abstract partial class HealthDisplay : CompositeDrawable
    {
        private readonly Bindable<bool> showHealthBar = new Bindable<bool>(true);

        [Resolved]
        protected HealthProcessor HealthProcessor { get; private set; } = null!;

        protected virtual bool PlayInitialIncreaseAnimation => true;

        public Bindable<double> Current { get; } = new BindableDouble
        {
            MinValue = 0,
            MaxValue = 1
        };

        private BindableNumber<double> health = null!;

        protected bool InitialAnimationPlaying => initialIncrease != null;

        private ScheduledDelegate? initialIncrease;

        /// <summary>
        /// Triggered when a <see cref="Judgement"/> is a successful hit, signaling the health display to perform a flash animation (if designed to do so).
        /// Calls to this method are debounced.
        /// </summary>
        protected virtual void Flash()
        {
        }

        [Resolved]
        private HUDOverlay? hudOverlay { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            HealthProcessor.NewJudgement += onNewJudgement;

            // Don't bind directly so we can animate the startup procedure.
            health = HealthProcessor.Health.GetBoundCopy();

            if (hudOverlay != null)
                showHealthBar.BindTo(hudOverlay.ShowHealthBar);

            // this probably shouldn't be operating on `this.`
            showHealthBar.BindValueChanged(healthBar => this.FadeTo(healthBar.NewValue ? 1 : 0, HUDOverlay.FADE_DURATION, HUDOverlay.FADE_EASING), true);

            initialHealthValue = health.Value;

            if (PlayInitialIncreaseAnimation)
                startInitialAnimation();
            else
                Current.Value = health.Value;
        }

        private double lastValue;
        private double initialHealthValue;

        protected override void Update()
        {
            base.Update();

            if (!InitialAnimationPlaying || health.Value != initialHealthValue)
            {
                Current.Value = health.Value;

                if (initialIncrease != null)
                    FinishInitialAnimation(Current.Value);
            }

            // Health changes every frame in draining situations.
            // Manually handle value changes to avoid bindable event flow overhead.
            if (!Precision.AlmostEquals(lastValue, Current.Value, 0.001f))
            {
                HealthChanged(Current.Value > lastValue);
                lastValue = Current.Value;
            }
        }

        protected virtual void HealthChanged(bool increase)
        {
        }

        private void startInitialAnimation()
        {
            if (Current.Value >= health.Value)
                return;

            // TODO: this should run in gameplay time, including showing a larger increase when skipping.
            // TODO: it should also start increasing relative to the first hitobject.
            const double increase_delay = 150;

            initialIncrease = Scheduler.AddDelayed(() =>
            {
                double newValue = Math.Min(Current.Value + 0.05f, health.Value);
                this.TransformBindableTo(Current, newValue, increase_delay);
                Scheduler.AddOnce(Flash);

                if (newValue >= health.Value)
                    FinishInitialAnimation(health.Value);
            }, increase_delay, true);
        }

        protected virtual void FinishInitialAnimation(double value)
        {
            if (initialIncrease == null)
                return;

            initialIncrease.Cancel();
            initialIncrease = null;

            // aside from the repeating `initialIncrease` scheduled task,
            // there may also be a `Current` transform in progress from that schedule.
            // ensure it plays out fully, to prevent changes to `Current.Value` being discarded by the ongoing transform.
            // and yes, this funky `targetMember` spec is seemingly the only way to do this
            // (see: https://github.com/ppy/osu-framework/blob/fe2769171c6e26d1b6fdd6eb7ea8353162fe9065/osu.Framework/Graphics/Transforms/TransformBindable.cs#L21)
            FinishTransforms(targetMember: $"{Current.GetHashCode()}.{nameof(Current.Value)}");
        }

        private void onNewJudgement(JudgementResult judgement)
        {
            if (judgement.IsHit && judgement.Type != HitResult.IgnoreHit)
                Scheduler.AddOnce(Flash);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (HealthProcessor.IsNotNull())
                HealthProcessor.NewJudgement -= onNewJudgement;
        }
    }
}
