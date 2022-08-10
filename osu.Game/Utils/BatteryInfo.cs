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
        /// The charge level of the battery, from <c>0</c> to <c>1</c>, or <c>null</c> if a battery isn't present.
        /// </summary>
        public abstract double? ChargeLevel { get; }

        /// <summary>
        /// Whether the current power source is the battery.
        /// </summary>
        /// <remarks>
        /// This is <c>false</c> when the device is charging or doesn't have a battery.
        /// </remarks>
        public abstract bool OnBattery { get; }
    }
}
