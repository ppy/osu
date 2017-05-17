// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCaseDirect : TestCase
    {
        public override string Description => @"osu!direct overlay";

        private DirectOverlay direct;

        public override void Reset()
        {
            base.Reset();

            Add(direct = new DirectOverlay());

            AddStep(@"Toggle", direct.ToggleVisibility);
        }
    }
}
