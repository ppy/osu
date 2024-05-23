// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Utils;
using SDL;

namespace osu.Desktop
{
    internal unsafe class SDL3BatteryInfo : BatteryInfo
    {
        public override double? ChargeLevel
        {
            get
            {
                int percentage;
                SDL3.SDL_GetPowerInfo(null, &percentage);

                if (percentage == -1)
                    return null;

                return percentage / 100.0;
            }
        }

        public override bool OnBattery => SDL3.SDL_GetPowerInfo(null, null) == SDL_PowerState.SDL_POWERSTATE_ON_BATTERY;
    }
}
