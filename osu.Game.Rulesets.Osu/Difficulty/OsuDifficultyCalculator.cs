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

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const double difficulty_multiplier = 0.0675;

        public override int Version => 20241007;

        public OsuDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        // Increasing this multiplier buffs versatile aim+flow maps
        public static double AimVersatilityBonus = 0.08;

        // Increasing this multiplier nerfs mixed aim+speed map (but not snapaim + flowaim!)
        public static double MechanicsAdditionPortion => 0.13;

        // This increases multiplier on both aim and speed. Exist for easier adjust of both skills after changing the MechanicsAdditionPortion
        public static double MechanicsMultiplier => 0.935;

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods };

            var aim = skills.OfType<TotalAim>().Single(a => a.IncludeSliders);
            double aimRating = Math.Sqrt(aim.DifficultyValue()) * difficulty_multiplier;
            double aimDifficultyStrainCount = aim.CountTopWeightedStrains();
            double difficultSliders = aim.GetDifficultSliders();

            double snapAimRating = Math.Sqrt(skills.OfType<SnapAim>().Single().DifficultyValue()) * difficulty_multiplier;
            double flowAimRating = Math.Sqrt(skills.OfType<FlowAim>().Single().DifficultyValue()) * difficulty_multiplier;

            var aimWithoutSliders = skills.OfType<TotalAim>().Single(a => !a.IncludeSliders);
            double aimRatingNoSliders = Math.Sqrt(aimWithoutSliders.DifficultyValue()) * difficulty_multiplier;

            var speed = skills.OfType<Speed>().Single();
            double speedRating = Math.Sqrt(speed.DifficultyValue()) * difficulty_multiplier;
            double speedNotes = speed.RelevantNoteCount();
            double speedDifficultyStrainCount = speed.CountTopWeightedStrains();

            var flashlight = skills.OfType<Flashlight>().SingleOrDefault();
            double flashlightRating = flashlight == null ? 0.0 : Math.Sqrt(flashlight.DifficultyValue()) * difficulty_multiplier;

            // Warning - this is very debatable but I think it's reasonable to assume that if map has 10 star flowaim difficulty - player who plays it must have at least 6 star snapaim skill
            snapAimRating = Math.Max(snapAimRating, flowAimRating * 0.6);

            // Adjust aim to reward more versatile maps
            aimRating = aimRating * (1 - AimVersatilityBonus) + (snapAimRating + flowAimRating) * AimVersatilityBonus;
            aimRatingNoSliders = aimRatingNoSliders * (1 - AimVersatilityBonus) + (snapAimRating + flowAimRating) * AimVersatilityBonus;
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
            else if (mods.Any(h => h is OsuModAutopilot))
            {
                speedRating *= 0.5;
                aimRating = 0.0;
                flashlightRating *= 0.4;
            }

            double baseAimPerformance = OsuStrainSkill.DifficultyToPerformance(aimRating);
            double baseSpeedPerformance = OsuStrainSkill.DifficultyToPerformance(speedRating);
            double baseFlashlightPerformance = 0.0;

            if (mods.Any(h => h is OsuModFlashlight))
                baseFlashlightPerformance = Flashlight.DifficultyToPerformance(flashlightRating);

            if (baseAimPerformance > baseSpeedPerformance)
                baseSpeedPerformance += (baseAimPerformance - baseSpeedPerformance) * MechanicsAdditionPortion;
            else
                baseAimPerformance += (baseSpeedPerformance - baseAimPerformance) * MechanicsAdditionPortion;

            double basePerformance =
                Math.Pow(
                    Math.Pow(baseAimPerformance, 1.1) +
                    Math.Pow(baseSpeedPerformance, 1.1) +
                    Math.Pow(baseFlashlightPerformance, 1.1), 1.0 / 1.1
                );

            double starRating = basePerformance > 0.00001
                ? Math.Cbrt(OsuPerformanceCalculator.PERFORMANCE_BASE_MULTIPLIER) * 0.027 * (Math.Cbrt(100000 / Math.Pow(2, 1 / 1.1) * basePerformance) + 4)
                : 0;

            double drainRate = beatmap.Difficulty.DrainRate;

            int hitCirclesCount = beatmap.HitObjects.Count(h => h is HitCircle);
            int sliderCount = beatmap.HitObjects.Count(h => h is Slider);
            int spinnerCount = beatmap.HitObjects.Count(h => h is Spinner);

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
                AimDifficultStrainCount = aimDifficultyStrainCount,
                SpeedDifficultStrainCount = speedDifficultyStrainCount,
                DrainRate = drainRate,
                MaxCombo = beatmap.GetMaxCombo(),
                HitCircleCount = hitCirclesCount,
                SliderCount = sliderCount,
                SpinnerCount = spinnerCount,
                SnapAimDifficulty = snapAimRating,
                FlowAimDifficulty = flowAimRating
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
            new MultiMod(new OsuModFlashlight(), new OsuModHidden())
        };
    }
}
