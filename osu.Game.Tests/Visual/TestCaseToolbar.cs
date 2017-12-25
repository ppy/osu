// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Toolbar;

namespace osu.Game.Tests.Visual
{
    public class TestCaseToolbar : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ToolbarButton),
            typeof(ToolbarModeSelector),
            typeof(ToolbarModeButton),
        };

        public TestCaseToolbar()
        {
            Add(new Toolbar { State = Visibility.Visible });
        }
    }
}
