// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Utils;

namespace osu.Desktop
{
    internal class SDL2BatteryInfo : BatteryInfo
    {
        public override double? ChargeLevel
        {
            get
            {
                SDL2.SDL.SDL_GetPowerInfo(out _, out int percentage);

                if (percentage == -1)
                    return null;

                return percentage / 100.0;
            }
        }

        public override bool OnBattery => SDL2.SDL.SDL_GetPowerInfo(out _, out _) == SDL2.SDL.SDL_PowerState.SDL_POWERSTATE_ON_BATTERY;
    }
}
