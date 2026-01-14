// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Utils;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaDifficultyCalculator : DifficultyCalculator
    {
        private const double difficulty_multiplier = 1;
        private const double final_scaling_factor = 0.975;
        private const double strain_threshold = 0.01;

        // Difficulty calculation weights
        private const double high_percentile_weight = 0.22; // 0.25 * 0.88
        private const double mid_percentile_weight = 0.188; // 0.20 * 0.94
        private const double power_mean_weight = 0.55;

        private const double rescale_high_threshold = 9.0;
        private const double rescale_high_factor = 1.2;

        public readonly double[] DifficultyPercentilesHigh = { 0.945, 0.935, 0.925, 0.915 };
        public readonly double[] DifficultyPercentilesMid = { 0.845, 0.835, 0.825, 0.815 };

        private readonly bool isForCurrentRuleset;

        public override int Version => 20241007;

        public ManiaDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            isForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.MatchesOnlineID(ruleset);
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new ManiaDifficultyAttributes { Mods = mods };

            var totalSkill = (Strain)skills.First(s => s is Strain);

            var objectStrains = totalSkill.GetObjectStrains().ToList();
            double weightedNoteCount = totalSkill.GetWeightedNoteCount();

            double sr = calculateDifficultyValue(objectStrains, weightedNoteCount);

            return new ManiaDifficultyAttributes
            {
                StarRating = sr * difficulty_multiplier,
                Mods = mods,
                MaxCombo = beatmap.HitObjects.Sum(maxComboForObject),
            };
        }

        private double calculateDifficultyValue(List<double> combinedStrains, double weightedNoteCount)
        {
            double[] sorted = combinedStrains.Where(s => s > 0).ToArray();
            if (sorted.Length == 0) return 0.0;

            Array.Sort(sorted);

            double highPercentileMean = DifficultyValueUtils.CalculatePercentileMean(sorted, DifficultyPercentilesHigh);
            double midPercentileMean = DifficultyValueUtils.CalculatePercentileMean(sorted, DifficultyPercentilesMid);
            double powerMean = DifficultyValueUtils.CalculatePowerMean(sorted, 5.0);

            double rawDifficulty = high_percentile_weight * highPercentileMean +
                                   mid_percentile_weight * midPercentileMean +
                                   power_mean_weight * powerMean;

            return applyFinalScaling(rawDifficulty, weightedNoteCount);
        }

        private double applyFinalScaling(double rawDifficulty, double weightedNoteCount)
        {
            double scaled = rawDifficulty * weightedNoteCount / (weightedNoteCount + 60.0);

            if (scaled > rescale_high_threshold)
            {
                scaled = rescale_high_threshold + (scaled - rescale_high_threshold) /
                    rescale_high_factor;
            }

            return scaled * final_scaling_factor;
        }

        private static int maxComboForObject(HitObject hitObject)
        {
            if (hitObject is HoldNote hold)
                return 1 + (int)((hold.EndTime - hold.StartTime) / 100);

            return 1;
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            var sortedObjects = beatmap.HitObjects.ToArray();
            int totalColumns = ((ManiaBeatmap)beatmap).TotalColumns;

            LegacySortHelper<HitObject>.Sort(sortedObjects,
                Comparer<HitObject>.Create((a, b) => (int)Math.Round(a.StartTime) - (int)Math.Round(b.StartTime)));

            if (sortedObjects.Length <= 1)
                return Array.Empty<DifficultyHitObject>();

            var objects = new List<DifficultyHitObject>(Math.Max(0, sortedObjects.Length - 1));
            List<DifficultyHitObject>[] perColumnObjects = new List<DifficultyHitObject>[totalColumns];

            for (int column = 0; column < totalColumns; column++)
                perColumnObjects[column] = new List<DifficultyHitObject>();

            for (int i = 1; i < sortedObjects.Length; i++)
            {
                var maniaDifficultyHitObject = new ManiaDifficultyHitObject(
                    sortedObjects[i],
                    sortedObjects[i - 1],
                    clockRate,
                    objects,
                    perColumnObjects,
                    objects.Count
                );

                objects.Add(maniaDifficultyHitObject);
                perColumnObjects[maniaDifficultyHitObject.Column].Add(maniaDifficultyHitObject);
            }

            ManiaDifficultyPreprocessor.ProcessAndAssign(objects, beatmap);

            return objects;
        }

        protected override IEnumerable<DifficultyHitObject> SortObjects(IEnumerable<DifficultyHitObject> input) => input;

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            return new Skill[]
            {
                new Strain(mods),
            };
        }

        protected override Mod[] DifficultyAdjustmentMods
        {
            get
            {
                var mods = new Mod[]
                {
                    new ManiaModDoubleTime(),
                    new ManiaModHalfTime(),
                    new ManiaModEasy(),
                    new ManiaModHardRock(),
                };

                if (isForCurrentRuleset)
                    return mods;

                return mods.Concat(new Mod[]
                {
                    new ManiaModKey1(),
                    new ManiaModKey2(),
                    new ManiaModKey3(),
                    new ManiaModKey4(),
                    new ManiaModKey5(),
                    new MultiMod(new ManiaModKey5(), new ManiaModDualStages()),
                    new ManiaModKey6(),
                    new MultiMod(new ManiaModKey6(), new ManiaModDualStages()),
                    new ManiaModKey7(),
                    new MultiMod(new ManiaModKey7(), new ManiaModDualStages()),
                    new ManiaModKey8(),
                    new MultiMod(new ManiaModKey8(), new ManiaModDualStages()),
                    new ManiaModKey9(),
                    new MultiMod(new ManiaModKey9(), new ManiaModDualStages()),
                }).ToArray();
            }
        }
    }
}
