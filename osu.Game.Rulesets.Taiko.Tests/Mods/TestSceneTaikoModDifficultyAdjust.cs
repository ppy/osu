// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public partial class TestSceneTaikoModDifficultyAdjust : TaikoModTestScene
    {
        [Test]
        public void TestRedundancyOnNullValues() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModDifficultyAdjust(),
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit { StartTime = 1000 },
                    new Hit { StartTime = 2000 }
                }
            },
            PassCondition = () => checkRedundancy()
        });

        [Test]
        public void TestRedundancyOnScrollSpeed1() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModDifficultyAdjust { ScrollSpeed = { Value = 1 } },
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit { StartTime = 1000 },
                    new Hit { StartTime = 2000 }
                }
            },
            PassCondition = () => checkRedundancy()
        });

        [Test]
        public void TestNonRedundancyOnScrollSpeed4() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModDifficultyAdjust { ScrollSpeed = { Value = 4 } },
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit { StartTime = 1000 },
                    new Hit { StartTime = 2000 }
                }
            },
            PassCondition = () => !checkRedundancy()
        });

        private bool checkRedundancy()
        {
            return CurrentTestData.Mods.Single(mod => mod.GetType() == typeof(TaikoModDifficultyAdjust)).IsRedundant(CurrentTestData.Beatmap);
        }
    }
}
