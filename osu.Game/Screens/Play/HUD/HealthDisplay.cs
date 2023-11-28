// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
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

        private double initialHealthAnimationValue;

        protected double InitialHealthAnimationValue
        {
            get => initialHealthAnimationValue;
            set
            {
                initialHealthAnimationValue = value;
                setCurrent(value);
            }
        }

        private ScheduledDelegate? initialIncrease;

        private IBindableNumber<double> health = null!;

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
            HealthProcessor.HealthChanged += onHealthChanged;

            // Don't bind directly so we can animate the startup procedure.
            health = HealthProcessor.Health.GetBoundCopy();

            if (hudOverlay != null)
                showHealthBar.BindTo(hudOverlay.ShowHealthBar);

            // this probably shouldn't be operating on `this.`
            showHealthBar.BindValueChanged(healthBar => this.FadeTo(healthBar.NewValue ? 1 : 0, HUDOverlay.FADE_DURATION, HUDOverlay.FADE_EASING), true);

            if (PlayInitialIncreaseAnimation)
                startInitialAnimation();
            else
                Current.Value = health.Value;
        }

        protected virtual void OnHealthChanged(double newValue, double oldValue, JudgementResult? result = null)
        {
        }

        private void onHealthChanged(double newValue, double _, JudgementResult? result = null)
        {
            finishInitialAnimation();
            setCurrent(newValue, result);

            if (result?.IsHit == true && result.Type != HitResult.IgnoreHit)
                Scheduler.AddOnce(Flash);
        }

        private void setCurrent(double value, JudgementResult? result = null)
        {
            double oldValue = Current.Value;
            Current.Value = value;

            OnHealthChanged(value, oldValue, result);
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
                this.TransformTo(nameof(InitialHealthAnimationValue), newValue, increase_delay);
                Scheduler.AddOnce(Flash);

                if (newValue >= health.Value)
                    finishInitialAnimation();
            }, increase_delay, true);
        }

        private void finishInitialAnimation()
        {
            if (initialIncrease == null)
                return;

            initialIncrease?.Cancel();
            initialIncrease = null;

            // todo: this might be unnecessary
            // aside from the repeating `initialIncrease` scheduled task,
            // there may also be a `Current` transform in progress from that schedule.
            // ensure it plays out fully, to prevent changes to `initialHealthAnimationValue` being discarded by the ongoing transform.
            FinishTransforms(targetMember: nameof(initialHealthAnimationValue));
        }

        private void onNewJudgement(JudgementResult judgement)
        {
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (HealthProcessor.IsNotNull())
                HealthProcessor.NewJudgement -= onNewJudgement;
        }
    }
}
