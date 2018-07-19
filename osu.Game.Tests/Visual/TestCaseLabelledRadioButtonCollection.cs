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
    public class TestCaseLabelledRadioButtonCollection : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LabelledRadioButtonCollection),
            typeof(OsuSetupRadioButtonCollection),
            typeof(OsuSetupRadioButton),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new LabelledRadioButtonCollection
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "Testing collection",
                    BottomLabelText = "This collection contains extraordinary options, such as \"OK!\" and \"No\".",
                    Items = new[]
                    {
                        new OsuSetupRadioButton
                        {
                            LabelText = "OK!"
                        },
                        new OsuSetupRadioButton
                        {
                            LabelText = "No"
                        }
                    },
                    Padding = new MarginPadding { Left = 150, Right = 150 }
                }
            };
        }
    }
}
