// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseOsuSetupRadioButton : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SetupRadioButton),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            SetupRadioButton osuSetupRadioButton;
            Children = new Drawable[]
            {
                osuSetupRadioButton = new SetupRadioButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "Radio button 1"
                }
            };

            AddStep("Set radio button value to true", () => osuSetupRadioButton.Current.Value = true);
            AddStep("Set radio button value to false", () => osuSetupRadioButton.Current.Value = false);
        }
    }
}
