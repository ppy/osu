// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    public class TestCaseEditorSetupCircularButton : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SetupCircularButton)
        };

        private readonly SetupCircularButton circularButton;

        public TestCaseEditorSetupCircularButton()
        {
            Child = circularButton = new SetupCircularButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                LabelText = "Button",
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            circularButton.DefaultColour = osuColour.Blue;
        }
    }
}
