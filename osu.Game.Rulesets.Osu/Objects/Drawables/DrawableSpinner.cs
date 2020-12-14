// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinner : DrawableOsuHitObject
    {
        public new Spinner HitObject => (Spinner)base.HitObject;

        public new OsuSpinnerJudgementResult Result => (OsuSpinnerJudgementResult)base.Result;

        public SpinnerRotationTracker RotationTracker { get; private set; }
        public SpinnerSpmCounter SpmCounter { get; private set; }

        private Container<DrawableSpinnerTick> ticks;
        private SpinnerBonusDisplay bonusDisplay;
        private PausableSkinnableSound spinningSample;

        private Bindable<bool> isSpinning;
        private bool spinnerFrequencyModulate;

        public DrawableSpinner()
            : this(null)
        {
        }

        public DrawableSpinner([CanBeNull] Spinner s = null)
            : base(s)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                ticks = new Container<DrawableSpinnerTick>(),
                new AspectContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SpinnerBody), _ => new DefaultSpinnerDisc()),
                        RotationTracker = new SpinnerRotationTracker(this)
                    }
                },
                SpmCounter = new SpinnerSpmCounter
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = 120,
                    Alpha = 0
                },
                bonusDisplay = new SpinnerBonusDisplay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = -120,
                },
                spinningSample = new PausableSkinnableSound
                {
                    Volume = { Value = 0 },
                    Looping = true,
                    Frequency = { Value = spinning_sample_initial_frequency }
                }
            };

            PositionBindable.BindValueChanged(pos => Position = pos.NewValue);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isSpinning = RotationTracker.IsSpinning.GetBoundCopy();
            isSpinning.BindValueChanged(updateSpinningSample);
        }

        private const float spinning_sample_initial_frequency = 1.0f;
        private const float spinning_sample_modulated_base_frequency = 0.5f;

        protected override void OnFree()
        {
            base.OnFree();

            spinningSample.Samples = null;
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();

            var firstSample = HitObject.Samples.FirstOrDefault();

            if (firstSample != null)
            {
                var clone = HitObject.SampleControlPoint.ApplyTo(firstSample).With("spinnerspin");

                spinningSample.Samples = new ISampleInfo[] { clone };
                spinningSample.Frequency.Value = spinning_sample_initial_frequency;
            }
        }

        private void updateSpinningSample(ValueChangedEvent<bool> tracking)
        {
            if (tracking.NewValue)
            {
                spinningSample?.Play();
                spinningSample?.VolumeTo(1, 200);
            }
            else
            {
                spinningSample?.VolumeTo(0, 200).Finally(_ => spinningSample.Stop());
            }
        }

        public override void StopAllSamples()
        {
            base.StopAllSamples();
            spinningSample?.Stop();
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableSpinnerTick tick:
                    ticks.Add(tick);
                    break;
            }
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);

            this.FadeOut(160).Expire();

            // skin change does a rewind of transforms, which will stop the spinning sound from playing if it's currently in playback.
            isSpinning?.TriggerChange();
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            ticks.Clear(false);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SpinnerBonusTick bonusTick:
                    return new DrawableSpinnerBonusTick(bonusTick);

                case SpinnerTick tick:
                    return new DrawableSpinnerTick(tick);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void ApplySkin(ISkinSource skin, bool allowFallback)
        {
            base.ApplySkin(skin, allowFallback);
            spinnerFrequencyModulate = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.SpinnerFrequencyModulate)?.Value ?? true;
        }

        /// <summary>
        /// The completion progress of this spinner from 0..1 (clamped).
        /// </summary>
        public float Progress
        {
            get
            {
                if (HitObject.SpinsRequired == 0)
                    // some spinners are so short they can't require an integer spin count.
                    // these become implicitly hit.
                    return 1;

                return Math.Clamp(Result.RateAdjustedRotation / 360 / HitObject.SpinsRequired, 0, 1);
            }
        }

        protected override JudgementResult CreateResult(Judgement judgement) => new OsuSpinnerJudgementResult(HitObject, judgement);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (Progress >= 1)
                Result.TimeCompleted ??= Time.Current;

            if (userTriggered || Time.Current < HitObject.EndTime)
                return;

            // Trigger a miss result for remaining ticks to avoid infinite gameplay.
            foreach (var tick in ticks.Where(t => !t.Result.HasResult))
                tick.TriggerResult(false);

            ApplyResult(r =>
            {
                if (Progress >= 1)
                    r.Type = HitResult.Great;
                else if (Progress > .9)
                    r.Type = HitResult.Ok;
                else if (Progress > .75)
                    r.Type = HitResult.Meh;
                else if (Time.Current >= HitObject.EndTime)
                    r.Type = r.Judgement.MinResult;
            });
        }

        protected override void Update()
        {
            base.Update();

            if (HandleUserInput)
                RotationTracker.Tracking = !Result.HasResult && (OsuActionInputManager?.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton) ?? false);

            if (spinningSample != null && spinnerFrequencyModulate)
                spinningSample.Frequency.Value = spinning_sample_modulated_base_frequency + Progress;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!SpmCounter.IsPresent && RotationTracker.Tracking)
                SpmCounter.FadeIn(HitObject.TimeFadeIn);
            SpmCounter.SetRotation(Result.RateAdjustedRotation);

            updateBonusScore();
        }

        private int wholeSpins;

        private void updateBonusScore()
        {
            if (ticks.Count == 0)
                return;

            int spins = (int)(Result.RateAdjustedRotation / 360);

            if (spins < wholeSpins)
            {
                // rewinding, silently handle
                wholeSpins = spins;
                return;
            }

            while (wholeSpins != spins)
            {
                var tick = ticks.FirstOrDefault(t => !t.Result.HasResult);

                // tick may be null if we've hit the spin limit.
                if (tick != null)
                {
                    tick.TriggerResult(true);
                    if (tick is DrawableSpinnerBonusTick)
                        bonusDisplay.SetBonusCount(spins - HitObject.SpinsRequired);
                }

                wholeSpins++;
            }
        }
    }
}
