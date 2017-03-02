// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;
using osu.Game.Screens.Multiplayer;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseMultiRadioBox : TestCase
    {
        public override string Name => @"MultiRadioButton";
        public override string Description => @"Browsing through Multiplayer Screens";

        public override void Reset()
        {
            base.Reset();

            Add(new MultiRadioBox());
        }
    }
}
