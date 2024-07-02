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
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneDifficultySettingsContent : SongSelectComponentsTestScene
    {
        private DifficultySettingsContent? difficultySettingsContent;
        private float relativeWidth;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddSliderStep("change relative width", 0, 1f, 0.5f, v =>
            {
                if (difficultySettingsContent != null)
                    difficultySettingsContent.Width = v;

                relativeWidth = v;
            });
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set content", () =>
            {
                Child = difficultySettingsContent = new DifficultySettingsContent
                {
                    Width = relativeWidth,
                };
            });
        }

        private Beatmap exampleBeatmap => new Beatmap
        {
            BeatmapInfo = new BeatmapInfo
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
            }
        };

        [Test]
        public void TestNoMod()
        {
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(exampleBeatmap));

            AddStep("no mods selected", () => SelectedMods.Value = Array.Empty<Mod>());

            AddAssert("first bar text is correct", () => difficultySettingsContent.ChildrenOfType<SpriteText>().First().Text.ToString(), () => Is.EqualTo("Circle "));
            AddAssert("circle size bar is white", () => barIsWhiteAt(0));
            AddAssert("HP drain bar is white", () => barIsWhiteAt(1));
            AddAssert("accuracy bar is white", () => barIsWhiteAt(2));
            AddAssert("approach rate bar is white", () => barIsWhiteAt(3));
        }

        [Test]
        public void TestFirstBarText()
        {
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(exampleBeatmap));
            AddStep("set ruleset to mania", () => Ruleset.Value = new ManiaRuleset().RulesetInfo);
            AddAssert("first bar text is correct", () => difficultySettingsContent.ChildrenOfType<SpriteText>().First().Text.ToString(), () => Is.EqualTo("Key "));
            AddStep("set ruleset to osu", () => Ruleset.Value = new OsuRuleset().RulesetInfo);
            AddAssert("first bar text is correct", () => difficultySettingsContent.ChildrenOfType<SpriteText>().First().Text.ToString(), () => Is.EqualTo("Circle "));
        }

        [Test]
        public void TestEasyMod()
        {
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(exampleBeatmap));

            AddStep("select EZ mod", () =>
            {
                var ruleset = BeatmapInfo.Value!.Ruleset.CreateInstance();
                SelectedMods.Value = new[] { ruleset.CreateMod<ModEasy>() };
            });

            AddAssert("circle size bar is blue", () => modBarIsBlueAt(0));
            AddAssert("HP drain bar is blue", () => modBarIsBlueAt(1));
            AddAssert("accuracy bar is blue", () => modBarIsBlueAt(2));
            AddAssert("approach rate bar is blue", () => modBarIsBlueAt(3));
        }

        [Test]
        public void TestHardRockMod()
        {
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(exampleBeatmap));

            AddStep("select HR mod", () =>
            {
                var ruleset = BeatmapInfo.Value!.Ruleset.CreateInstance();
                SelectedMods.Value = new[] { ruleset.CreateMod<ModHardRock>() };
            });

            AddAssert("circle size bar is red", () => modBarIsRedAt(0));
            AddAssert("HP drain bar is red", () => modBarIsRedAt(1));
            AddAssert("accuracy bar is red", () => modBarIsRedAt(2));
            AddAssert("approach rate bar is red", () => modBarIsRedAt(3));
        }

        [Test]
        public void TestUnchangedDifficultyAdjustMod()
        {
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(exampleBeatmap));

            AddStep("select unchanged Difficulty Adjust mod", () =>
            {
                var ruleset = BeatmapInfo.Value!.Ruleset.CreateInstance();
                var difficultyAdjustMod = ruleset.CreateMod<ModDifficultyAdjust>();
                difficultyAdjustMod!.ReadFromDifficulty(BeatmapInfo.Value.Difficulty);
                SelectedMods.Value = new[] { difficultyAdjustMod };
            });

            AddAssert("circle size bar is white", () => barIsWhiteAt(0));
            AddAssert("HP drain bar is white", () => barIsWhiteAt(1));
            AddAssert("accuracy bar is white", () => barIsWhiteAt(2));
            AddAssert("approach rate bar is white", () => barIsWhiteAt(3));
        }

        [Test]
        public void TestChangedDifficultyAdjustMod()
        {
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(exampleBeatmap));

            AddStep("select changed Difficulty Adjust mod", () =>
            {
                var ruleset = BeatmapInfo.Value!.Ruleset.CreateInstance();
                var difficultyAdjustMod = ruleset.CreateMod<OsuModDifficultyAdjust>();
                var originalDifficulty = BeatmapInfo.Value.Difficulty;

                difficultyAdjustMod!.ReadFromDifficulty(originalDifficulty);
                difficultyAdjustMod.DrainRate.Value = originalDifficulty.DrainRate - 0.5f;
                difficultyAdjustMod.ApproachRate.Value = originalDifficulty.ApproachRate + 2.2f;
                SelectedMods.Value = new[] { difficultyAdjustMod };
            });

            AddAssert("circle size bar is white", () => barIsWhiteAt(0));
            AddAssert("drain rate bar is blue", () => modBarIsBlueAt(1));
            AddAssert("accuracy bar is white", () => barIsWhiteAt(2));
            AddAssert("approach rate bar is red", () => modBarIsRedAt(3));
        }

        [Test]
        public void TestAPIBeatmap()
        {
            AddStep("set beatmap", () => BeatmapInfo.Value = new APIBeatmap
            {
                CircleSize = 7.2f,
                DrainRate = 3,
                OverallDifficulty = 5.7f,
                ApproachRate = 3.5f,
                StarRating = 4.5f,
            });
        }

        private BarStatisticRow barAt(int index) => difficultySettingsContent.ChildrenOfType<BarStatisticRow>().ElementAt(index);

        private bool barIsWhiteAt(int index) => barAt(index).ChildrenOfType<Bar>().ElementAt(1).AccentColour == ColourProvider.Highlight1;
        private bool modBarIsBlueAt(int index) => barAt(index).ChildrenOfType<Bar>().ElementAt(2).AccentColour == colours.BlueDark;
        private bool modBarIsRedAt(int index) => barAt(index).ChildrenOfType<Bar>().ElementAt(2).AccentColour == colours.Red1;
    }
}
