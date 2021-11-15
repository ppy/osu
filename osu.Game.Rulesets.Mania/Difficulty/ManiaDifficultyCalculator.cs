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
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaDifficultyCalculator : DifficultyCalculator
    {
        private const double star_scaling_factor = 0.018;

        private readonly bool isForCurrentRuleset;
        private readonly double originalOverallDifficulty;

        public ManiaDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            isForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.MatchesOnlineID(ruleset);
            originalOverallDifficulty = beatmap.BeatmapInfo.Difficulty.OverallDifficulty;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new ManiaDifficultyAttributes { Mods = mods, Skills = skills };

            HitWindows hitWindows = new ManiaHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            return new ManiaDifficultyAttributes
            {
                StarRating = skills[0].DifficultyValue() * star_scaling_factor,
                Mods = mods,
                GreatHitWindow = Math.Ceiling(getHitWindow300(mods) / clockRate),
                ScoreMultiplier = getScoreMultiplier(mods),
                MaxCombo = beatmap.HitObjects.Sum(h => h is HoldNote ? 2 : 1),
                Skills = skills
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            var sortedObjects = beatmap.HitObjects.ToArray();

            LegacySortHelper<HitObject>.Sort(sortedObjects, Comparer<HitObject>.Create((a, b) => (int)Math.Round(a.StartTime) - (int)Math.Round(b.StartTime)));

            for (int i = 1; i < sortedObjects.Length; i++)
                yield return new ManiaDifficultyHitObject(sortedObjects[i], sortedObjects[i - 1], clockRate);
        }

        // Sorting is done in CreateDifficultyHitObjects, since the full list of hitobjects is required.
        protected override IEnumerable<DifficultyHitObject> SortObjects(IEnumerable<DifficultyHitObject> input) => input;

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate) => new Skill[]
        {
            new Strain(mods, ((ManiaBeatmap)Beatmap).TotalColumns)
        };

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

        private int getHitWindow300(Mod[] mods)
        {
            if (isForCurrentRuleset)
            {
                double od = Math.Min(10.0, Math.Max(0, 10.0 - originalOverallDifficulty));
                return applyModAdjustments(34 + 3 * od, mods);
            }

            if (Math.Round(originalOverallDifficulty) > 4)
                return applyModAdjustments(34, mods);

            return applyModAdjustments(47, mods);

            static int applyModAdjustments(double value, Mod[] mods)
            {
                if (mods.Any(m => m is ManiaModHardRock))
                    value /= 1.4;
                else if (mods.Any(m => m is ManiaModEasy))
                    value *= 1.4;

                if (mods.Any(m => m is ManiaModDoubleTime))
                    value *= 1.5;
                else if (mods.Any(m => m is ManiaModHalfTime))
                    value *= 0.75;

                return (int)value;
            }
        }

        private double getScoreMultiplier(Mod[] mods)
        {
            double scoreMultiplier = 1;

            foreach (var m in mods)
            {
                switch (m)
                {
                    case ManiaModNoFail _:
                    case ManiaModEasy _:
                    case ManiaModHalfTime _:
                        scoreMultiplier *= 0.5;
                        break;
                }
            }

            var maniaBeatmap = (ManiaBeatmap)Beatmap;
            int diff = maniaBeatmap.TotalColumns - maniaBeatmap.OriginalTotalColumns;

            if (diff > 0)
                scoreMultiplier *= 0.9;
            else if (diff < 0)
                scoreMultiplier *= 0.9 + 0.04 * diff;

            return scoreMultiplier;
        }
    }
}
