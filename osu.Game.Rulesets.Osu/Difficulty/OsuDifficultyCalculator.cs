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
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const double performance_base_multiplier = 1.15; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things.
        private const double difficulty_multiplier = 0.0675;
        private const double star_rating_multiplier = 0.0265;

        public override int Version => 20250306;

        private double mechanicalDifficultyRating;

        public OsuDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        // Increasing this multiplier buffs versatile aim+flow maps
        public static double AimVersatilityBonus = 0.08;

        public static double CalculateDifficultyMultiplier(Mod[] mods, int totalHits, int spinnerCount)
        {
            double multiplier = performance_base_multiplier;

            if (mods.Any(m => m is OsuModSpunOut) && totalHits > 0)
                multiplier *= 1.0 - Math.Pow((double)spinnerCount / totalHits, 0.85);

            return multiplier;
        }

        /// <summary>
        /// Calculates a visibility bonus that is applicable to Hidden and Traceable.
        /// </summary>
        public static double CalculateVisibilityBonus(Mod[] mods, double approachRate, double visibilityFactor = 1)
        {
            // NOTE: TC's effect is only noticeable in performance calculations until lazer mods are accounted for server-side.
            bool isAlwaysPartiallyVisible = mods.OfType<OsuModHidden>().Any(m => !m.OnlyFadeApproachCircles.Value) || mods.OfType<OsuModTraceable>().Any();

            // Start from normal curve, rewarding lower AR up to AR5
            double readingBonus = 0.04 * (12.0 - Math.Max(approachRate, 5));

            readingBonus *= visibilityFactor;

            // For AR up to 0 - reduce reward for very low ARs when object is visible
            if (approachRate < 5)
                readingBonus += (isAlwaysPartiallyVisible ? 0.04 : 0.03) * (5.0 - Math.Max(approachRate, 0));

            // Starting from AR0 - cap values so they won't grow to infinity
            if (approachRate < 0)
                readingBonus += (isAlwaysPartiallyVisible ? 0.1 : 0.075) * (1 - Math.Pow(1.5, approachRate));

            return readingBonus;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods };

            var aim = skills.OfType<TotalAim>().Single(a => a.IncludeSliders);
            var aimWithoutSliders = skills.OfType<TotalAim>().Single(a => !a.IncludeSliders);
            var speed = skills.OfType<Speed>().Single();
            var flashlight = skills.OfType<Flashlight>().SingleOrDefault();

            double speedNotes = speed.RelevantNoteCount();

            double aimDifficultStrainCount = aim.CountTopWeightedStrains();
            double speedDifficultStrainCount = speed.CountTopWeightedStrains();

            double aimNoSlidersTopWeightedSliderCount = aimWithoutSliders.CountTopWeightedSliders();
            double aimNoSlidersDifficultStrainCount = aimWithoutSliders.CountTopWeightedStrains();

            double aimTopWeightedSliderFactor = aimNoSlidersTopWeightedSliderCount / Math.Max(1, aimNoSlidersDifficultStrainCount - aimNoSlidersTopWeightedSliderCount);

            double speedTopWeightedSliderCount = speed.CountTopWeightedSliders();
            double speedTopWeightedSliderFactor = speedTopWeightedSliderCount / Math.Max(1, speedDifficultStrainCount - speedTopWeightedSliderCount);

            double difficultSliders = aim.GetDifficultSliders();

            double preempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / clockRate;
            double approachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5;

            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            double hitWindowGreat = hitWindows.WindowFor(HitResult.Great) / clockRate;

            double overallDifficulty = (80 - hitWindowGreat) / 6;

            int hitCircleCount = beatmap.HitObjects.Count(h => h is HitCircle);
            int sliderCount = beatmap.HitObjects.Count(h => h is Slider);
            int spinnerCount = beatmap.HitObjects.Count(h => h is Spinner);

            int totalHits = beatmap.HitObjects.Count;

            double drainRate = beatmap.Difficulty.DrainRate;

            double aimDifficultyValue = aim.DifficultyValue();
            double aimNoSlidersDifficultyValue = aimWithoutSliders.DifficultyValue();
            double snapAimDifficultyValue = skills.OfType<SnapAim>().Single().DifficultyValue();
            double flowAimDifficultyValue = skills.OfType<FlowAim>().Single().DifficultyValue();
            double speedDifficultyValue = speed.DifficultyValue();

            mechanicalDifficultyRating = calculateMechanicalDifficultyRating(aimDifficultyValue, speedDifficultyValue);

            double aimRating = computeTotalAimRating(aim.DifficultyValue(), snapAimDifficultyValue, flowAimDifficultyValue, mods, totalHits, approachRate, overallDifficulty);
            double aimRatingNoSliders = computeTotalAimRating(aimWithoutSliders.DifficultyValue(), snapAimDifficultyValue, flowAimDifficultyValue, mods, totalHits, approachRate, overallDifficulty);
            double speedRating = computeSpeedRating(speed.DifficultyValue(), mods, totalHits, approachRate, overallDifficulty);

            double snapAimRating = computeSnapAimRating(snapAimDifficultyValue, mods, totalHits, approachRate, overallDifficulty);
            double flowAimRating = computeFlowAimRating(flowAimDifficultyValue, mods, totalHits, approachRate, overallDifficulty);

            double flashlightRating = 0.0;

            if (flashlight is not null)
                flashlightRating = computeFlashlightRating(flashlight.DifficultyValue(), mods, totalHits, overallDifficulty);

            double sliderFactor = aimRating > 0 ? aimRatingNoSliders / aimRating : 1;

            double baseAimPerformance = OsuStrainSkill.DifficultyToPerformance(aimRating);
            double baseSpeedPerformance = OsuStrainSkill.DifficultyToPerformance(speedRating);
            double baseFlashlightPerformance = Flashlight.DifficultyToPerformance(flashlightRating);

            double basePerformance =
                Math.Pow(
                    Math.Pow(SumMechanicalDifficulty(baseAimPerformance, baseSpeedPerformance), 1.1) +
                    Math.Pow(baseFlashlightPerformance, 1.1), 1.0 / 1.1
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
                SliderFactor = sliderFactor,
                AimDifficultStrainCount = aimDifficultStrainCount,
                SpeedDifficultStrainCount = speedDifficultStrainCount,
                AimTopWeightedSliderFactor = aimTopWeightedSliderFactor,
                SpeedTopWeightedSliderFactor = speedTopWeightedSliderFactor,
                DrainRate = drainRate,
                MaxCombo = beatmap.GetMaxCombo(),
                HitCircleCount = hitCircleCount,
                SliderCount = sliderCount,
                SpinnerCount = spinnerCount,
                NestedScorePerObject = sliderNestedScorePerObject,
                LegacyScoreBaseMultiplier = legacyScoreBaseMultiplier,
                MaximumLegacyComboScore = scoreAttributes.ComboScore,
                SnapAimDifficulty = snapAimRating,
                FlowAimDifficulty = flowAimRating
            };

            return attributes;
        }

        // Summation for aim and speed, reducing reward for mixed maps
        public static double SumMechanicalDifficulty(double aim, double speed)
        {
            const double addition_portion = 0.15;

            // We take this min to max ratio as a basepoint to be not changed when addition_portion is changed
            const double balance_base_point = 0.2;
            const double power = 1.1;

            // This is automatically-computed multiplier to avoid manual multiplier balancing when addition_portion is changed
            double multiplier = Math.Pow(1 + Math.Pow(balance_base_point, power), 1.0 / power) /
                Math.Pow(
                    Math.Pow(1 + addition_portion, power) +
                    Math.Pow(balance_base_point + addition_portion, power), 1.0 / power
                );

            double max = Math.Max(aim, speed);

            double difficulty =
                Math.Pow(
                    Math.Pow(aim + addition_portion * max, power) +
                    Math.Pow(speed + addition_portion * max, power), 1.0 / power
                );

            return difficulty * multiplier;
        }

        private double computeTotalAimRating(double aimDifficultyValue, double snapAimDifficultyValue, double flowAimDifficultyValue, Mod[] mods, int totalHits, double approachRate, double overallDifficulty)
        {
            if (mods.Any(m => m is OsuModAutopilot))
                return 0;

            double aimRating = calculateDifficultyRating(aimDifficultyValue);
            double snapAimRating = calculateDifficultyRating(snapAimDifficultyValue);
            double flowAimRating = calculateDifficultyRating(flowAimDifficultyValue);

            if (mods.Any(m => m is OsuModTouchDevice))
            {
                aimRating = Math.Pow(aimRating, 0.83);
                snapAimRating = Math.Pow(snapAimRating, 0.83);
                // no reduce on flow aim rating is intentional
            }

            if (mods.Any(m => m is OsuModRelax))
            {
                aimRating *= 0.9;
                flowAimRating *= 0;
                // no reduce on snap aim rating is intentional, because it's used only in versatility bonus, not as a base
            }

            aimRating = double.Lerp(aimRating, snapAimRating + flowAimRating, AimVersatilityBonus);

            return computeRawAimRating(aimRating, mods, totalHits, approachRate, overallDifficulty);
        }

        private double computeSnapAimRating(double snapAimDifficultyValue, Mod[] mods, int totalHits, double approachRate, double overallDifficulty)
        {
            if (mods.Any(m => m is OsuModAutopilot))
                return 0;

            double snapAimRating = Math.Sqrt(snapAimDifficultyValue) * difficulty_multiplier;

            if (mods.Any(m => m is OsuModTouchDevice))
                snapAimRating = Math.Pow(snapAimRating, 0.83);

            // To ensure that result would not be bigger than normal aim difficulty rating
            if (mods.Any(m => m is OsuModRelax))
                snapAimRating *= 0.9;

            return computeRawAimRating(snapAimRating, mods, totalHits, approachRate, overallDifficulty);
        }

        private double computeFlowAimRating(double flowAimDifficultyValue, Mod[] mods, int totalHits, double approachRate, double overallDifficulty)
        {
            if (mods.Any(m => m is OsuModAutopilot) || mods.Any(m => m is OsuModRelax))
                return 0;

            double flowAimRating = Math.Sqrt(flowAimDifficultyValue) * difficulty_multiplier;

            // To ensure that result would not be bigger than normal aim difficulty rating
            if (mods.Any(m => m is OsuModTouchDevice))
                flowAimRating = Math.Pow(flowAimRating, 0.83);

            return computeRawAimRating(flowAimRating, mods, totalHits, approachRate, overallDifficulty);
        }

        private double computeRawAimRating(double aimRating, Mod[] mods, int totalHits, double approachRate, double overallDifficulty)
        {
            if (mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                aimRating *= 1.0 - magnetisedStrength;
            }

            double ratingMultiplier = 1.0;

            double approachRateLengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                             (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);

            double approachRateFactor = 0.0;
            if (approachRate > 10.33)
                approachRateFactor = 0.3 * (approachRate - 10.33);
            else if (approachRate < 8.0)
                approachRateFactor = 0.05 * (8.0 - approachRate);

            if (mods.Any(h => h is OsuModRelax))
                approachRateFactor = 0.0;

            ratingMultiplier *= 1.0 + approachRateFactor * approachRateLengthBonus; // Buff for longer maps with high AR.

            if (mods.Any(m => m is OsuModHidden))
            {
                double visibilityFactor = calculateAimVisibilityFactor(approachRate);
                ratingMultiplier *= 1.0 + CalculateVisibilityBonus(mods, approachRate, visibilityFactor);
            }

            // It is important to consider accuracy difficulty when scaling with accuracy.
            ratingMultiplier *= 0.98 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 2500;

            return aimRating * Math.Cbrt(ratingMultiplier);
        }

        private double computeSpeedRating(double speedDifficultyValue, Mod[] mods, int totalHits, double approachRate, double overallDifficulty)
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

            double approachRateLengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                             (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);

            double approachRateFactor = 0.0;
            if (approachRate > 10.33)
                approachRateFactor = 0.3 * (approachRate - 10.33);

            if (mods.Any(m => m is OsuModAutopilot))
                approachRateFactor = 0.0;

            ratingMultiplier *= 1.0 + approachRateFactor * approachRateLengthBonus; // Buff for longer maps with high AR.

            if (mods.Any(m => m is OsuModHidden))
            {
                double visibilityFactor = calculateSpeedVisibilityFactor(approachRate);
                ratingMultiplier *= 1.0 + CalculateVisibilityBonus(mods, approachRate, visibilityFactor);
            }

            ratingMultiplier *= 0.95 + Math.Pow(Math.Max(0, overallDifficulty), 2) / 750;

            return speedRating * Math.Cbrt(ratingMultiplier);
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

        private double calculateAimVisibilityFactor(double approachRate)
        {
            const double ar_factor_end_point = 11.5;

            double mechanicalDifficultyFactor = DifficultyCalculationUtils.ReverseLerp(mechanicalDifficultyRating, 5, 10);
            double arFactorStartingPoint = double.Lerp(9, 10.33, mechanicalDifficultyFactor);

            return DifficultyCalculationUtils.ReverseLerp(approachRate, ar_factor_end_point, arFactorStartingPoint);
        }

        private double calculateSpeedVisibilityFactor(double approachRate)
        {
            const double ar_factor_end_point = 11.5;

            double mechanicalDifficultyFactor = DifficultyCalculationUtils.ReverseLerp(mechanicalDifficultyRating, 5, 10);
            double arFactorStartingPoint = double.Lerp(10, 10.33, mechanicalDifficultyFactor);

            return DifficultyCalculationUtils.ReverseLerp(approachRate, ar_factor_end_point, arFactorStartingPoint);
        }

        private static double calculateMechanicalDifficultyRating(double aimDifficultyValue, double speedDifficultyValue)
        {
            double aimValue = OsuStrainSkill.DifficultyToPerformance(calculateDifficultyRating(aimDifficultyValue));
            double speedValue = OsuStrainSkill.DifficultyToPerformance(calculateDifficultyRating(speedDifficultyValue));

            double totalValue = Math.Pow(Math.Pow(aimValue, 1.1) + Math.Pow(speedValue, 1.1), 1 / 1.1);

            return calculateStarRating(totalValue, performance_base_multiplier);
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
                new TotalAim(mods, true),
                new TotalAim(mods, false),
                new Speed(mods),
                new SnapAim(mods),
                new FlowAim(mods),
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
