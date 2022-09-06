// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Play
{
    public static class GameplayClockExtensions
    {
        /// <summary>
        /// The rate of gameplay when playback is at 100%.
        /// This excludes any seeking / user adjustments.
        /// </summary>
        public static double GetTrueGameplayRate(this IGameplayClock clock)
        {
            double rate = Math.Sign(clock.Rate);
            foreach (double a in clock.GameplayAdjustments)
                rate *= a;
            return rate;
        }
    }
}
