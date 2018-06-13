// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Screens.Setup;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;
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
        };

        public TestCaseEditorSetupScreen()
        {
            Setup setup = new Setup(new TestWorkingBeatmap(new RulesetInfo()));
            Child = setup;

            // TODO: Add more test steps for all the actions in the tabs
            AddStep("Select General tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.General);
            AddStep("Select Difficulty tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Difficulty);
            AddStep("Select Audio tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Audio);
            AddStep("Select Colours tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Colours);
            AddStep("Select Design tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Design);
            AddStep("Select Advanced tab", () => setup.MenuBar.Mode.Value = SetupScreenMode.Advanced);
        }
    }
}
