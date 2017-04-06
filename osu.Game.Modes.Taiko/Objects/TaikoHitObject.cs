// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Modes.Objects;

namespace osu.Game.Modes.Taiko.Objects
{
    public abstract class TaikoHitObject : HitObject
    {
        /// <summary>
        /// HitCircle radius.
        /// </summary>
        public const float CIRCLE_RADIUS = 42f;

        /// <summary>
        /// The time taken from the initial (off-screen) spawn position to the centre of the hit target for a <see cref="ControlPoint.BeatLength"/> of 1000ms.
        /// </summary>
        private const double scroll_time = 6000;

        /// <summary>
        /// Our adjusted <see cref="scroll_time"/> taking into consideration local <see cref="ControlPoint.BeatLength"/> and other speed multipliers.
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

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            ScrollTime = scroll_time * (timing.BeatLengthAt(StartTime) * timing.SpeedMultiplierAt(StartTime) / 1000) / difficulty.SliderMultiplier;

            ControlPoint overridePoint;
            Kiai = timing.TimingPointAt(StartTime, out overridePoint).KiaiMode;

            if (overridePoint != null)
                Kiai |= overridePoint.KiaiMode;
        }
    }
}