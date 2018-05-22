// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Lists;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Types;

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
        /// The time at which the HitObject starts.
        /// </summary>
        public virtual double StartTime { get; set; }

        private List<SampleInfo> samples;

        /// <summary>
        /// The samples to be played when this hit object is hit.
        /// <para>
        /// In the case of <see cref="IHasRepeats"/> types, this is the sample of the curve body
        /// and can be treated as the default samples for the hit object.
        /// </para>
        /// </summary>
        public List<SampleInfo> Samples
        {
            get => samples ?? (samples = new List<SampleInfo>());
            set => samples = value;
        }

        [JsonIgnore]
        public SampleControlPoint SampleControlPoint;

        /// <summary>
        /// Whether this <see cref="HitObject"/> is in Kiai time.
        /// </summary>
        [JsonIgnore]
        public bool Kiai { get; private set; }

        private float overallDifficulty = BeatmapDifficulty.DEFAULT_DIFFICULTY;

        /// <summary>
        /// The hit windows for this <see cref="HitObject"/>.
        /// </summary>
        public HitWindows HitWindows { get; set; }

        private readonly Lazy<SortedList<HitObject>> nestedHitObjects = new Lazy<SortedList<HitObject>>(() => new SortedList<HitObject>((h1, h2) => h1.StartTime.CompareTo(h2.StartTime)));

        [JsonIgnore]
        public IReadOnlyList<HitObject> NestedHitObjects => nestedHitObjects.Value;

        /// <summary>
        /// Applies default values to this HitObject.
        /// </summary>
        /// <param name="controlPointInfo">The control points.</param>
        /// <param name="difficulty">The difficulty settings to use.</param>
        public void ApplyDefaults(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            ApplyDefaultsToSelf(controlPointInfo, difficulty);

            if (nestedHitObjects.IsValueCreated)
                nestedHitObjects.Value.Clear();

            CreateNestedHitObjects();

            if (nestedHitObjects.IsValueCreated)
            {
                nestedHitObjects.Value.ForEach(h =>
                {
                    h.HitWindows = HitWindows;
                    h.ApplyDefaults(controlPointInfo, difficulty);
                });
            }
        }

        protected virtual void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            SampleControlPoint samplePoint = controlPointInfo.SamplePointAt(StartTime);
            EffectControlPoint effectPoint = controlPointInfo.EffectPointAt(StartTime);

            Kiai = effectPoint.KiaiMode;
            SampleControlPoint = samplePoint;

            if (HitWindows == null)
                HitWindows = CreateHitWindows();
            HitWindows?.SetDifficulty(difficulty.OverallDifficulty);
        }

        protected virtual void CreateNestedHitObjects()
        {
        }

        protected void AddNested(HitObject hitObject) => nestedHitObjects.Value.Add(hitObject);

        /// <summary>
        /// Creates the <see cref="HitWindows"/> for this <see cref="HitObject"/>.
        /// This can be null to indicate that the <see cref="HitObject"/> has no <see cref="HitWindows"/>.
        /// <para>
        /// This will only be invoked if <see cref="HitWindows"/> hasn't been set externally (e.g. from a <see cref="BeatmapConverter"/>.
        /// </para>
        /// </summary>
        protected virtual HitWindows CreateHitWindows() => new HitWindows();
    }
}
