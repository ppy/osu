// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public abstract class TaikoHitObject : HitObject
    {
        /// <summary>
        /// Default size of a drawable taiko hit object.
        /// </summary>
        public const float DEFAULT_SIZE = 0.45f;

        /// <summary>
        /// Scale multiplier for a strong drawable taiko hit object.
        /// </summary>
        public const float STRONG_SCALE = 1.4f;

        /// <summary>
        /// Default size of a strong drawable taiko hit object.
        /// </summary>
        public const float DEFAULT_STRONG_SIZE = DEFAULT_SIZE * STRONG_SCALE;

        /// <summary>
        /// The time taken from the initial (off-screen) spawn position to the centre of the hit target for a <see cref="TimingControlPoint.BeatLength"/> of 1000ms.
        /// </summary>
        private const double scroll_time = 6000;

        /// <summary>
        /// Our adjusted <see cref="scroll_time"/> taking into consideration local <see cref="TimingControlPoint.BeatLength"/> and other speed multipliers.
        /// </summary>
        public double ScrollTime;

        /// <summary>
        /// Whether this HitObject is a "strong" type.
        /// Strong hit objects give more points for hitting the hit object with both keys.
        /// </summary>
        public bool IsStrong;

        /// <summary>
        /// Whether this HitObject is in Kiai time.
        /// </summary>
        public bool Kiai { get; protected set; }

        public override void ApplyDefaults(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);
            EffectControlPoint effectPoint = controlPointInfo.EffectPointAt(StartTime);

            ScrollTime = scroll_time * (timingPoint.BeatLength * difficultyPoint.SpeedMultiplier / 1000) / difficulty.SliderMultiplier;

            Kiai |= effectPoint.KiaiMode;
        }
    }
}