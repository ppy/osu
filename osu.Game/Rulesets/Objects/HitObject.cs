// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ListExtensions;
using osu.Framework.Lists;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// A HitObject describes an object in a Beatmap.
    /// <para>
    /// HitObjects may contain more properties for which you should be checking through the IHas* types.
    /// </para>
    /// </summary>
    public class HitObject
    {
        /// <summary>
        /// A small adjustment to the start time of control points to account for rounding/precision errors.
        /// </summary>
        private const double control_point_leniency = 1;

        /// <summary>
        /// Invoked after <see cref="ApplyDefaults"/> has completed on this <see cref="HitObject"/>.
        /// </summary>
        public event Action<HitObject> DefaultsApplied;

        public readonly Bindable<double> StartTimeBindable = new BindableDouble();

        /// <summary>
        /// The time at which the HitObject starts.
        /// </summary>
        public virtual double StartTime
        {
            get => StartTimeBindable.Value;
            set => StartTimeBindable.Value = value;
        }

        public readonly BindableList<HitSampleInfo> SamplesBindable = new BindableList<HitSampleInfo>();

        /// <summary>
        /// The samples to be played when this hit object is hit.
        /// <para>
        /// In the case of <see cref="IHasRepeats"/> types, this is the sample of the curve body
        /// and can be treated as the default samples for the hit object.
        /// </para>
        /// </summary>
        public IList<HitSampleInfo> Samples
        {
            get => SamplesBindable;
            set
            {
                SamplesBindable.Clear();
                SamplesBindable.AddRange(value);
            }
        }

        /// <summary>
        /// Any samples which may be used by this hit object that are non-standard.
        /// This is used only to preload these samples ahead of time.
        /// </summary>
        public virtual IList<HitSampleInfo> AuxiliarySamples => ImmutableList<HitSampleInfo>.Empty;

        /// <summary>
        /// Whether this <see cref="HitObject"/> is in Kiai time.
        /// </summary>
        [JsonIgnore]
        public bool Kiai { get; private set; }

        /// <summary>
        /// The hit windows for this <see cref="HitObject"/>.
        /// </summary>
        [JsonIgnore]
        public HitWindows HitWindows { get; set; }

        private readonly List<HitObject> nestedHitObjects = new List<HitObject>();

        [JsonIgnore]
        public SlimReadOnlyListWrapper<HitObject> NestedHitObjects => nestedHitObjects.AsSlimReadOnly();

        /// <summary>
        /// Applies default values to this HitObject.
        /// </summary>
        /// <param name="controlPointInfo">The control points.</param>
        /// <param name="difficulty">The difficulty settings to use.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void ApplyDefaults(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty, CancellationToken cancellationToken = default)
        {
            ApplyDefaultsToSelf(controlPointInfo, difficulty);

            nestedHitObjects.Clear();

            CreateNestedHitObjects(cancellationToken);

            if (this is IHasComboInformation hasCombo)
            {
                foreach (HitObject hitObject in nestedHitObjects)
                {
                    if (hitObject is IHasComboInformation n)
                    {
                        n.ComboIndexBindable.BindTo(hasCombo.ComboIndexBindable);
                        n.ComboIndexWithOffsetsBindable.BindTo(hasCombo.ComboIndexWithOffsetsBindable);
                        n.IndexInCurrentComboBindable.BindTo(hasCombo.IndexInCurrentComboBindable);
                    }
                }
            }

            nestedHitObjects.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));

            foreach (var h in nestedHitObjects)
                h.ApplyDefaults(controlPointInfo, difficulty, cancellationToken);

            // `ApplyDefaults()` may be called multiple times on a single hitobject.
            // to prevent subscribing to `StartTimeBindable.ValueChanged` multiple times with the same callback,
            // remove the previous subscription (if present) before (re-)registering.
            StartTimeBindable.ValueChanged -= onStartTimeChanged;

            // this callback must be (re-)registered after default application
            // to ensure that the read of `this.GetEndTime()` within `onStartTimeChanged` doesn't return an invalid value
            // if `StartTimeBindable` is changed prior to default application.
            StartTimeBindable.ValueChanged += onStartTimeChanged;

            DefaultsApplied?.Invoke(this);

            void onStartTimeChanged(ValueChangedEvent<double> time)
            {
                double offset = time.NewValue - time.OldValue;

                foreach (var nested in nestedHitObjects)
                    nested.StartTime += offset;
            }
        }

        protected virtual void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            Kiai = controlPointInfo.EffectPointAt(StartTime + control_point_leniency).KiaiMode;

            HitWindows ??= CreateHitWindows();
            HitWindows?.SetDifficulty(difficulty.OverallDifficulty);
        }

        protected virtual void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
        }

        protected void AddNested(HitObject hitObject) => nestedHitObjects.Add(hitObject);

        /// <summary>
        /// Creates the <see cref="Judgement"/> that represents the scoring information for this <see cref="HitObject"/>.
        /// </summary>
        [NotNull]
        public virtual Judgement CreateJudgement() => new Judgement();

        /// <summary>
        /// Creates the <see cref="HitWindows"/> for this <see cref="HitObject"/>.
        /// This can be null to indicate that the <see cref="HitObject"/> has no <see cref="HitWindows"/> and timing errors should not be displayed to the user.
        /// <para>
        /// This will only be invoked if <see cref="HitWindows"/> hasn't been set externally (e.g. from a <see cref="BeatmapConverter{T}"/>.
        /// </para>
        /// </summary>
        [NotNull]
        protected virtual HitWindows CreateHitWindows() => new HitWindows();

        /// <summary>
        /// The maximum offset from the end time of <see cref="HitObject"/> at which this <see cref="HitObject"/> can be judged.
        /// <para>
        /// Defaults to the miss window.
        /// </para>
        /// </summary>
        public virtual double MaximumJudgementOffset => HitWindows?.WindowFor(HitResult.Miss) ?? 0;

        public IList<HitSampleInfo> CreateSlidingSamples()
        {
            var slidingSamples = new List<HitSampleInfo>();

            var normalSample = Samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL);
            if (normalSample != null)
                slidingSamples.Add(normalSample.With("sliderslide"));

            var whistleSample = Samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_WHISTLE);
            if (whistleSample != null)
                slidingSamples.Add(whistleSample.With("sliderwhistle"));

            return slidingSamples;
        }

        /// <summary>
        /// Create a <see cref="HitSampleInfo"/> based on the sample settings of the first <see cref="HitSampleInfo.HIT_NORMAL"/> sample in <see cref="Samples"/>.
        /// If no sample is available, sane default settings will be used instead.
        /// </summary>
        /// <remarks>
        /// In the case an existing sample exists, all settings apart from the sample name will be inherited. This includes volume, bank and suffix.
        /// </remarks>
        /// <param name="sampleName">The name of the sample.</param>
        /// <returns>A populated <see cref="HitSampleInfo"/>.</returns>
        public HitSampleInfo CreateHitSampleInfo(string sampleName = HitSampleInfo.HIT_NORMAL)
        {
            if (Samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL) is HitSampleInfo existingSample)
                return existingSample.With(newName: sampleName);

            return new HitSampleInfo(sampleName);
        }
    }

    public static class HitObjectExtensions
    {
        /// <summary>
        /// Returns the end time of this object.
        /// </summary>
        /// <remarks>
        /// This returns the <see cref="IHasDuration.EndTime"/> where available, falling back to <see cref="HitObject.StartTime"/> otherwise.
        /// </remarks>
        /// <param name="hitObject">The object.</param>
        /// <returns>The end time of this object.</returns>
        public static double GetEndTime(this HitObject hitObject) => (hitObject as IHasDuration)?.EndTime ?? hitObject.StartTime;
    }
}
