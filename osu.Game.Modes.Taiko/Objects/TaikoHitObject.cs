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
        public const float CIRCLE_RADIUS = 64;

        /// <summary>
        /// The time to scroll in the HitObject.
        /// </summary>
        public double PreEmpt;

        /// <summary>
        /// Whether this HitObject is accented.
        /// Accented hit objects give more points for hitting the hit object with both keys.
        /// </summary>
        public bool Accented;

        /// <summary>
        /// Whether this HitObject is in Kiai time.
        /// </summary>
        public bool Kiai { get; protected set; }

        public override void ApplyDefaults(TimingInfo timing, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(timing, difficulty);

            PreEmpt = 600 / (timing.SliderVelocityAt(StartTime) * difficulty.SliderMultiplier) * 1000;

            ControlPoint overridePoint;
            Kiai = timing.TimingPointAt(StartTime, out overridePoint).KiaiMode;

            if (overridePoint != null)
                Kiai |= overridePoint.KiaiMode;
        }
    }
}