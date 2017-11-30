// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Screens.Edit.Screens.Compose.RadioButtons;

namespace osu.Game.Tests.Visual
{
    public class TestCaseEditorComposeRadioButtons : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(DrawableRadioButton) };

        public TestCaseEditorComposeRadioButtons()
        {
            Add(new RadioButtonCollection
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 150,
                Items = new[]
                {
                    new RadioButton("Item 1", () => { }),
                    new RadioButton("Item 2", () => { }),
                    new RadioButton("Item 3", () => { }),
                    new RadioButton("Item 4", () => { }),
                    new RadioButton("Item 5", () => { })
                }
            });
        }
    }
}
