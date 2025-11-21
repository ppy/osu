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

        // Pre-computed constants
        private const double scaled_cross_column_factor = 0.2352941176;
        private const double scaled_jack_factor = 0.01666666667;
        private const double scaled_pressing_factor = 0.025;
        private const double scaled_unevenness_factor = 0.5;
        private const double scaled_release_factor = 0.1666666667;

        // Difficulty calculation weights
        private const double high_percentile_weight = 0.22; // 0.25 * 0.88
        private const double mid_percentile_weight = 0.188; // 0.20 * 0.94
        private const double power_mean_weight = 0.55;

        private const double rescale_high_threshold = 9.0;
        private const double rescale_high_factor = 1.0 / 1.2; // should be 1.0 / 1.2 in the future

        public readonly double[] DifficultyPercentilesHigh = { 0.945, 0.935, 0.925, 0.915 };
        public readonly double[] DifficultyPercentilesMid = { 0.845, 0.835, 0.825, 0.815 };

        private readonly bool isForCurrentRuleset;
        private Dictionary<(int, int, int, int, int, int, int), double>? strainCache;

        public override int Version => 20241008;

        public ManiaDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            isForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.MatchesOnlineID(ruleset);
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new ManiaDifficultyAttributes { Mods = mods };

            var sameColumnSkill = (Jack)skills[0];
            var crossColumnSkill = (CrossColumn)skills[1];
            var pressingIntensitySkill = (PressingIntensity)skills[2];
            var unevennessSkill = (Unevenness)skills[3];
            var releaseSkill = (Release)skills[4];
            var localNoteDensitySkill = (LocalNoteDensity)skills[5];

            var combinedStrains = combineStrains(
                sameColumnSkill.GetObjectStrains().ToList(),
                crossColumnSkill.GetObjectStrains().ToList(),
                pressingIntensitySkill.GetObjectStrains().ToList(),
                unevennessSkill.GetObjectStrains().ToList(),
                releaseSkill.GetObjectStrains().ToList(),
                localNoteDensitySkill.GetObjectStrains().ToList(),
                localNoteDensitySkill.GetActiveKeyStrains().ToList()
            );

            double sr = calculateDifficultyValue(combinedStrains, localNoteDensitySkill);
            int totalColumns = ((ManiaBeatmap)Beatmap).TotalColumns;

            return new ManiaDifficultyAttributes
            {
                StarRating = sr * difficulty_multiplier,
                CrossColumnDifficulty = crossColumnSkill.DifficultyValue() * scaled_cross_column_factor / totalColumns,
                JackDifficulty = sameColumnSkill.DifficultyValue() * scaled_jack_factor,
                PressingIntensityDifficulty = pressingIntensitySkill.DifficultyValue() * scaled_pressing_factor,
                UnevennessDifficulty = unevennessSkill.DifficultyValue() * scaled_unevenness_factor,
                ReleaseDifficulty = releaseSkill.DifficultyValue() * scaled_release_factor,
                Mods = mods,
                MaxCombo = beatmap.HitObjects.Sum(maxComboForObject),
            };
        }

        private List<double> combineStrains(
            IReadOnlyList<double> jackStrains,
            IReadOnlyList<double> crossStrains,
            IReadOnlyList<double> pressingStrains,
            IReadOnlyList<double> unevennessStrains,
            IReadOnlyList<double> releaseStrains,
            IReadOnlyList<double> localNoteStrains,
            IReadOnlyList<double> activeKeyStrains)
        {
            strainCache ??= new Dictionary<(int, int, int, int, int, int, int), double>();
            int count = new[]
            {
                jackStrains.Count,
                crossStrains.Count,
                pressingStrains.Count,
                unevennessStrains.Count,
                releaseStrains.Count,
                localNoteStrains.Count,
                activeKeyStrains.Count
            }.Max();

            var combinedStrains = new List<double>(count);
            static double valueOrZero(IReadOnlyList<double> list, int i) => i < list.Count ? list[i] : 0.0;

            for (int i = 0; i < count; i++)
            {
                double sameColumn = valueOrZero(jackStrains, i);
                double crossColumn = valueOrZero(crossStrains, i);
                double pressingIntensity = valueOrZero(pressingStrains, i);
                double unevenness = valueOrZero(unevennessStrains, i);
                double release = valueOrZero(releaseStrains, i);

                if (sameColumn < strain_threshold && crossColumn < strain_threshold &&
                    pressingIntensity < strain_threshold && unevenness < strain_threshold &&
                    release < strain_threshold)
                {
                    combinedStrains.Add(0.0);
                    continue;
                }

                double localNoteCount = valueOrZero(localNoteStrains, i);
                double activeKeyCount = valueOrZero(activeKeyStrains, i);

                var cacheKey = (
                    (int)Math.Round(sameColumn * 1000),
                    (int)Math.Round(crossColumn * 1000),
                    (int)Math.Round(pressingIntensity * 1000),
                    (int)Math.Round(unevenness * 1000),
                    (int)Math.Round(release * 1000),
                    (int)Math.Round(localNoteCount * 1000),
                    (int)Math.Round(activeKeyCount * 1000)
                );

                if (strainCache.TryGetValue(cacheKey, out double cachedResult))
                {
                    combinedStrains.Add(cachedResult);
                    continue;
                }

                double clampedSameColumn = Math.Min(sameColumn, 8.0 + 0.85 * sameColumn);

                // Adjust unevenness impact based on how many keys are active
                double unevennessKeyAdjustment = 1.0;
                if (unevenness > 0.0 && activeKeyCount > 0.0)
                    unevennessKeyAdjustment = Math.Pow(unevenness, 3.0 / activeKeyCount);

                // Combine unevenness with same-column difficulty
                double unevennessSameColumnComponent = unevennessKeyAdjustment * clampedSameColumn;
                double firstComponent = 0.4 * Math.Pow(unevennessSameColumnComponent, 1.5);

                // Combine unevenness with pressing intensity and release difficulty
                double releaseComponent = release * 35.0 / (localNoteCount + 8.0);
                double unevennessPressingReleaseComponent = Math.Pow(unevenness, 2.0 / 3.0) * (0.8 * pressingIntensity + releaseComponent);
                double secondComponent = 0.6 * Math.Pow(unevennessPressingReleaseComponent, 1.5);

                // Main strain difficulty combining both components
                double totalStrainDifficulty = Math.Pow(firstComponent + secondComponent, 2.0 / 3.0);

                // Cross-column coordination component
                double twistComponent = (unevennessKeyAdjustment * crossColumn) / (crossColumn + totalStrainDifficulty + 1.0);
                double poweredTwist = twistComponent > 0.0 ? twistComponent * Math.Sqrt(twistComponent) : 0.0;

                double finalStrain = 2.7 * Math.Sqrt(totalStrainDifficulty) * poweredTwist + totalStrainDifficulty * 0.27;

                strainCache[cacheKey] = finalStrain;
                combinedStrains.Add(finalStrain);
            }

            return combinedStrains;
        }

        private double calculateDifficultyValue(List<double> combinedStrains, LocalNoteDensity localNoteDensitySkill)
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

            return applyFinalScaling(rawDifficulty, localNoteDensitySkill);
        }

        private double applyFinalScaling(double rawDifficulty, LocalNoteDensity localNoteDensitySkill)
        {
            double totalCurrentNotes = localNoteDensitySkill.GetTotalNotesWithWeight();
            double scaled = rawDifficulty * totalCurrentNotes / (totalCurrentNotes + 60.0);

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
                new Jack(mods),
                new CrossColumn(mods),
                new PressingIntensity(mods),
                new Unevenness(mods),
                new Release(mods),
                new LocalNoteDensity(mods),
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
