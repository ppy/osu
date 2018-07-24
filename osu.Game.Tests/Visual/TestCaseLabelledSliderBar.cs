// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseLabelledSliderBar : OsuTestCase
    {
        private LabelledSliderBar labelledSliderBar;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SetupTickSliderBar),
            typeof(LabelledSliderBar),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                labelledSliderBar = new LabelledSliderBar
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "Testing slider bar",
                    BottomLabelText = "Test bottom text",
                    LeftTickCaption = "Left",
                    MiddleTickCaption = "Middle",
                    RightTickCaption = "Right",
                    SliderMinValue = 10,
                    SliderMaxValue = 50,
                    SliderNormalPrecision = 5,
                    SliderAlternatePrecision = 0.1f,
                    Padding = new MarginPadding { Left = 150, Right = 150 }
                }
            };

            AddStep("Change slider bar value to 25", () => labelledSliderBar.CurrentValue = 25);
            AddAssert("Check new slider bar value", () => labelledSliderBar.CurrentValue == 25);
            AddStep("Change slider bar value to 27.2", () => labelledSliderBar.CurrentValue = 27.2f);
            AddAssert("Check new binded slider bar value", () => labelledSliderBar.CurrentValue == 25);
            AddSliderStep("Current value", 10f, 50f, 10f, a => labelledSliderBar.CurrentValue = a);
        }
    }
}
