// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    /// <summary>
    /// Provides access to the system's power status.
    /// Currently implemented on iOS and Android only.
    /// </summary>
    public abstract class PowerStatus
    {
        /// <summary>
        /// The maximum battery level considered as low, from 0 to 1.
        /// </summary>
        public abstract double BatteryCutoff { get; }

        /// <summary>
        /// The charge level of the battery, from 0 to 1.
        /// </summary>
        public abstract double ChargeLevel { get; }

        public abstract bool IsCharging { get; }

        /// <summary>
        /// Whether the battery is currently low in charge.
        /// Returns true if not charging and current charge level is lower than or equal to <see cref="BatteryCutoff"/>.
        /// </summary>
        public bool IsLowBattery => !IsCharging && ChargeLevel <= BatteryCutoff;
    }
}
