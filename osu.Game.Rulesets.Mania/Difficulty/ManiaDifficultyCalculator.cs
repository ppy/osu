// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaDifficultyCalculator : DifficultyCalculator
    {
        private const double star_scaling_factor = 0.018;

        private readonly bool isForCurrentRuleset;

        public ManiaDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            isForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.Equals(ruleset.RulesetInfo);
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate, double upTo = double.PositiveInfinity)
        {
            if (beatmap.HitObjects.Count == 0)
                return new ManiaDifficultyAttributes { Mods = mods, Skills = skills };

            HitWindows hitWindows = new ManiaHitWindows();
            hitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            return new ManiaDifficultyAttributes
            {
                StarRating = difficultyValue(skills) * star_scaling_factor,
                Mods = mods,
                // Todo: This int cast is temporary to achieve 1:1 results with osu!stable, and should be removed in the future
                GreatHitWindow = (int)(hitWindows.WindowFor(HitResult.Great)) / clockRate,
                Skills = skills
            };
        }

        private double difficultyValue(Skill[] skills)
        {
            // Preprocess the strains to find the maximum overall + individual (aggregate) strain from each section
            var overall = skills.OfType<Overall>().Single();
            var aggregatePeaks = new List<double>(Enumerable.Repeat(0.0, overall.StrainPeaks.Count));

            foreach (var individual in skills.OfType<Individual>())
            {
                for (int i = 0; i < individual.StrainPeaks.Count; i++)
                {
                    double aggregate = individual.StrainPeaks[i] + overall.StrainPeaks[i];

                    if (aggregate > aggregatePeaks[i])
                        aggregatePeaks[i] = aggregate;
                }
            }

            aggregatePeaks.Sort((a, b) => b.CompareTo(a)); // Sort from highest to lowest strain.

            double difficulty = 0;
            double weight = 1;

            // Difficulty is the weighted sum of the highest strains from every section.
            foreach (double strain in aggregatePeaks)
            {
                difficulty += strain * weight;
                weight *= 0.9;
            }

            return difficulty;
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
                yield return new ManiaDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], clockRate);
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap)
        {
            int columnCount = ((ManiaBeatmap)beatmap).TotalColumns;

            var skills = new List<Skill> { new Overall(columnCount) };

            for (int i = 0; i < columnCount; i++)
                skills.Add(new Individual(i, columnCount));

            return skills.ToArray();
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

                // if we are a convert, we can be played in any key mod.
                return mods.Concat(new Mod[]
                {
                    new ManiaModKey1(),
                    new ManiaModKey2(),
                    new ManiaModKey3(),
                    new ManiaModKey4(),
                    new ManiaModKey5(),
                    new ManiaModKey6(),
                    new ManiaModKey7(),
                    new ManiaModKey8(),
                    new ManiaModKey9(),
                }).ToArray();
            }
        }
    }
}
