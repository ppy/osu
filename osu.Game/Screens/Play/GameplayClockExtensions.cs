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
            // To handle rewind, we still want to maintain the same direction as the underlying clock.
            double rate = clock.Rate == 0 ? 1 : Math.Sign(clock.Rate);

            return rate
                   * clock.AdjustmentsFromMods.AggregateFrequency.Value
                   * clock.AdjustmentsFromMods.AggregateTempo.Value;
        }
    }
}
