// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    public abstract class PowerStatus
    {
        /// <summary>
        /// The maximum battery level before a warning notification
        /// is sent.
        /// </summary>
        public virtual double BatteryCutoff { get; } = 0.2;

        public virtual double ChargeLevel { get; set; }
        public virtual bool IsCharging { get; set; }
    }

    public class DefaultPowerStatus : PowerStatus
    {
        public override double ChargeLevel { get; set; } = 1;
        public override bool IsCharging { get; set; } = true;
    }
}
