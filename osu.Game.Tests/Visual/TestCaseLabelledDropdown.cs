// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseLabelledDropdown : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            // Now that's a problem
            typeof(LabelledDropdown<int>),
            typeof(OsuDropdown<int>),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            LabelledDropdown<int> labelledTextBox;
            Children = new Drawable[]
            {
                labelledTextBox = new LabelledDropdown<int>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "A dropdown",
                    Padding = new MarginPadding { Left = 150, Right = 150 }
                }
            };

            labelledTextBox.AddDropdownItems(new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("First", 1),
                new KeyValuePair<string, int>("Second", 2),
                new KeyValuePair<string, int>("Third", 3),
                new KeyValuePair<string, int>("Fourth", 4),
            });

            AddStep("Select the first item", () => labelledTextBox.DropdownSelectedIndex = 0);
            AddStep("Select the second item", () => labelledTextBox.DropdownSelectedIndex = 1);
            AddStep("Select the third item", () => labelledTextBox.DropdownSelectedIndex = 2);
            AddStep("Select the fourth item", () => labelledTextBox.DropdownSelectedIndex = 3);
        }
    }
}
