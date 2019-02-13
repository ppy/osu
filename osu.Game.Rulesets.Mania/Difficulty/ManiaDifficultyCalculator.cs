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
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaDifficultyCalculator : DifficultyCalculator
    {
        private const double star_scaling_factor = 0.018;

        private int columnCount;

        private readonly bool isForCurrentRuleset;

        public ManiaDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            isForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.Equals(ruleset.RulesetInfo);
        }

        protected override void PopulateAttributes(DifficultyAttributes attributes, IBeatmap beatmap, Skill[] skills, double timeRate)
        {
            var maniaAttributes = (ManiaDifficultyAttributes)attributes;

            var overallStrain = skills.OfType<Overall>().Single().DifficultyValue();
            var highestIndividual = skills.OfType<Individual>().Max(s => s.DifficultyValue());

            maniaAttributes.StarRating = (overallStrain + highestIndividual) * star_scaling_factor;

            // Todo: This int cast is temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            maniaAttributes.GreatHitWindow = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / timeRate;
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double timeRate)
        {
            columnCount = ((ManiaBeatmap)beatmap).TotalColumns;

            for (int i = 1; i < beatmap.HitObjects.Count; i++)
                yield return new ManiaDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], timeRate);
        }

        protected override Skill[] CreateSkills()
        {
            var skills = new List<Skill> { new Overall(columnCount) };

            for (int i = 0; i < columnCount; i++)
                skills.Add(new Individual(i, columnCount));

            return skills.ToArray();
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(Mod[] mods) => new ManiaDifficultyAttributes(mods);

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
