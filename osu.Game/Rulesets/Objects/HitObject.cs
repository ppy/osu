// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Audio;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;

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
        public double StartTime;

        /// <summary>
        /// The samples to be played when this hit object is hit.
        /// <para>
        /// In the case of <see cref="IHasRepeats"/> types, this is the sample of the curve body
        /// and can be treated as the default samples for the hit object.
        /// </para>
        /// </summary>
        public List<SampleInfo> Samples = new List<SampleInfo>();

        /// <summary>
        /// Applies default values to this HitObject.
        /// </summary>
        /// <param name="difficulty">The difficulty settings to use.</param>
        /// <param name="timing">The timing settings to use.</param>
        public virtual void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            ControlPoint overridePoint;
            ControlPoint timingPoint = timing.TimingPointAt(StartTime, out overridePoint);

            ControlPoint samplePoint = overridePoint ?? timingPoint;

            // Initialize first sample
            Samples.ForEach(s => initializeSampleInfo(s, samplePoint));

            // Initialize any repeat samples
            var repeatData = this as IHasRepeats;
            repeatData?.RepeatSamples?.ForEach(r => r.ForEach(s => initializeSampleInfo(s, samplePoint)));
        }

        private void initializeSampleInfo(SampleInfo sample, ControlPoint controlPoint)
        {
            if (sample.Volume == 0)
                sample.Volume = controlPoint?.SampleVolume ?? 0;

            // If the bank is not assigned a name, assign it from the control point
            if (string.IsNullOrEmpty(sample.Bank))
                sample.Bank = controlPoint?.SampleBank ?? @"normal";
        }
    }
}
