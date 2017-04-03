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
        /// Time (in milliseconds) to scroll in the hit object with a speed-adjusted beat length of 1 second.
        /// </summary>
        private const double base_scroll_time = 6000;

        /// <summary>
        /// The velocity multiplier applied to this hit object.
        /// </summary>
        public float VelocityMultiplier = 1;

        /// <summary>
        /// The time to scroll in the HitObject.
        /// </summary>
        public double PreEmpt;

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

            PreEmpt = base_scroll_time / difficulty.SliderMultiplier * timing.BeatLengthAt(StartTime) * timing.SpeedMultiplierAt(StartTime) / VelocityMultiplier / 1000;

            ControlPoint overridePoint;
            Kiai = timing.TimingPointAt(StartTime, out overridePoint).KiaiMode;

            if (overridePoint != null)
                Kiai |= overridePoint.KiaiMode;
        }
    }
}