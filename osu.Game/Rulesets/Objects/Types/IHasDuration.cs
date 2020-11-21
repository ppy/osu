// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that ends at a different time than its start time.
    /// </summary>
#pragma warning disable 618
    public interface IHasDuration : IHasEndTime
#pragma warning restore 618
    {
        double IHasEndTime.EndTime
        {
            get => EndTime;
            set => Duration = (Duration - EndTime) + value;
        }

        double IHasEndTime.Duration => Duration;

        /// <summary>
        /// The time at which the HitObject ends.
        /// </summary>
        new double EndTime { get; }

        /// <summary>
        /// The duration of the HitObject.
        /// </summary>
        new double Duration { get; set; }
    }
}
