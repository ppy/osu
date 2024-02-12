// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSpinner : DrawableOsuHitObject
    {
        public new Spinner HitObject => (Spinner)base.HitObject;

        public new OsuSpinnerJudgementResult Result => (OsuSpinnerJudgementResult)base.Result;

        public SkinnableDrawable Body { get; private set; }

        public SpinnerRotationTracker RotationTracker { get; private set; }

        private SpinnerSpmCalculator spmCalculator;

        private Container<DrawableSpinnerTick> ticks;
        private PausableSkinnableSound spinningSample;

        private Bindable<bool> isSpinning;
        private bool spinnerFrequencyModulate;

        private const float spinning_sample_initial_frequency = 1.0f;
        private const float spinning_sample_modulated_base_frequency = 0.5f;

        private PausableSkinnableSound maxBonusSample;

        /// <summary>
        /// The amount of bonus score gained from spinning after the required number of spins, for display purposes.
        /// </summary>
        public double CurrentBonusScore => score_per_tick * Math.Clamp(completedFullSpins.Value - HitObject.SpinsRequiredForBonus, 0, HitObject.MaximumBonusSpins);

        /// <summary>
        /// The maximum amount of bonus score which can be achieved from extra spins.
        /// </summary>
        public double MaximumBonusScore => score_per_tick * HitObject.MaximumBonusSpins;

        public IBindable<int> CompletedFullSpins => completedFullSpins;

        private readonly Bindable<int> completedFullSpins = new Bindable<int>();

        /// <summary>
        /// The number of spins per minute this spinner is spinning at, for display purposes.
        /// </summary>
        public readonly IBindable<double> SpinsPerMinute = new BindableDouble();

        private const double fade_out_duration = 240;

        public DrawableSpinner()
            : this(null)
        {
        }

        public DrawableSpinner([CanBeNull] Spinner s = null)
            : base(s)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                spmCalculator = new SpinnerSpmCalculator
                {
                    Result = { BindTarget = SpinsPerMinute },
                },
                ticks = new Container<DrawableSpinnerTick>
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new AspectContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        Body = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.SpinnerBody), _ => new DefaultSpinner()),
                        RotationTracker = new SpinnerRotationTracker(this)
                    }
                },
                spinningSample = new PausableSkinnableSound
                {
                    Volume = { Value = 0 },
                    MinimumSampleVolume = MINIMUM_SAMPLE_VOLUME,
                    Looping = true,
                    Frequency = { Value = spinning_sample_initial_frequency }
                },
                maxBonusSample = new PausableSkinnableSound
                {
                    MinimumSampleVolume = MINIMUM_SAMPLE_VOLUME,
                }
            });

            PositionBindable.BindValueChanged(pos => Position = pos.NewValue);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isSpinning = RotationTracker.IsSpinning.GetBoundCopy();
            isSpinning.BindValueChanged(updateSpinningSample);
        }

        protected override void OnFree()
        {
            base.OnFree();

            spinningSample.ClearSamples();
            maxBonusSample.ClearSamples();
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();

            spinningSample.Samples = HitObject.CreateSpinningSamples().Cast<ISampleInfo>().ToArray();
            spinningSample.Frequency.Value = spinning_sample_initial_frequency;

            maxBonusSample.Samples = new ISampleInfo[] { new SpinnerBonusMaxSampleInfo(HitObject.CreateHitSampleInfo()) };
        }

        private void updateSpinningSample(ValueChangedEvent<bool> tracking)
        {
            if (tracking.NewValue)
            {
                if (!spinningSample.IsPlaying)
                    spinningSample.Play();

                spinningSample.VolumeTo(1, 300);
            }
            else
            {
                spinningSample.VolumeTo(0, fade_out_duration);
            }
        }

        public override void StopAllSamples()
        {
            base.StopAllSamples();
            spinningSample?.Stop();
            maxBonusSample?.Stop();
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

            this.FadeOut(fade_out_duration).OnComplete(_ =>
            {
                // looping sample should be stopped here as it is safer than running in the OnComplete
                // of the volume transition above.
                spinningSample.Stop();
            });

            Expire();

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

                return Math.Clamp(Result.TotalRotation / 360 / HitObject.SpinsRequired, 0, 1);
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
            {
                bool isValidSpinningTime = Time.Current >= HitObject.StartTime && Time.Current <= HitObject.EndTime;
                bool correctButtonPressed = (OsuActionInputManager?.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton) ?? false);

                RotationTracker.Tracking = !Result.HasResult
                                           && correctButtonPressed
                                           && isValidSpinningTime;
            }

            if (spinningSample != null && spinnerFrequencyModulate)
                spinningSample.Frequency.Value = spinning_sample_modulated_base_frequency + Progress;

            // Ticks can theoretically be judged at any point in the spinner's duration.
            // A tick must be alive to correctly play back samples,
            // but for performance reasons, we only want to keep the next tick alive.
            var next = NestedHitObjects.FirstOrDefault(h => !h.Judged);

            // See default `LifetimeStart` as set in `DrawableSpinnerTick`.
            if (next?.LifetimeStart == double.MaxValue)
                next.LifetimeStart = HitObject.StartTime;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (Result.TimeStarted == null && RotationTracker.Tracking)
                Result.TimeStarted = Time.Current;

            // don't update after end time to avoid the rate display dropping during fade out.
            // this shouldn't be limited to StartTime as it causes weirdness with the underlying calculation, which is expecting updates during that period.
            if (Time.Current <= HitObject.EndTime)
                spmCalculator.SetRotation(Result.TotalRotation);

            updateBonusScore();
        }

        private static readonly int score_per_tick = new OsuScoreProcessor().GetBaseScoreForResult(new SpinnerBonusTick.OsuSpinnerBonusTickJudgement().MaxResult);

        private void updateBonusScore()
        {
            if (ticks.Count == 0)
                return;

            int spins = (int)(Result.TotalRotation / 360);

            if (spins < completedFullSpins.Value)
            {
                // rewinding, silently handle
                completedFullSpins.Value = spins;
                return;
            }

            while (completedFullSpins.Value != spins)
            {
                var tick = ticks.FirstOrDefault(t => !t.Result.HasResult);

                // tick may be null if we've hit the spin limit.
                if (tick == null)
                {
                    // we still want to play a sound. this will probably be a new sound in the future, but for now let's continue playing the bonus sound.
                    // TODO: this doesn't concurrency. i can't figure out how to make it concurrency. samples are bad and need a refactor.
                    maxBonusSample.Play();
                }
                else
                    tick.TriggerResult(true);

                completedFullSpins.Value++;
            }
        }

        public class SpinnerBonusMaxSampleInfo : HitSampleInfo
        {
            public override IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (string name in base.LookupNames)
                        yield return name;

                    foreach (string name in base.LookupNames)
                        yield return name.Replace("-max", string.Empty);
                }
            }

            public SpinnerBonusMaxSampleInfo(HitSampleInfo sampleInfo)
                : base("spinnerbonus-max", sampleInfo.Bank, sampleInfo.Suffix, sampleInfo.Volume)

            {
            }
        }
    }
}
