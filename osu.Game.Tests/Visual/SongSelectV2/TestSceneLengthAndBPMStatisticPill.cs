// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneLengthAndBPMStatisticPill : SongSelectComponentsTestScene
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set pill", () => Child = new LengthAndBPMStatisticPill());
        }

        [Test]
        public void TestLocalBeatmap()
        {
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Length = 83000,
                    // Drain not tested
                    // BPM is 60
                    OnlineID = 1,
                },
            }));

            AddStep("set double time", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });

            AddAssert("length value is red", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(0).ValueColour,
                () => Is.EqualTo(colours.ForModType(ModType.DifficultyIncrease)));
            AddAssert("bpm value is red", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(1).ValueColour,
                () => Is.EqualTo(colours.ForModType(ModType.DifficultyIncrease)));

            AddStep("set half time", () => SelectedMods.Value = new[] { new OsuModHalfTime() });

            AddAssert("length value is green", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(0).ValueColour,
                () => Is.EqualTo(colours.ForModType(ModType.DifficultyReduction)));
            AddAssert("bpm value is green", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(1).ValueColour,
                () => Is.EqualTo(colours.ForModType(ModType.DifficultyReduction)));
        }

        [Test]
        public void TestAPIBeatmap()
        {
            AddStep("set beatmap", () => BeatmapInfo.Value = new APIBeatmap
            {
                Length = 83000,
                HitLength = 62000,
                BPM = 321,
                OnlineID = 2,
            });

            AddStep("set double time", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });

            AddAssert("length value color not changed", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(0).ValueColour,
                () => Is.EqualTo(ColourProvider.Content2));
            AddAssert("bpm value color not changed", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(1).ValueColour,
                () => Is.EqualTo(ColourProvider.Content2));

            AddStep("set half time", () => SelectedMods.Value = new[] { new OsuModHalfTime() });

            AddAssert("length value color not changed", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(0).ValueColour,
                () => Is.EqualTo(ColourProvider.Content2));
            AddAssert("bpm value color not changed", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(1).ValueColour,
                () => Is.EqualTo(ColourProvider.Content2));
        }
    }
}
