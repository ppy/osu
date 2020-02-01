// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select.Details;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.SongSelect
{
    [System.ComponentModel.Description("Advanced beatmap statistics display")]
    public class TestSceneAdvancedStats : OsuTestScene
    {
        private TestAdvancedStats advancedStats;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [SetUp]
        public void Setup() => Schedule(() => Child = advancedStats = new TestAdvancedStats
        {
            Width = 500
        });

        private BeatmapInfo exampleBeatmapInfo => new BeatmapInfo
        {
            RulesetID = 0,
            Ruleset = rulesets.AvailableRulesets.First(),
            BaseDifficulty = new BeatmapDifficulty
            {
                CircleSize = 7.2f,
                DrainRate = 1,
                OverallDifficulty = 5.7f,
                ApproachRate = 3.5f
            },
            StarDifficulty = 4.5f
        };

        [Test]
        public void TestNoMod()
        {
            AddStep("set beatmap", () => advancedStats.Beatmap = exampleBeatmapInfo);

            AddStep("no mods selected", () => SelectedMods.Value = Array.Empty<Mod>());

            AddAssert("first bar text is Circle Size", () => advancedStats.ChildrenOfType<SpriteText>().First().Text == "Circle Size");
            AddAssert("circle size bar is white", () => advancedStats.FirstValue.ModBar.AccentColour == Color4.White);
            AddAssert("HP drain bar is white", () => advancedStats.HpDrain.ModBar.AccentColour == Color4.White);
            AddAssert("accuracy bar is white", () => advancedStats.Accuracy.ModBar.AccentColour == Color4.White);
            AddAssert("approach rate bar is white", () => advancedStats.ApproachRate.ModBar.AccentColour == Color4.White);
        }

        [Test]
        public void TestManiaFirstBarText()
        {
            AddStep("set beatmap", () => advancedStats.Beatmap = new BeatmapInfo
            {
                Ruleset = rulesets.GetRuleset(3),
                BaseDifficulty = new BeatmapDifficulty
                {
                    CircleSize = 5,
                    DrainRate = 4.3f,
                    OverallDifficulty = 4.5f,
                    ApproachRate = 3.1f
                },
                StarDifficulty = 8
            });

            AddAssert("first bar text is Key Count", () => advancedStats.ChildrenOfType<SpriteText>().First().Text == "Key Count");
        }

        [Test]
        public void TestEasyMod()
        {
            AddStep("set beatmap", () => advancedStats.Beatmap = exampleBeatmapInfo);

            AddStep("select EZ mod", () =>
            {
                var ruleset = advancedStats.Beatmap.Ruleset.CreateInstance();
                SelectedMods.Value = new[] { ruleset.GetAllMods().OfType<ModEasy>().Single() };
            });

            AddAssert("circle size bar is blue", () => advancedStats.FirstValue.ModBar.AccentColour == colours.BlueDark);
            AddAssert("HP drain bar is blue", () => advancedStats.HpDrain.ModBar.AccentColour == colours.BlueDark);
            AddAssert("accuracy bar is blue", () => advancedStats.Accuracy.ModBar.AccentColour == colours.BlueDark);
            AddAssert("approach rate bar is blue", () => advancedStats.ApproachRate.ModBar.AccentColour == colours.BlueDark);
        }

        [Test]
        public void TestHardRockMod()
        {
            AddStep("set beatmap", () => advancedStats.Beatmap = exampleBeatmapInfo);

            AddStep("select HR mod", () =>
            {
                var ruleset = advancedStats.Beatmap.Ruleset.CreateInstance();
                SelectedMods.Value = new[] { ruleset.GetAllMods().OfType<ModHardRock>().Single() };
            });

            AddAssert("circle size bar is red", () => advancedStats.FirstValue.ModBar.AccentColour == colours.Red);
            AddAssert("HP drain bar is red", () => advancedStats.HpDrain.ModBar.AccentColour == colours.Red);
            AddAssert("accuracy bar is red", () => advancedStats.Accuracy.ModBar.AccentColour == colours.Red);
            AddAssert("approach rate bar is red", () => advancedStats.ApproachRate.ModBar.AccentColour == colours.Red);
        }

        private class TestAdvancedStats : AdvancedStats
        {
            public new StatisticRow FirstValue => base.FirstValue;
            public new StatisticRow HpDrain => base.HpDrain;
            public new StatisticRow Accuracy => base.Accuracy;
            public new StatisticRow ApproachRate => base.ApproachRate;
        }
    }
}
