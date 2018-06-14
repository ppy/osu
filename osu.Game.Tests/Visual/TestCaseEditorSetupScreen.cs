// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Screens.Setup;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;
using osu.Game.Screens.Edit.Screens.Setup.BottomHeaders;
using osu.Game.Screens.Edit.Screens.Setup.Screens;
using osu.Game.Tests.Beatmaps;
using osu.Game.Rulesets;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorSetupScreen : OsuTestCase
    {
        private readonly Setup setup;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Setup),
            typeof(SetupMenuBar),
            typeof(SetupScreenSelectionTabControl),
            typeof(LabelledTextBox),
            typeof(Header),
            typeof(GeneralScreen),
            typeof(DifficultyScreen),
            typeof(AudioScreen),
            typeof(ColoursScreen),
            typeof(DesignScreen),
            typeof(AdvancedScreen),
            typeof(GeneralScreenBottomHeader),
        };

        public TestCaseEditorSetupScreen()
        {
            setup = new Setup(new TestWorkingBeatmap(new RulesetInfo()));
            Child = setup;
            addSteps();
        }

        private void addSteps()
        {
            AddStep("Select General tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.General);
            AddStep("Change artist to \"test artist\"", () => (setup.CurrentScreen as GeneralScreen).ChangeArtist("test artist"));
            AddAssert("Check new artist value", () => setup.CurrentScreen.Beatmap.Value.Metadata.ArtistUnicode == "test artist");
            AddStep("Change romanised artist to \"test romanised artist\"", () => (setup.CurrentScreen as GeneralScreen).ChangeRomanisedArtist("test romanised artist"));
            AddAssert("Check new romanised artist value", () => setup.CurrentScreen.Beatmap.Value.Metadata.Artist == "test romanised artist");
            AddStep("Change title to \"test title\"", () => (setup.CurrentScreen as GeneralScreen).ChangeTitle("test title"));
            AddAssert("Check new title value", () => setup.CurrentScreen.Beatmap.Value.Metadata.TitleUnicode == "test title");
            AddStep("Change romanised title to \"test romanised title\"", () => (setup.CurrentScreen as GeneralScreen).ChangeRomanisedTitle("test romanised title"));
            AddAssert("Check new romanised title value", () => setup.CurrentScreen.Beatmap.Value.Metadata.Title == "test romanised title");
            AddStep("Change difficulty to \"test difficulty\"", () => (setup.CurrentScreen as GeneralScreen).ChangeDifficulty("test difficulty"));
            AddAssert("Check new difficulty value", () => setup.CurrentScreen.Beatmap.Value.Beatmap.BeatmapInfo.Version == "test difficulty");
            AddStep("Change source to \"test source\"", () => (setup.CurrentScreen as GeneralScreen).ChangeSource("test source"));
            AddAssert("Check new source value", () => setup.CurrentScreen.Beatmap.Value.Beatmap.Metadata.Source == "test source");
            AddStep("Change tags to \"test tags\"", () => (setup.CurrentScreen as GeneralScreen).ChangeTags("test tags"));
            AddAssert("Check new tags value", () => setup.CurrentScreen.Beatmap.Value.Beatmap.Metadata.Tags == "test tags");

            AddStep("Select Difficulty tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Difficulty);

            AddStep("Select Audio tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Audio);

            AddStep("Select Colours tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Colours);

            AddStep("Select Design tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Design);

            AddStep("Select Advanced tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Advanced);
        }
    }
}
