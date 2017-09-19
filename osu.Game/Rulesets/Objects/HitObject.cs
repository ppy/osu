// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        /// <summary>
        /// The samples to be played when this hit object is hit.
        /// <para>
        /// In the case of <see cref="IHasRepeats"/> types, this is the sample of the curve body
        /// and can be treated as the default samples for the hit object.
        /// </para>
        /// </summary>
        public SampleInfoList Samples = new SampleInfoList();

        /// <summary>
        /// Whether this <see cref="HitObject"/> is in Kiai time.
        /// </summary>
        public bool Kiai { get; private set; }

        /// <summary>
        /// Applies default values to this HitObject.
        /// </summary>
        /// <param name="controlPointInfo">The control points.</param>
        /// <param name="difficulty">The difficulty settings to use.</param>
        public virtual void ApplyDefaults(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            SoundControlPoint soundPoint = controlPointInfo.SoundPointAt(StartTime);
            EffectControlPoint effectPoint = controlPointInfo.EffectPointAt(StartTime);

            Kiai |= effectPoint.KiaiMode;

            // Initialize first sample
            Samples.ForEach(s => initializeSampleInfo(s, soundPoint));

            // Initialize any repeat samples
            var repeatData = this as IHasRepeats;
            repeatData?.RepeatSamples?.ForEach(r => r.ForEach(s => initializeSampleInfo(s, soundPoint)));
        }

        private void initializeSampleInfo(SampleInfo sample, SoundControlPoint soundPoint)
        {
            if (sample.Volume == 0)
                sample.Volume = soundPoint?.SampleVolume ?? 0;

            // If the bank is not assigned a name, assign it from the control point
            if (string.IsNullOrEmpty(sample.Bank))
                sample.Bank = soundPoint?.SampleBank ?? @"normal";
        }
    }
}
