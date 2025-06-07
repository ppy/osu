// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const double performance_base_multiplier = 1.14; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things.
        private const double difficulty_multiplier = 0.0675;
        private const double star_rating_multiplier = 0.0265;

        public override int Version => 20250306;

        public OsuDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        public static double CalculateDifficultyMultiplier(Mod[] mods, int totalHits, int spinnerCount)
        {
            double multiplier = performance_base_multiplier;

            if (mods.Any(m => m is OsuModSpunOut) && totalHits > 0)
                multiplier *= 1.0 - Math.Pow((double)spinnerCount / totalHits, 0.85);

            return multiplier;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods };

            var aim = skills.OfType<Aim>().Single(a => a.IncludeSliders);
            var aimWithoutSliders = skills.OfType<Aim>().Single(a => !a.IncludeSliders);
            var speed = skills.OfType<Speed>().Single();
            var flashlight = skills.OfType<Flashlight>().SingleOrDefault();
            var reading = skills.OfType<Reading>().SingleOrDefault();

            double speedNotes = speed.RelevantNoteCount();

            double aimDifficultStrainCount = aim.CountTopWeightedStrains();
            double speedDifficultStrainCount = speed.CountTopWeightedStrains();
            double readingDifficultNoteCount = reading?.CountTopWeightedNotes() ?? 0.0;

            double aimNoSlidersTopWeightedSliderCount = aimWithoutSliders.CountTopWeightedSliders();
            double aimNoSlidersDifficultStrainCount = aimWithoutSliders.CountTopWeightedStrains();

            double aimTopWeightedSliderFactor = aimNoSlidersTopWeightedSliderCount / Math.Max(1, aimNoSlidersDifficultStrainCount - aimNoSlidersTopWeightedSliderCount);

            double speedTopWeightedSliderCount = speed.CountTopWeightedSliders();
            double speedTopWeightedSliderFactor = speedTopWeightedSliderCount / Math.Max(1, speedDifficultStrainCount - speedTopWeightedSliderCount);

            double difficultSliders = aim.GetDifficultSliders();

            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            double hitWindowGreat = hitWindows.WindowFor(HitResult.Great) / clockRate;

            double overallDifficulty = (80 - hitWindowGreat) / 6;

            int hitCircleCount = beatmap.HitObjects.Count(h => h is HitCircle);
            int sliderCount = beatmap.HitObjects.Count(h => h is Slider);
            int spinnerCount = beatmap.HitObjects.Count(h => h is Spinner);

            int totalHits = beatmap.HitObjects.Count;

            double drainRate = beatmap.Difficulty.DrainRate;

            double aimRating = computeAimRating(aim.DifficultyValue(), mods, overallDifficulty);
            double aimRatingNoSliders = computeAimRating(aimWithoutSliders.DifficultyValue(), mods, overallDifficulty);
            double speedRating = computeSpeedRating(speed.DifficultyValue(), mods, overallDifficulty);

            double readingRating = 0.0;

            if (reading is not null)
                readingRating = computeReadingRating(reading.DifficultyValue(), mods, overallDifficulty);

            double flashlightRating = 0.0;

            if (flashlight is not null)
                flashlightRating = computeFlashlightRating(flashlight.DifficultyValue(), mods, totalHits, overallDifficulty);

            double sliderFactor = aimRating > 0 ? aimRatingNoSliders / aimRating : 1;

            double baseAimPerformance = OsuStrainSkill.DifficultyToPerformance(aimRating);
            double baseSpeedPerformance = OsuStrainSkill.DifficultyToPerformance(speedRating);
            double baseReadingPerformance = Reading.DifficultyToPerformance(readingRating);
            double baseFlashlightPerformance = Flashlight.DifficultyToPerformance(flashlightRating);

            double basePerformance =
                Math.Pow(
                    Math.Pow(baseAimPerformance, 1.1) +
                    Math.Pow(baseSpeedPerformance, 1.1) +
                    Math.Pow(baseFlashlightPerformance, 1.1) +
                    Math.Pow(baseReadingPerformance, 1.1), 1.0 / 1.1
                );

            double multiplier = CalculateDifficultyMultiplier(mods, totalHits, spinnerCount);
            double starRating = calculateStarRating(basePerformance, multiplier);

            double sliderNestedScorePerObject = LegacyScoreUtils.CalculateNestedScorePerObject(beatmap, totalHits);
            double legacyScoreBaseMultiplier = LegacyScoreUtils.CalculateDifficultyPeppyStars(beatmap);

            var simulator = new OsuLegacyScoreSimulator();
            var scoreAttributes = simulator.Simulate(WorkingBeatmap, beatmap);

            OsuDifficultyAttributes attributes = new OsuDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                AimDifficulty = aimRating,
                AimDifficultSliderCount = difficultSliders,
                SpeedDifficulty = speedRating,
                SpeedNoteCount = speedNotes,
                FlashlightDifficulty = flashlightRating,
                ReadingDifficulty = readingRating,
                SliderFactor = sliderFactor,
                AimDifficultStrainCount = aimDifficultStrainCount,
                SpeedDifficultStrainCount = speedDifficultStrainCount,
                ReadingDifficultNoteCount = readingDifficultNoteCount,
                AimTopWeightedSliderFactor = aimTopWeightedSliderFactor,
                SpeedTopWeightedSliderFactor = speedTopWeightedSliderFactor,
                DrainRate = drainRate,
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

        private double computeAimRating(double aimDifficultyValue, Mod[] mods, double overallDifficulty)
        {
            if (mods.Any(m => m is OsuModAutopilot))
                return 0;

            double aimRating = calculateDifficultyRating(aimDifficultyValue);

            if (mods.Any(m => m is OsuModTouchDevice))
                aimRating = Math.Pow(aimRating, 0.8);

            if (mods.Any(m => m is OsuModRelax))
                aimRating *= 0.9;

            if (mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                aimRating *= 1.0 - magnetisedStrength;
            }

            double ratingMultiplier = 1.0;

            // It is important to consider accuracy difficulty when scaling with accuracy.
            ratingMultiplier *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return aimRating * Math.Cbrt(ratingMultiplier);
        }

        private double computeSpeedRating(double speedDifficultyValue, Mod[] mods, double overallDifficulty)
        {
            if (mods.Any(m => m is OsuModRelax))
                return 0;

            double speedRating = calculateDifficultyRating(speedDifficultyValue);

            if (mods.Any(m => m is OsuModAutopilot))
                speedRating *= 0.5;

            if (mods.Any(m => m is OsuModMagnetised))
            {
                // reduce speed rating because of the speed distance scaling, with maximum reduction being 0.7x
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                speedRating *= 1.0 - magnetisedStrength * 0.3;
            }

            double ratingMultiplier = 1.0;

            ratingMultiplier *= 0.95 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 750;

            return speedRating * Math.Cbrt(ratingMultiplier);
        }

        private double computeReadingRating(double readingDifficultyValue, Mod[] mods, double overallDifficulty)
        {
            double readingRating = calculateDifficultyRating(readingDifficultyValue);

            if (mods.Any(m => m is OsuModTouchDevice))
                readingRating = Math.Pow(readingRating, 0.8);

            if (mods.Any(m => m is OsuModRelax))
                readingRating *= 0.7;
            else if (mods.Any(m => m is OsuModAutopilot))
                readingRating *= 0.4;

            if (mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                readingRating *= 1.0 - magnetisedStrength;
            }

            double ratingMultiplier = 1.0;

            // It is important to also consider accuracy difficulty when doing that.
            ratingMultiplier *= 0.75 + Math.Pow(Math.Max(0, overallDifficulty), 2.2) / 800;

            return readingRating * Math.Sqrt(ratingMultiplier);
        }

        private double computeFlashlightRating(double flashlightDifficultyValue, Mod[] mods, int totalHits, double overallDifficulty)
        {
            if (!mods.Any(m => m is OsuModFlashlight))
                return 0;

            double flashlightRating = calculateDifficultyRating(flashlightDifficultyValue);

            if (mods.Any(m => m is OsuModTouchDevice))
                flashlightRating = Math.Pow(flashlightRating, 0.8);

            if (mods.Any(m => m is OsuModRelax))
                flashlightRating *= 0.7;
            else if (mods.Any(m => m is OsuModAutopilot))
                flashlightRating *= 0.4;

            if (mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                flashlightRating *= 1.0 - magnetisedStrength;
            }

            double ratingMultiplier = 1.0;

            // Account for shorter maps having a higher ratio of 0 combo/100 combo flashlight radius.
            ratingMultiplier *= 0.7 + 0.1 * Math.Min(1.0, totalHits / 200.0) +
                                (totalHits > 200 ? 0.2 * Math.Min(1.0, (totalHits - 200) / 200.0) : 0.0);

            // It is important to consider accuracy difficulty when scaling with accuracy.
            ratingMultiplier *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return flashlightRating * Math.Sqrt(ratingMultiplier);
        }

        private static double calculateStarRating(double basePerformance, double multiplier)
        {
            if (basePerformance <= 0.00001)
                return 0;

            return Math.Cbrt(multiplier) * star_rating_multiplier * (Math.Cbrt(100000 / Math.Pow(2, 1 / 1.1) * basePerformance) + 4);
        }

        private static double calculateDifficultyRating(double difficultyValue) => Math.Sqrt(difficultyValue) * difficulty_multiplier;

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

            // The first jump is formed by the first two hitobjects of the map.
            // If the map has less than two OsuHitObjects, the enumerator will not return anything.
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
            {
                objects.Add(new OsuDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], clockRate, objects, objects.Count));
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
                new Reading(beatmap, mods, clockRate)
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
            new OsuModHidden(),
            new OsuModSpunOut(),
        };
    }
}
