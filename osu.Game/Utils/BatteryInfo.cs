// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    /// <summary>
    /// Provides access to the system's power status.
    /// </summary>
    public abstract class BatteryInfo
    {
        /// <summary>
        /// The charge level of the battery, from 0 to 1.
        /// </summary>
        public abstract double ChargeLevel { get; }

        public abstract bool IsCharging { get; }
    }
}
