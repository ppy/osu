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
        public const double DIFFICULTY_MULTIPLIER = 0.067;
        public const double SUM_POWER = 1.1;
        public const double FL_SUM_POWER = 1.6;
        public override int Version => 20220902;

        public OsuDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods };

            double aimRating = Math.Sqrt(skills[0].DifficultyValue()) * DIFFICULTY_MULTIPLIER;
            double aimRatingNoSliders = Math.Sqrt(skills[1].DifficultyValue()) * DIFFICULTY_MULTIPLIER;
            double speedRating = Math.Sqrt(skills[2].DifficultyValue()) * DIFFICULTY_MULTIPLIER;
            double speedNotes = ((Speed)skills[2]).RelevantNoteCount();
            double flashlightRating = Math.Sqrt(skills[3].DifficultyValue()) * DIFFICULTY_MULTIPLIER;

            double readingLowARRating = Math.Sqrt(skills[4].DifficultyValue()) * DIFFICULTY_MULTIPLIER;
            double readingHighARRating = Math.Sqrt(skills[5].DifficultyValue()) * DIFFICULTY_MULTIPLIER;
            double readingSlidersRating = 0;
            double hiddenRating = Math.Sqrt(skills[6].DifficultyValue()) * DIFFICULTY_MULTIPLIER;
            double hiddenFlashlightRating = Math.Sqrt(skills[7].DifficultyValue()) * DIFFICULTY_MULTIPLIER;

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

            double baseAimPerformance = OsuStrainSkill.DifficultyToPerformance(aimRating);
            double baseSpeedPerformance = OsuStrainSkill.DifficultyToPerformance(speedRating);

            // Cognition
            double baseFlashlightPerformance = 0.0;
            if (mods.Any(h => h is OsuModFlashlight))
                baseFlashlightPerformance = Flashlight.DifficultyToPerformance(flashlightRating);

            double baseReadingLowARPerformance = ReadingLowAR.DifficultyToPerformance(readingLowARRating);
            double baseReadingHighARPerformance = OsuStrainSkill.DifficultyToPerformance(readingHighARRating);
            double baseReadingARPerformance = Math.Pow(Math.Pow(baseReadingLowARPerformance, SUM_POWER) + Math.Pow(baseReadingHighARPerformance, SUM_POWER), 1.0 / SUM_POWER);

            double baseFlashlightARPerformance = Math.Pow(Math.Pow(baseFlashlightPerformance, FL_SUM_POWER) + Math.Pow(baseReadingARPerformance, FL_SUM_POWER), 1.0 / FL_SUM_POWER);

            double baseReadingHiddenPerformance = 0;
            if (mods.Any(h => h is OsuModHidden))
                baseReadingHiddenPerformance = ReadingHidden.DifficultyToPerformance(hiddenRating);

            double baseReadingSliderPerformance = 0;
            double baseReadingNonARPerformance = baseReadingHiddenPerformance + baseReadingSliderPerformance;

            double preempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            double drainRate = beatmap.Difficulty.DrainRate;
            int maxCombo = beatmap.GetMaxCombo();

            int hitCirclesCount = beatmap.HitObjects.Count(h => h is HitCircle);
            int sliderCount = beatmap.HitObjects.Count(h => h is Slider);
            int spinnerCount = beatmap.HitObjects.Count(h => h is Spinner);

            // Limit cognition by full memorisation difficulty
            double cognitionPerformance = Math.Pow(Math.Pow(baseFlashlightARPerformance, SUM_POWER) + Math.Pow(baseReadingNonARPerformance, SUM_POWER), 1.0 / SUM_POWER);
            double mechanicalPerformance = Math.Pow(Math.Pow(baseAimPerformance, SUM_POWER) + Math.Pow(baseSpeedPerformance, SUM_POWER), 1.0 / SUM_POWER);

            double maxHiddenFlashlightPerformance = OsuPerformanceCalculator.ComputePerfectFlashlightValue(hiddenFlashlightRating, hitCirclesCount + sliderCount);

            cognitionPerformance = OsuPerformanceCalculator.AdjustCognitionPerformance(cognitionPerformance, mechanicalPerformance, maxHiddenFlashlightPerformance);

            double basePerformance =
                Math.Pow(
                    Math.Pow(mechanicalPerformance, SUM_POWER) +
                    Math.Pow(cognitionPerformance, SUM_POWER)
                    , 1.0 / SUM_POWER
                );

            double starRating = basePerformance > 0.00001
                ? Math.Cbrt(OsuPerformanceCalculator.PERFORMANCE_BASE_MULTIPLIER) * 0.027 * (Math.Cbrt(100000 / Math.Pow(2, 1 / 1.1) * basePerformance) + 4)
                : 0;

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
                HiddenFlashlightDifficulty = hiddenFlashlightRating,
                SliderFactor = sliderFactor,
                ApproachRate = IBeatmapDifficultyInfo.InverseDifficultyRange(preempt, 1800, 1200, 450),
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
            var skills = new List<Skill>
            {
                new Aim(mods, true),
                new Aim(mods, false),
                new Speed(mods),
                new Flashlight(mods),
                new ReadingLowAR(mods),
                new ReadingHighAR(mods),
                new ReadingHidden(mods),
                new HiddenFlashlight(mods),
            };

            if (mods.Any(h => h is OsuModFlashlight))
                skills.Add(new Flashlight(mods));

            return skills.ToArray();
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
