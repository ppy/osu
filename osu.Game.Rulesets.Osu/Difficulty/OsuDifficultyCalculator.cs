// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        public override int Version => 20260706;

        public OsuDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, IReadOnlyList<ISkillAttributes> skillAttributes)
        {
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods };

            var aimAttributes = skillAttributes.OfType<AimAttributes>().Single(a => a.WithSliders);
            var aimWithoutSlidersAttributes = skillAttributes.OfType<AimAttributes>().Single(a => !a.WithSliders);
            var speedAttributes = skillAttributes.OfType<SpeedAttributes>().Single();
            var flashlightAttributes = skillAttributes.OfType<FlashlightAttributes>().SingleOrDefault();
            var readingAttributes = skillAttributes.OfType<ReadingAttributes>().Single();

            double aimTopWeightedSliderFactor = aimWithoutSlidersAttributes.TopWeightedSlidersCount / Math.Max(1, aimWithoutSlidersAttributes.TopWeightedStrainsCount - aimWithoutSlidersAttributes.TopWeightedSlidersCount);
            double speedTopWeightedSliderFactor = speedAttributes.TopWeightedSlidersCount / Math.Max(1, speedAttributes.TopWeightedObjectDifficultiesCount - speedAttributes.TopWeightedSlidersCount);

            int hitCircleCount = beatmap.HitObjects.Count(h => h is HitCircle);
            int sliderCount = beatmap.HitObjects.Count(h => h is Slider);
            int spinnerCount = beatmap.HitObjects.Count(h => h is Spinner);

            int totalHits = beatmap.HitObjects.Count;

            double aimRating = calculateAimDifficultyRating(aimAttributes.Difficulty);
            double aimNoSlidersRating = calculateAimDifficultyRating(aimWithoutSlidersAttributes.Difficulty);

            double sliderFactor = aimAttributes.Difficulty > 0
                ? aimNoSlidersRating / aimRating
                : 1;

            double speedRating = calculateDifficultyRating(speedAttributes.Difficulty);
            double readingRating = calculateDifficultyRating(readingAttributes.Difficulty);

            double flashlightRating = 0.0;

            if (flashlightAttributes is not null)
                flashlightRating = calculateDifficultyRating(flashlightAttributes.Difficulty);

            double sliderNestedScorePerObject = LegacyScoreUtils.CalculateNestedScorePerObject(beatmap, totalHits);
            double legacyScoreBaseMultiplier = LegacyScoreUtils.CalculateDifficultyPeppyStars(WorkingBeatmap.Beatmap);

            var simulator = new OsuLegacyScoreSimulator();
            var scoreAttributes = simulator.Simulate(WorkingBeatmap, beatmap);

            double baseAimPerformance = OsuPerformanceCalculator.DifficultyToPerformance(aimRating);
            double baseSpeedPerformance = OsuPerformanceCalculator.DifficultyToPerformance(speedRating);
            double baseReadingPerformance = OsuPerformanceCalculator.DifficultyToPerformance(readingRating);
            double baseFlashlightPerformance = Flashlight.DifficultyToPerformance(flashlightRating);
            double baseCognitionPerformance = SumCognitionDifficulty(baseReadingPerformance, baseFlashlightPerformance);

            double basePerformance = DiffUtils.Norm(OsuPerformanceCalculator.PERFORMANCE_NORM_EXPONENT, baseAimPerformance, baseSpeedPerformance, baseCognitionPerformance);

            double starRating = calculateStarRating(basePerformance);

            OsuDifficultyAttributes attributes = new OsuDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                AimDifficulty = aimRating,
                AimDifficultSliderCount = aimAttributes.DifficultSlidersCount,
                SpeedDifficulty = speedRating,
                SpeedNoteCount = speedAttributes.RelevantObjectCount,
                FlashlightDifficulty = flashlightRating,
                ReadingDifficulty = readingRating,
                SliderFactor = sliderFactor,
                AimDifficultStrainCount = aimAttributes.TopWeightedStrainsCount,
                SpeedDifficultStrainCount = speedAttributes.TopWeightedObjectDifficultiesCount,
                ReadingDifficultNoteCount = readingAttributes.TopWeightedObjectDifficultiesCount,
                AimTopWeightedSliderFactor = aimTopWeightedSliderFactor,
                SpeedTopWeightedSliderFactor = speedTopWeightedSliderFactor,
                MaxCombo = beatmap.GetMaxCombo(),
                HitCircleCount = hitCircleCount,
                SliderCount = sliderCount,
                SpinnerCount = spinnerCount,
                NestedScorePerObject = sliderNestedScorePerObject,
                LegacyScoreBaseMultiplier = legacyScoreBaseMultiplier,
                MaximumLegacyComboScore = scoreAttributes.ComboScore
            };

            return attributes;
        }

        public static double SumCognitionDifficulty(double reading, double flashlight)
        {
            if (reading <= 0)
                return flashlight;

            if (flashlight <= 0)
                return reading;

            // Nerf flashlight value in cognition sum when reading is greater than flashlight
            return DiffUtils.Norm(OsuPerformanceCalculator.PERFORMANCE_NORM_EXPONENT, reading, flashlight * Math.Clamp(flashlight / reading, 0.25, 1.0));
        }

        private double calculateAimDifficultyRating(double difficultyValue) => DiffUtils.Pow(difficultyValue, 0.63) * 0.02275;

        private double calculateDifficultyRating(double difficultyValue) => Math.Sqrt(difficultyValue) * 0.0675;

        private double calculateStarRating(double basePerformance)
        {
            return Math.Cbrt(basePerformance * OsuPerformanceCalculator.PERFORMANCE_BASE_MULTIPLIER);
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, Mod[] mods)
        {
            List<DifficultyHitObject> objects = new List<DifficultyHitObject>(beatmap.HitObjects.Count);

            double clockRate = ModUtils.CalculateRateWithMods(mods);

            // The first jump is formed by the first two hitobjects of the map.
            // If the map has less than two OsuHitObjects, the enumerator will not return anything.
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
            {
                objects.Add(new OsuDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], clockRate, objects, objects.Count));
            }

            return objects;
        }

        protected override ISkill[] CreateSkills(IBeatmap beatmap, Mod[] mods, DifficultyHitObject[] difficultyHitObjects)
        {
            var skills = new List<ISkill>
            {
                new Aim(mods, difficultyHitObjects, true),
                new Aim(mods, difficultyHitObjects, false),
                new Speed(mods, difficultyHitObjects),
                new Reading(mods, difficultyHitObjects)
            };

            if (mods.Any(h => h is OsuModFlashlight))
                skills.Add(new Flashlight(mods, difficultyHitObjects, beatmap.HitObjects.Count));

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
            new OsuModHidden(),
        };
    }
}
