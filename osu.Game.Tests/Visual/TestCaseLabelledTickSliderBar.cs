// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseLabelledSliderBar : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuTickSliderBar),
            typeof(LabelledSliderBar),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new LabelledSliderBar
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "Testing slider bar",
                    BottomLabelText = "Test bottom text",
                    LeftTickCaption = "Left",
                    MiddleTickCaption = "Middle",
                    RightTickCaption = "Right",
                    SliderMinValue = 10,
                    SliderMaxValue = 25,
                    SliderNormalPrecision = 2,
                    SliderAlternatePrecision = 0.1f,
                    Padding = new MarginPadding { Left = 150, Right = 150 }
                }
            };
        }
    }
}
