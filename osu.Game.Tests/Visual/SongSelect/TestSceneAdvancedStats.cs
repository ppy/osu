// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select.Details;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.SongSelect
{
    [System.ComponentModel.Description("Advanced beatmap statistics display")]
    public partial class TestSceneAdvancedStats : OsuTestScene
    {
        private TestAdvancedStats advancedStats;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [SetUp]
        public void Setup() => Schedule(() => Child = advancedStats = new TestAdvancedStats
        {
            Width = 500,
        });

        private BeatmapInfo exampleBeatmapInfo => new BeatmapInfo
        {
            Ruleset = rulesets.AvailableRulesets.First(),
            Difficulty = new BeatmapDifficulty
            {
                CircleSize = 7.2f,
                DrainRate = 3,
                OverallDifficulty = 5.7f,
                ApproachRate = 3.5f
            },
            StarRating = 4.5f
        };

        [Test]
        public void TestNoMod()
        {
            AddStep("set beatmap", () => advancedStats.BeatmapInfo = exampleBeatmapInfo);

            AddStep("no mods selected", () => SelectedMods.Value = Array.Empty<Mod>());

            AddAssert("first bar text is correct", () => advancedStats.ChildrenOfType<SpriteText>().First().Text == BeatmapsetsStrings.ShowStatsCs);
            AddAssert("circle size bar is white", () => barIsWhite(advancedStats.FirstValue));
            AddAssert("HP drain bar is white", () => barIsWhite(advancedStats.HpDrain));
            AddAssert("accuracy bar is white", () => barIsWhite(advancedStats.Accuracy));
            AddAssert("approach rate bar is white", () => barIsWhite(advancedStats.ApproachRate));
        }

        [Test]
        public void TestFirstBarText()
        {
            AddStep("set ruleset to mania", () => advancedStats.Ruleset.Value = new ManiaRuleset().RulesetInfo);
            AddAssert("first bar text is correct", () => advancedStats.ChildrenOfType<SpriteText>().First().Text == BeatmapsetsStrings.ShowStatsCsMania);
            AddStep("set ruleset to osu", () => advancedStats.Ruleset.Value = new OsuRuleset().RulesetInfo);
            AddAssert("first bar text is correct", () => advancedStats.ChildrenOfType<SpriteText>().First().Text == BeatmapsetsStrings.ShowStatsCs);
        }

        [Test]
        public void TestEasyMod()
        {
            AddStep("set beatmap", () => advancedStats.BeatmapInfo = exampleBeatmapInfo);

            AddStep("select EZ mod", () =>
            {
                var ruleset = advancedStats.BeatmapInfo.Ruleset.CreateInstance().AsNonNull();
                SelectedMods.Value = new[] { ruleset.CreateMod<ModEasy>() };
            });

            AddAssert("circle size bar is blue", () => barIsBlue(advancedStats.FirstValue));
            AddAssert("HP drain bar is blue", () => barIsBlue(advancedStats.HpDrain));
            AddAssert("accuracy bar is blue", () => barIsBlue(advancedStats.Accuracy));
            AddAssert("approach rate bar is blue", () => barIsBlue(advancedStats.ApproachRate));
        }

        [Test]
        public void TestHardRockMod()
        {
            AddStep("set beatmap", () => advancedStats.BeatmapInfo = exampleBeatmapInfo);

            AddStep("select HR mod", () =>
            {
                var ruleset = advancedStats.BeatmapInfo.Ruleset.CreateInstance().AsNonNull();
                SelectedMods.Value = new[] { ruleset.CreateMod<ModHardRock>() };
            });

            AddAssert("circle size bar is red", () => barIsRed(advancedStats.FirstValue));
            AddAssert("HP drain bar is red", () => barIsRed(advancedStats.HpDrain));
            AddAssert("accuracy bar is red", () => barIsRed(advancedStats.Accuracy));
            AddAssert("approach rate bar is red", () => barIsRed(advancedStats.ApproachRate));
        }

        [Test]
        public void TestUnchangedDifficultyAdjustMod()
        {
            AddStep("set beatmap", () => advancedStats.BeatmapInfo = exampleBeatmapInfo);

            AddStep("select unchanged Difficulty Adjust mod", () =>
            {
                var ruleset = advancedStats.BeatmapInfo.Ruleset.CreateInstance().AsNonNull();
                var difficultyAdjustMod = ruleset.CreateMod<ModDifficultyAdjust>().AsNonNull();
                difficultyAdjustMod.ReadFromDifficulty(advancedStats.BeatmapInfo.Difficulty);
                SelectedMods.Value = new[] { difficultyAdjustMod };
            });

            AddAssert("circle size bar is white", () => barIsWhite(advancedStats.FirstValue));
            AddAssert("HP drain bar is white", () => barIsWhite(advancedStats.HpDrain));
            AddAssert("accuracy bar is white", () => barIsWhite(advancedStats.Accuracy));
            AddAssert("approach rate bar is white", () => barIsWhite(advancedStats.ApproachRate));
        }

        [Test]
        public void TestChangedDifficultyAdjustMod()
        {
            AddStep("set beatmap", () => advancedStats.BeatmapInfo = exampleBeatmapInfo);

            AddStep("select changed Difficulty Adjust mod", () =>
            {
                var ruleset = advancedStats.BeatmapInfo.Ruleset.CreateInstance().AsNonNull();
                var difficultyAdjustMod = ruleset.CreateMod<OsuModDifficultyAdjust>().AsNonNull();
                var originalDifficulty = advancedStats.BeatmapInfo.Difficulty;

                difficultyAdjustMod.ReadFromDifficulty(originalDifficulty);
                difficultyAdjustMod.DrainRate.Value = originalDifficulty.DrainRate - 0.5f;
                difficultyAdjustMod.ApproachRate.Value = originalDifficulty.ApproachRate + 2.2f;
                SelectedMods.Value = new[] { difficultyAdjustMod };
            });

            AddAssert("circle size bar is white", () => barIsWhite(advancedStats.FirstValue));
            AddAssert("drain rate bar is blue", () => barIsBlue(advancedStats.HpDrain));
            AddAssert("accuracy bar is white", () => barIsWhite(advancedStats.Accuracy));
            AddAssert("approach rate bar is red", () => barIsRed(advancedStats.ApproachRate));
        }

        private bool barIsWhite(AdvancedStats.StatisticRow row) => row.ModBar.AccentColour == Color4.White;
        private bool barIsBlue(AdvancedStats.StatisticRow row) => row.ModBar.AccentColour == colours.BlueDark;
        private bool barIsRed(AdvancedStats.StatisticRow row) => row.ModBar.AccentColour == colours.Red;

        private partial class TestAdvancedStats : AdvancedStats
        {
            public new StatisticRow FirstValue => base.FirstValue;
            public new StatisticRow HpDrain => base.HpDrain;
            public new StatisticRow Accuracy => base.Accuracy;
            public new StatisticRow ApproachRate => base.ApproachRate;
        }
    }
}
