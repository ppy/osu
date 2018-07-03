// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseLabelledTextBox : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LabelledTextBox),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            LabelledTextBox labelledTextBox;
            Children = new Drawable[]
            {
                labelledTextBox = new LabelledTextBox
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "Testing text",
                    TextBoxPlaceholderText = "This is definitely working as intended",
                    Padding = new MarginPadding { Left = 150, Right = 150 }
                }
            };
        }
    }
}
