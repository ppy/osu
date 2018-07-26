// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseLabelledDropdown : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LabelledDropdown<int>),
            typeof(OsuDropdown<int>),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            LabelledDropdown<int> labelledDropdown;
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Padding = new MarginPadding { Left = 150, Right = 150 },
                Child = labelledDropdown = new LabelledDropdown<int>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "A dropdown",
                }
            };

            labelledDropdown.AddDropdownItems(new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("First", 1),
                new KeyValuePair<string, int>("Second", 2),
                new KeyValuePair<string, int>("Third", 3),
                new KeyValuePair<string, int>("Fourth", 4),
            });

            AddStep("Select the first item", () => labelledDropdown.DropdownSelectedIndex = 0);
            AddStep("Select the second item", () => labelledDropdown.DropdownSelectedIndex = 1);
            AddStep("Select the third item", () => labelledDropdown.DropdownSelectedIndex = 2);
            AddStep("Select the fourth item", () => labelledDropdown.DropdownSelectedIndex = 3);
        }
    }
}
