// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    public static class LegacyScoreUtils
    {
        /// <summary>
        /// Calculates the average amount of score per object that is caused by slider ticks.
        /// </summary>
        public static double CalculateSliderNestedScorePerObject(IBeatmap beatmap, int objectCount)
        {
            const double big_tick_score = 30;
            const double small_tick_score = 10;

            var sliders = beatmap.HitObjects.OfType<Slider>().ToArray();

            // 1 for head, 1 for tail
            int amountOfBigTicks = sliders.Length * 2;

            // Add slider repeats
            amountOfBigTicks += sliders.Select(s => s.RepeatCount).Sum();

            int amountOfSmallTicks = sliders.Select(s => s.NestedHitObjects.Count(nho => nho is SliderTick)).Sum();

            double totalScore = amountOfBigTicks * big_tick_score + amountOfSmallTicks * small_tick_score;

            return totalScore / objectCount;
        }

        /// <summary>
        /// Returns the legacy score multiplier for the given mods.
        /// </summary>
        /// <remarks>
        /// Logic copied from <see cref="OsuLegacyScoreSimulator.GetLegacyScoreMultiplier"/>.
        /// </remarks>
        public static double GetLegacyScoreMultiplier(IReadOnlyList<Mod> mods)
        {
            bool scoreV2 = mods.Any(m => m is ModScoreV2);

            double multiplier = 1.0;

            foreach (var mod in mods)
            {
                switch (mod)
                {
                    case OsuModNoFail:
                        multiplier *= scoreV2 ? 1.0 : 0.5;
                        break;

                    case OsuModEasy:
                        multiplier *= 0.5;
                        break;

                    case OsuModHalfTime:
                    case OsuModDaycore:
                        multiplier *= 0.3;
                        break;

                    case OsuModHidden:
                        multiplier *= 1.06;
                        break;

                    case OsuModHardRock:
                        multiplier *= scoreV2 ? 1.10 : 1.06;
                        break;

                    case OsuModDoubleTime:
                    case OsuModNightcore:
                        multiplier *= scoreV2 ? 1.20 : 1.12;
                        break;

                    case OsuModFlashlight:
                        multiplier *= 1.12;
                        break;

                    case OsuModSpunOut:
                        multiplier *= 0.9;
                        break;

                    case OsuModRelax:
                    case OsuModAutopilot:
                        return 0;
                }
            }

            return multiplier;
        }

        public static int CalculateDifficultyPeppyStars(IBeatmap beatmap)
        {
            int objectCount = beatmap.HitObjects.Count;
            int drainLength = 0;

            if (objectCount > 0)
            {
                int breakLength = beatmap.Breaks.Select(b => (int)Math.Round(b.EndTime) - (int)Math.Round(b.StartTime)).Sum();
                drainLength = ((int)Math.Round(beatmap.HitObjects[^1].StartTime) - (int)Math.Round(beatmap.HitObjects[0].StartTime) - breakLength) / 1000;
            }

            return LegacyRulesetExtensions.CalculateDifficultyPeppyStars(beatmap.Difficulty, objectCount, drainLength);
        }
    }
}
