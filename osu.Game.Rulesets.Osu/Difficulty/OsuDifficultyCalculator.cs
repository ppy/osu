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

        //The bonus multiplier is a basic multiplier that indicate how strong the impact of Difficulty Factor is.
        private const double bonus_multiplier = 0.6;

        public override int Version => 20241007;

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
            double difficultSliders = ((Aim)skills[0]).GetDifficultSliders();
            double flashlightRating = 0.0;

            double aimConsistencyFactor = skills[0].ConsistencyFactor;
            double speedConsistencyFactor = skills[2].ConsistencyFactor;

            if (mods.Any(h => h is OsuModFlashlight))
                flashlightRating = Math.Sqrt(skills[3].DifficultyValue()) * difficulty_multiplier;

            double sliderFactor = aimRating > 0 ? aimRatingNoSliders / aimRating : 1;

            double aimDifficultyStrainCount = ((OsuStrainSkill)skills[0]).CountTopWeightedStrains();
            double speedDifficultyStrainCount = ((OsuStrainSkill)skills[2]).CountTopWeightedStrains();

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
            else if (mods.Any(h => h is OsuModAutopilot))
            {
                speedRating *= 0.5;
                aimRating = 0.0;
                flashlightRating *= 0.4;
            }

            double speedApproachRateBonus = beatmap.Difficulty.ApproachRate > 10.33 ? 0.2 * (beatmap.Difficulty.ApproachRate - 10.33) : 0.0; //AR bonus for higher AR;
            double aimApproachRateBonus = beatmap.Difficulty.ApproachRate > 10.33 ? 0.2 * (beatmap.Difficulty.ApproachRate - 10.33) : beatmap.Difficulty.ApproachRate < 8.0 ? 0.02 * (8.0 - beatmap.Difficulty.ApproachRate) : 0.0; //AR bonus for higher and lower AR;

            double baseAimPerformance = OsuStrainSkill.DifficultyToPerformance(aimRating);
            baseAimPerformance *= OsuStrainSkill.CalculateLengthBonus(beatmap.HitObjects.Count, skills[0].ConsistencyFactor, aimApproachRateBonus);
            double baseSpeedPerformance = OsuStrainSkill.DifficultyToPerformance(speedRating);
            baseSpeedPerformance *= OsuStrainSkill.CalculateLengthBonus(beatmap.HitObjects.Count, skills[2].ConsistencyFactor, speedApproachRateBonus);
            double baseFlashlightPerformance = 0.0;

            double flashligthConsistencyFactor = 0.0;

            if (mods.Any(h => h is OsuModFlashlight))
            {
                flashligthConsistencyFactor = skills[3].ConsistencyFactor;
                baseFlashlightPerformance = Flashlight.DifficultyToPerformance(flashlightRating);
                baseFlashlightPerformance *= Flashlight.CalculateLengthBonus(beatmap.HitObjects.Count, flashligthConsistencyFactor, 0.0);
            }

            double basePerformance =
                Math.Pow(
                    Math.Pow(baseAimPerformance, 1.1) +
                    Math.Pow(baseSpeedPerformance, 1.1) +
                    Math.Pow(baseFlashlightPerformance, 1.1), 1.0 / 1.1
                );

            double starRating = basePerformance > 0.00001
                ? Math.Cbrt(OsuPerformanceCalculator.PERFORMANCE_BASE_MULTIPLIER) * 0.027 * (Math.Cbrt(100000 / Math.Pow(2, 1 / 1.1) * basePerformance) + 4)
                : 0;

            double preempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / clockRate;
            double drainRate = beatmap.Difficulty.DrainRate;

            int hitCirclesCount = beatmap.HitObjects.Count(h => h is HitCircle);
            int sliderCount = beatmap.HitObjects.Count(h => h is Slider);
            int spinnerCount = beatmap.HitObjects.Count(h => h is Spinner);

            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            double hitWindowGreat = hitWindows.WindowFor(HitResult.Great) / clockRate;
            double hitWindowOk = hitWindows.WindowFor(HitResult.Ok) / clockRate;
            double hitWindowMeh = hitWindows.WindowFor(HitResult.Meh) / clockRate;

            OsuDifficultyAttributes attributes = new OsuDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                AimDifficulty = aimRating,
                AimConsistencyFactor = skills[0].ConsistencyFactor,
                AimDifficultSliderCount = difficultSliders,
                SpeedDifficulty = speedRating,
                SpeedConsistencyFactor = skills[2].ConsistencyFactor,
                SpeedNoteCount = speedNotes,
                FlashlightDifficulty = flashlightRating,
                FlashlightConsistencyFactor = flashligthConsistencyFactor,
                SliderFactor = sliderFactor,
                AimDifficultStrainCount = aimDifficultyStrainCount,
                SpeedDifficultStrainCount = speedDifficultyStrainCount,
                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                GreatHitWindow = hitWindowGreat,
                OkHitWindow = hitWindowOk,
                MehHitWindow = hitWindowMeh,
                DrainRate = drainRate,
                MaxCombo = beatmap.GetMaxCombo(),
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
                new Speed(mods)
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
