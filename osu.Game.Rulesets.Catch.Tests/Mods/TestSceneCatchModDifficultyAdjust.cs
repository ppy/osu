// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests.Mods
{
    public partial class TestSceneCatchModDifficultyAdjust : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [Test]
        public void TestRedundancyOnNullValues() => CreateModTest(new ModTestData
        {
            Mod = new CatchModDifficultyAdjust(),
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Fruit { StartTime = 1000 },
                    new Fruit { StartTime = 2000 }
                }
            },
            PassCondition = () => checkRedundancy()
        });

        [Test]
        public void TestRedundancyOnSameValues() => CreateModTest(new ModTestData
        {
            Mod = new CatchModDifficultyAdjust { CircleSize = { Value = 8 }, ApproachRate = { Value = 8 } },
            Beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = 8,
                        ApproachRate = 8
                    }
                },
                HitObjects = new List<HitObject>
                {
                    new Fruit { StartTime = 1000 },
                    new Fruit { StartTime = 2000 }
                }
            },
            PassCondition = () => checkRedundancy()
        });

        [Test]
        public void TestNonRedundancyOnDifferentCircleSize() => CreateModTest(new ModTestData
        {
            Mod = new CatchModDifficultyAdjust { CircleSize = { Value = 10 } },
            Beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = 8
                    }
                },
                HitObjects = new List<HitObject>
                {
                    new Fruit { StartTime = 1000 },
                    new Fruit { StartTime = 2000 }
                }
            },
            PassCondition = () => !checkRedundancy()
        });

        [Test]
        public void TestNonRedundancyOnDifferentApproachRate() => CreateModTest(new ModTestData
        {
            Mod = new CatchModDifficultyAdjust { ApproachRate = { Value = 10 } },
            Beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        ApproachRate = 8
                    }
                },
                HitObjects = new List<HitObject>
                {
                    new Fruit { StartTime = 1000 },
                    new Fruit { StartTime = 2000 }
                }
            },
            PassCondition = () => !checkRedundancy()
        });

        [Test]
        public void TestNonRedundancyOnHardRockOffsetsTrue() => CreateModTest(new ModTestData
        {
            Mod = new CatchModDifficultyAdjust { HardRockOffsets = { Value = true } },
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Fruit { StartTime = 1000 },
                    new Fruit { StartTime = 2000 }
                }
            },
            PassCondition = () => !checkRedundancy()
        });

        [Test]
        public void TestNonRedundancyOnSameCSAndARButHardRockOffsetsTrue() => CreateModTest(new ModTestData
        {
            Mod = new CatchModDifficultyAdjust { CircleSize = { Value = 8 }, ApproachRate = { Value = 8 }, HardRockOffsets = { Value = true } },
            Beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = 8,
                        ApproachRate = 8
                    }
                },
                HitObjects = new List<HitObject>
                {
                    new Fruit { StartTime = 1000 },
                    new Fruit { StartTime = 2000 }
                }
            },
            PassCondition = () => !checkRedundancy()
        });

        private bool checkRedundancy()
        {
            return CurrentTestData.Mods.Single(mod => mod.GetType() == typeof(CatchModDifficultyAdjust)).IsRedundant(CurrentTestData.Beatmap);
        }
    }
}
