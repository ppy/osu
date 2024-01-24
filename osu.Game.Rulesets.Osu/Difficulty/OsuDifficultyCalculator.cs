// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const double difficulty_multiplier = 0.0675;

        public override int Version => 20220902;
        public static double SumPower => 1.1;

        public OsuDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods };

            double aimRating = Math.Sqrt(skills[0].DifficultyValue()) * difficulty_multiplier;
            double aimRatingNoSliders = Math.Sqrt(skills[1].DifficultyValue()) * difficulty_multiplier;
            double speedRating = Math.Sqrt(skills[2].DifficultyValue()) * difficulty_multiplier;
            double speedNotes = ((Speed)skills[2]).RelevantNoteCount();
            double flashlightRating = Math.Sqrt(skills[3].DifficultyValue()) * difficulty_multiplier;

            double readingLowARRating = Math.Sqrt(skills[4].DifficultyValue()) * difficulty_multiplier;
            double readingHighARRating = Math.Sqrt(skills[5].DifficultyValue()) * difficulty_multiplier;
            double readingSlidersRating = 0;
            double hiddenRating = Math.Sqrt(skills[6].DifficultyValue()) * difficulty_multiplier;

            double sliderFactor = aimRating > 0 ? aimRatingNoSliders / aimRating : 1;

            if (mods.Any(m => m is OsuModTouchDevice))
            {
                aimRating = Math.Pow(aimRating, 0.8);
                flashlightRating = Math.Pow(flashlightRating, 0.8);
            }

            if (mods.Any(h => h is OsuModRelax))
            {
                aimRating *= 0.9;
                speedRating = 0.0;
                flashlightRating *= 0.7;
            }

            double baseAimPerformance = Math.Pow(5 * Math.Max(1, aimRating / 0.0675) - 4, 3) / 100000;
            double baseSpeedPerformance = Math.Pow(5 * Math.Max(1, speedRating / 0.0675) - 4, 3) / 100000;

            // Cognition
            double baseFlashlightPerformance = 0.0;
            if (mods.Any(h => h is OsuModFlashlight))
                baseFlashlightPerformance = Math.Pow(flashlightRating, 2.0) * 25.0;

            double baseReadingLowARPerformance = Math.Pow(readingLowARRating, 2.5) * 17.0;
            double baseReadingHighARPerformance = Math.Pow(5 * Math.Max(1, readingHighARRating / 0.0675) - 4, 3) / 100000;
            double baseReadingARPerformance = Math.Pow(Math.Pow(baseReadingLowARPerformance, SumPower) + Math.Pow(baseReadingHighARPerformance, SumPower), 1.0 / SumPower);

            double baseFlashlightARPerformance = Math.Pow(Math.Pow(baseFlashlightPerformance, 1.8) + Math.Pow(baseReadingARPerformance, 1.8), 1.0 / 1.8);

            double baseReadingHiddenPerformance = 0;
            if (mods.Any(h => h is OsuModHidden))
                baseReadingHiddenPerformance = Math.Pow(hiddenRating, 2.0) * 25.0;

            double baseReadingSliderPerformance = 0;
            double baseReadingNonARPerformance = baseReadingHiddenPerformance + baseReadingSliderPerformance;

            double basePerformance =
                Math.Pow(
                    Math.Pow(baseAimPerformance, SumPower) +
                    Math.Pow(baseSpeedPerformance, SumPower) +
                    Math.Pow(baseFlashlightARPerformance, SumPower) +
                    Math.Pow(baseReadingNonARPerformance, SumPower)
                    , 1.0 / SumPower
                );

            double starRating = basePerformance > 0.00001
                ? Math.Cbrt(OsuPerformanceCalculator.PERFORMANCE_BASE_MULTIPLIER) * 0.027 * (Math.Cbrt(100000 / Math.Pow(2, 1 / 1.1) * basePerformance) + 4)
                : 0;

            double preempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / clockRate;
            double drainRate = beatmap.Difficulty.DrainRate;
            int maxCombo = beatmap.GetMaxCombo();

            int hitCirclesCount = beatmap.HitObjects.Count(h => h is HitCircle);
            int sliderCount = beatmap.HitObjects.Count(h => h is Slider);
            int spinnerCount = beatmap.HitObjects.Count(h => h is Spinner);

            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            double hitWindowGreat = hitWindows.WindowFor(HitResult.Great) / clockRate;

            OsuDifficultyAttributes attributes = new OsuDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                AimDifficulty = aimRating,
                SpeedDifficulty = speedRating,
                SpeedNoteCount = speedNotes,
                ReadingDifficultyLowAR = readingLowARRating,
                ReadingDifficultyHighAR = readingHighARRating,
                ReadingDifficultySliders = readingSlidersRating,
                HiddenDifficulty = hiddenRating,
                FlashlightDifficulty = flashlightRating,
                SliderFactor = sliderFactor,
                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                DrainRate = drainRate,
                MaxCombo = maxCombo,
                HitCircleCount = hitCirclesCount,
                SliderCount = sliderCount,
                SpinnerCount = spinnerCount,
            };

            return attributes;
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

            // The first jump is formed by the first two hitobjects of the map.
            // If the map has less than two OsuHitObjects, the enumerator will not return anything.
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
            {
                var lastLast = i > 1 ? beatmap.HitObjects[i - 2] : null;
                objects.Add(new OsuDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], lastLast, clockRate, objects, objects.Count));
            }

            return objects;
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            return new Skill[]
            {
                new Aim(mods, true),
                new Aim(mods, false),
                new Speed(mods),
                new Flashlight(mods),
                new ReadingLowAR(mods),
                new ReadingHighAR(mods),
                new ReadingHidden(mods),
            };
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new OsuModTouchDevice(),
            new OsuModDoubleTime(),
            new OsuModHalfTime(),
            new OsuModEasy(),
            new OsuModHardRock(),
            new OsuModFlashlight(),
            new MultiMod(new OsuModFlashlight(), new OsuModHidden())
        };
    }
}
