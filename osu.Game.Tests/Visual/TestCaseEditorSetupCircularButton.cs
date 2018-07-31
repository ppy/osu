// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
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

        public TestCaseEditorSetupCircularButton()
        {
            SetupCircularButton circularButton;

            Child = circularButton = new SetupCircularButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "Button",
                Action = () => { }
            };

            AddStep("Enable button", () => circularButton.Enabled.Value = true);
            AddStep("Disable button", () => circularButton.Enabled.Value = false);
        }
    }
}
